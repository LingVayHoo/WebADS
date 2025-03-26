using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebADS.Data;
using WebADS.Models;
using WebADS.Models.Token;

namespace WebADS.Services
{
    public class ADSHandler
    {
        private ADSContext _context;
        private MyStorageRequester _requester;
        private MoySkladTokenHandler _handler;
        private MasterToken _masterToken;

        public ADSHandler(ADSContext aDSContext, MyStorageRequester requester, MoySkladTokenHandler handler, MasterToken masterToken)
        {
            _context = aDSContext;
            _requester = requester;
            _handler = handler;
            _masterToken = masterToken;
        }

        public async Task<IEnumerable<AddressDBModel>> GetAddresses(string article)
        {
            var all = await _context.addresses.ToListAsync();
            var filtered = all.Where(model => model.Article == article);

            return filtered;
        }
        

        public async Task<IEnumerable<HistoryFormated>> GetHistoryByArticle(string article)
        {
            // Находим последние 15 записей истории по указанному артикулу
            var historyRecords = await _context.addresseshistory
                .Where(h => h.Article == article)
                .OrderByDescending(h => h.ChangeDate)  // Сортируем по дате изменения в убывающем порядке
                .Take(15)  // Ограничиваем выборку последними 15 записями
                .ToListAsync();

            List<HistoryFormated> formatedData = new List<HistoryFormated>();

            foreach (var historyRecord in historyRecords)
            {
                formatedData.Add(new HistoryFormated(historyRecord));
            }

            return formatedData;  // Возвращаем найденные записи
        }

        public async Task<AddressDBModel?> GetAddressDBModel(Guid id)
        {
            var addressDBModel = await _context.addresses.FindAsync(id);

            return addressDBModel;
        }

        public async Task<Dictionary<string, string>> SearchPlace(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return [];
            }

            // Разделяем строку по '-'
            var parts = searchString.Split('-');

            if (parts.Length != 4)
            {
                return [];
            }

            // Извлекаем значения
            var zone = parts[0];
            var row = parts[1];
            var place = parts[2];
            var level = parts[3];

            // Ищем в базе данных
            var result = await _context.addresses
                .Where(a => a.Zone == zone && a.Row == row && a.Place == place && a.Level == level)
                .ToListAsync();

            if (!result.Any())
            {
                return [];
            }

            Dictionary<string, string> dResult = new Dictionary<string, string>();

            foreach (AddressDBModel e in result)
            {
                dResult.Add(e.ProductID, e.ProductName);
            }

            return dResult;
        }

        public async Task<bool> PutAddressDBModel(AddressHistory addressHistory, string identityUsername)
        {
            var newAddressDBModel = addressHistory.addressDBModel;
            var id = newAddressDBModel.Id;

            // Получаем текущую запись из базы данных
            var oldAddressDBModel = await _context.addresses
                .AsTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (oldAddressDBModel == null)
            {
                return false;
            }

            // Если новая запись помечена как HomePlace, нужно убедиться, что только одна запись с таким Article может иметь IsPrimaryPlace = true
            if (newAddressDBModel.IsPrimaryPlace)
            {
                var conflictingHomePlace = await _context.addresses
                    .FirstOrDefaultAsync(a => a.Article == newAddressDBModel.Article && a.IsPrimaryPlace && a.Id != id);

                if (conflictingHomePlace != null)
                {
                    // Устанавливаем IsPrimaryPlace = false для старой записи
                    conflictingHomePlace.IsPrimaryPlace = false;
                    _context.addresses.Update(conflictingHomePlace);
                    await _context.SaveChangesAsync();
                }
            }

            // Обновляем запись и сохраняем изменения
            try
            {
                var token = _masterToken.Master_token;
                // Обновляем состояние записи
                _context.Entry(oldAddressDBModel).CurrentValues.SetValues(newAddressDBModel);
                _context.Entry(oldAddressDBModel).OriginalValues["RowVersion"] = newAddressDBModel.RowVersion;

                await _context.SaveChangesAsync();

                var locationValue = string.Empty;

                if (newAddressDBModel.IsPrimaryPlace)
                {
                    locationValue = $"{newAddressDBModel.Zone}-{newAddressDBModel.Row}-{newAddressDBModel.Place}-{newAddressDBModel.Level}";
                    
                    await _requester.UpdateExportAttributeAsync(
                        newAddressDBModel.ProductID, locationValue, token);
                }

                // Создаем запись в истории
                var historyRecord = new AddressHistoryDBModel
                {
                    Article = oldAddressDBModel.Article,
                    ChangeType = "Update",
                    OldValues = JsonConvert.SerializeObject(oldAddressDBModel),
                    NewValues = JsonConvert.SerializeObject(newAddressDBModel),
                    ChangeDate = DateTime.UtcNow,
                    ChangedBy = addressHistory.ChangedBy
                };

                _context.addresseshistory.Add(historyRecord);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Обработка исключения конкурентного обновления
                if (!AddressDBModelExists(id))
                {
                    return false;
                }
                else
                {
                    // Логирование и повторная ошибка                    
                    return false;
                }
            }
            catch (Exception)
            {
                // Логирование и повторная ошибка
                return false;
            }

            return true;
        }

