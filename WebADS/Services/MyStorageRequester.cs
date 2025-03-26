using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using WebADS.Models.Token;

namespace WebADS.Services
{
    public class MyStorageRequester
    {
        private readonly HttpClient _client;

        private readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1); // Ограничиваем доступ к очереди
        private readonly SemaphoreSlim _globalSemaphore; // Ограничение параллельных запросов для аккаунта
        private readonly SemaphoreSlim _userSemaphore; // Ограничение параллельных запросов для пользователя
        private readonly SemaphoreSlim _taskQueueSemaphore; // Ограничение асинхронных задач в очереди
        private readonly int _requestsPer3Seconds; // Количество запросов за 3 секунды
        private readonly Queue<DateTime> _requestTimestamps; // Для отслеживания времени запросов
        private readonly int _maxHeaderSize; // Максимальный размер заголовков
        private readonly int _maxRequestSize; // Максимальный размер данных в одном запросе

        public MyStorageRequester(
            HttpClient client,
            int requestsPer3Seconds = 45,
            int maxHeaderSize = 8192, // 8 KB
            int maxRequestSize = 20 * 1024 * 1024, // 20 MB
            int maxUserParallelRequests = 5,
            int maxGlobalParallelRequests = 20,
            int maxQueuedTasks = 4)
        {
            _client = client;

            _requestsPer3Seconds = requestsPer3Seconds;
            _maxHeaderSize = maxHeaderSize;
            _maxRequestSize = maxRequestSize;

            _requestTimestamps = new Queue<DateTime>();

            _globalSemaphore = new SemaphoreSlim(maxGlobalParallelRequests, maxGlobalParallelRequests);
            _userSemaphore = new SemaphoreSlim(maxUserParallelRequests, maxUserParallelRequests);
            _taskQueueSemaphore = new SemaphoreSlim(maxQueuedTasks, maxQueuedTasks);
        }

        // Метод для получения токена с использованием учетных данных
        public async Task<string> FetchTokenAsync(string username, string password)
        {
            string url = "https://api.moysklad.ru/api/remap/1.2/security/token";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                HttpResponseMessage response = await client.PostAsync(url, null);

                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty; // Возвращаем пустую строку, если ответ не успешный
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                Token accessToken = JsonConvert.DeserializeObject<Token>(responseBody) ?? new Token();

                if (!string.IsNullOrEmpty(accessToken?.access_token))
                {
                    return accessToken.access_token;
                }
            }

            return string.Empty;
        }

        public async Task<HttpResponseMessage> GetResponseWithoutRedirectsAsync(string requestUrl, string token)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false // Отключение автоматического следования за перенаправлениями
            };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                HttpResponseMessage response = await client.GetAsync(requestUrl);

                return response;
            }
        }

        public async Task<HttpResponseMessage> CheckToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = string.Empty;
            };
            string url = $"https://api.moysklad.ru/api/remap/1.2/entity/product?search=checkdata";

            ValidateHeaders(url, token);

            // Установка заголовков
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            HttpResponseMessage response = await _client.GetAsync(url);
            return response;
        }

        public async Task<string> GetRequestAsync(string url, string token)
        {
            ValidateHeaders(url, token);

            await _taskQueueSemaphore.WaitAsync();
            try
            {
                await EnforceRateLimitAsync();
                await _globalSemaphore.WaitAsync();
                await _userSemaphore.WaitAsync();

                try
                {
                    // Установка заголовков
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                    HttpResponseMessage response = await _client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // Обработка ответа
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var decompressedStream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress))
                    using (var reader = new System.IO.StreamReader(decompressedStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                catch(Exception ex)
                {
                    return string.Empty;
                }
                finally
                {
                    _userSemaphore.Release();
                    _globalSemaphore.Release();
                }
            }
            finally
            {
                _taskQueueSemaphore.Release();
            }
        }

        public async Task<HttpResponseMessage> PostRequestAsync(
            string? jsonData,
            string url,
            string token,
            string scheme = "Bearer")
        {
            ValidateHeaders(url, token);
            ValidateRequestSize(jsonData);

            await _taskQueueSemaphore.WaitAsync();
            try
            {
                await EnforceRateLimitAsync();
                await _globalSemaphore.WaitAsync();
                await _userSemaphore.WaitAsync();

                try
                {
                    // Установка заголовков
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
                    _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                    // Создание контента запроса
                    StringContent? content = null;
                    if (jsonData != null)
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");


                    HttpResponseMessage response = await _client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    return response;
                }
                finally
                {
                    _userSemaphore.Release();
                    _globalSemaphore.Release();
                }
            }
            finally
            {
                _taskQueueSemaphore.Release();
            }
        }

        public async Task<byte[]> GetByteArrayAsync(string directUrl)
        {
            var imageBytes = await _client.GetByteArrayAsync(directUrl);
            return imageBytes;
        }

        public async Task<HttpResponseMessage> GetSimpleRequest(string url, string token)
        {
            try
            {
                // Установка заголовков
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                // Выполнение GET-запроса
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
                HttpResponseMessage response = await _client.GetAsync(url);
                return response;

            }
            catch (HttpRequestException)
            {
                throw; // Перебрасываем исключение дальше
            }
            catch (Exception)
            {
                throw; // Перебрасываем исключение дальше
            }
        }

        private async Task EnforceRateLimitAsync()
        {
            await _rateLimitSemaphore.WaitAsync(); // Захватываем семафор для синхронизации
            try
            {
                DateTime now = DateTime.UtcNow;

                // Удаляем устаревшие запросы (старше 3 секунд)
                while (_requestTimestamps.Count > 0 && (now - _requestTimestamps.Peek()).TotalSeconds > 3)
                {
                    _requestTimestamps.Dequeue();
                }

                // Если запросов в очереди больше или равно лимиту, ждем
                if (_requestTimestamps.Count >= _requestsPer3Seconds)
                {
                    var delayTime = TimeSpan.FromSeconds(3) - (now - _requestTimestamps.Peek());
                    await Task.Delay(delayTime); // Асинхронное ожидание
                }

                // Добавляем текущий запрос в очередь
                _requestTimestamps.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                _rateLimitSemaphore.Release(); // Освобождаем семафор
            }
        }

        private void ValidateHeaders(string url, string token)
        {
            var headerSize = Encoding.UTF8.GetByteCount(url) + Encoding.UTF8.GetByteCount(token) + 100; // Примерный расчет
            if (headerSize > _maxHeaderSize)
            {
                throw new InvalidOperationException($"Размер заголовков превышает {_maxHeaderSize} байт.");
            }
        }

        private void ValidateRequestSize(string? data)
        {
            if (data == null) return;
            if (Encoding.UTF8.GetByteCount(data) > _maxRequestSize)
            {
                throw new InvalidOperationException($"Размер данных превышает {_maxRequestSize} байт.");
            }
        }

        public async Task<HttpResponseMessage> UpdateExportAttributeAsync(string productId, string locationValue, string token)
        {
            try
            {
                // URL для изменения продукта
                var url = $"https://api.moysklad.ru/api/remap/1.2/entity/product/{productId}";

                // Установка заголовков
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                // Данные для обновления атрибута "Экспорт"
                var requestBody = new
                {
                    attributes = new[]
                    {
                        new
                        {
                            meta = new
                            {
                                href = "https://api.moysklad.ru/api/remap/1.2/entity/product/metadata/attributes/5b0a23e8-7b1c-11ef-0a80-0f2e000c2cd1",
                                type = "attributemetadata",
                                mediaType = "application/json"
                            },
                            value = locationValue // Изменение значения атрибута "Экспорт"
                        }
                    }
                };

                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Выполнение PUT-запроса
                HttpResponseMessage response = await _client.PutAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (HttpRequestException)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