        public async Task<bool> PostAddressDBModel(AddressHistory addressHistory, string identityUsername)
        {
            var addressDBModel = addressHistory.addressDBModel;

            // Если это новое место и IsPrimaryPlace равно true, убедитесь, что нет другой записи с таким же Article и IsPrimaryPlace равным true
            if (addressDBModel.IsPrimaryPlace)
            {
                var existingHomePlace = await _context.addresses
                    .FirstOrDefaultAsync(a => a.Article == addressDBModel.Article && a.IsPrimaryPlace);

                if (existingHomePlace != null)
                {
                    // Устанавливаем IsPrimaryPlace = false для существующей записи
                    existingHomePlace.IsPrimaryPlace = false;
                    _context.addresses.Update(existingHomePlace); // Обновляем существующее HomePlace
                }
            }

            // Добавляем новую запись в базу данных
            _context.addresses.Add(addressDBModel);

            try
            {
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных

                var locationValue = string.Empty;

                if (addressDBModel.IsPrimaryPlace)
                {
                    var token = 

                    locationValue = $"{addressDBModel.Zone}-{addressDBModel.Row}-{addressDBModel.Place}-{addressDBModel.Level}";
                    
                    await _requester.UpdateExportAttributeAsync(
                        addressDBModel.ProductID, locationValue, token);
                }

                // Сериализуем новые значения
                var newValuesJson = JsonConvert.SerializeObject(addressDBModel);

                // Создаем запись в истории
                var historyRecord = new AddressHistoryDBModel
                {
                    Article = addressDBModel.Article,
                    ChangeType = "Create",  // Тип изменения: Создание
                    OldValues = string.Empty,       // Старых значений нет, так как это новая запись
                    NewValues = newValuesJson,
                    ChangeDate = DateTime.UtcNow,
                    ChangedBy = addressHistory.ChangedBy    // Или передавай текущего пользователя
                };

                // Добавляем запись истории в базу данных
                _context.addresseshistory.Add(historyRecord);
                await _context.SaveChangesAsync(); // Сохраняем изменения в истории
            }
            catch (DbUpdateConcurrencyException)
            {
                // Обработка исключения конкурентного обновления
                return false;
            }
            catch (Exception)
            {
                // Логирование и обработка общей ошибки
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteAddressDBModel(Guid id, [FromQuery] string changedBy)
        {
            // Получаем запись для удаления
            var addressDBModel = await _context.addresses.FindAsync(id);
            if (addressDBModel == null)
            {
                return false;
            }

            // Сериализуем старые значения перед удалением
            var oldValuesJson = JsonConvert.SerializeObject(addressDBModel);

            // Удаляем запись
            _context.addresses.Remove(addressDBModel);
            await _context.SaveChangesAsync();

            // Создаем запись в истории
            var historyRecord = new AddressHistoryDBModel
            {
                Article = addressDBModel.Article,
                ChangeType = "Delete",  // Тип изменения: Удаление
                OldValues = oldValuesJson,
                NewValues = string.Empty,       // Нет новых значений, так как запись удалена
                ChangeDate = DateTime.UtcNow,
                ChangedBy = changedBy   // Или передавай текущего пользователя
            };

            // Добавляем запись истории в базу данных
            _context.addresseshistory.Add(historyRecord);
            await _context.SaveChangesAsync(); // Сохраняем изменения в истории

            return true;
        }

        private bool AddressDBModelExists(Guid id)
        {
            return _context.addresses.Any(e => e.Id == id);
        }
    }
}
