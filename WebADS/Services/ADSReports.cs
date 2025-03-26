using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using WebADS.Data;
using WebADS.Models;

namespace WebADS.Services
{
    [Authorize]
    public class ADSReports
    {
        private readonly ADSContext _adsContext;

        public ADSReports(ADSContext adsContext)
        {
            _adsContext = adsContext;
        }

        public async Task<FileResult> GetADSReportExcelAsync(string storeID, string zone)
        {
            // Получаем данные из базы
            var data = await GetADSReportAsync(storeID, zone);

            // Создаем поток для Excel-файла
            var stream = new MemoryStream();

            // Настройка контекста лицензии EPPlus (требуется для версии 5+)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Addresses");

                // Заголовки колонок
                string[] headers = { "Склад", "Артикул", "Наименование", "Зона", "Ряд", "Место", "Уровень", "Кол-во/Вместимость", "Основное", "Домашнее" };

                for (int col = 0; col < headers.Length; col++)
                {
                    worksheet.Cells[1, col + 1].Value = headers[col];
                }

                // Форматирование заголовков
                var headerRange = worksheet.Cells[1, 1, 1, headers.Length];
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                headerRange.Style.Font.Bold = true; // Жирный шрифт
                headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Заполняем данные
                for (int i = 0; i < data.Length; i++)
                {
                    var row = data[i];
                    worksheet.Cells[i + 2, 1].Value = row.StoreID;
                    worksheet.Cells[i + 2, 2].Value = row.Article;
                    worksheet.Cells[i + 2, 3].Value = row.ProductName;
                    worksheet.Cells[i + 2, 4].Value = row.Zone;
                    worksheet.Cells[i + 2, 5].Value = row.Row;
                    worksheet.Cells[i + 2, 6].Value = row.Place;
                    worksheet.Cells[i + 2, 7].Value = row.Level;
                    worksheet.Cells[i + 2, 8].Value = row.Qty;
                    worksheet.Cells[i + 2, 9].Value = row.IsPrimaryPlace ? "Да" : "Нет";
                    worksheet.Cells[i + 2, 10].Value = row.IsSalesLocation ? "Да" : "Нет";
                }

                // Автонастройка ширины колонок (проверяем, что данные есть)
                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                // Сохраняем Excel в поток
                package.SaveAs(stream);
            }

            // Возвращаем файл
            stream.Position = 0; // Сбрасываем позицию потока
            return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Addresses_{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }


        public async Task<Models.AddressDBModel[]> GetADSReportAsync(string storeID, string zone)
        {
            try
            {
                // Создаем базовый запрос
                var query = _adsContext.addresses.AsQueryable();

                // Применяем фильтрацию по storeID, если он не равен "all"
                if (!string.IsNullOrEmpty(storeID) && storeID != "all")
                {
                    query = query.Where(a => a.StoreID == storeID);
                }

                // Применяем фильтрацию по zone, если он не равен "all"
                if (!string.IsNullOrEmpty(zone) && zone != "all")
                {
                    query = query.Where(a => a.Zone == zone);
                }

                // Выполняем асинхронный запрос и возвращаем результат
                var report = await query.ToArrayAsync();
                return report;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<AddressDBModel>> GetAddressesByPlace(
            string storeID,
            string zone,
            string row,
            string place,
            string level)
        {
            var query = _adsContext.addresses.AsQueryable();

            if (storeID != "all")
            {
                query = query.Where(a => a.StoreID == storeID);
            }

            if (zone != "all")
            {
                query = query.Where(a => a.Zone == zone);
            }

            if (row != "all")
            {
                query = query.Where(a => a.Row == row);
            }

            if (place != "all")
            {
                query = query.Where(a => a.Place == place);
            }

            if (level != "all")
            {
                query = query.Where(a => a.Level == level);
            }

            var result = await query.OrderBy(value => value).ToListAsync();

            return result ?? new List<AddressDBModel>();
        }


        public async Task<List<string>> GetUniqueValuesAsync(
            Expression<Func<AddressDBModel, string>> selector,
            string storeID, 
            string? zone = null, 
            string? row = null, 
            string? place = null)
        {
            var query = _adsContext.addresses.AsQueryable();

            // Применяем фильтрацию по переданным параметрам
            if (!string.IsNullOrEmpty(storeID) && storeID != "all")
                query = query.Where(a => a.StoreID == storeID);

            if (!string.IsNullOrEmpty(zone) && zone != "all")
                query = query.Where(a => a.Zone == zone);

            if (!string.IsNullOrEmpty(row) && row != "all")
                query = query.Where(a => a.Row == row);

            if (!string.IsNullOrEmpty(place) && place != "all")
                query = query.Where(a => a.Place == place);

            // Фильтруем пустые значения, выбираем уникальные и сортируем
            return await query
                    .Where(selector.NotNullOrEmpty()) // Фильтрация null и пустых строк на уровне SQL
                    .Select(selector)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToListAsync();
        }

        public async Task<List<string>> GetUniqueValuesAsync(string storeID, params string[] filters)
        {
            if (filters.Length == 3 && !string.IsNullOrEmpty(filters[2]))
                return await GetUniqueValuesAsync(selector: a => a.Level, storeID, filters[0], filters[1], filters[2]); // Уровень
            else if (filters.Length == 2 && !string.IsNullOrEmpty(filters[1]))

                return await GetUniqueValuesAsync(selector: a => a.Place, storeID, filters[0], filters[1]); // Место
            
            else if (filters.Length == 1 && !string.IsNullOrEmpty(filters[0]))
                return await GetUniqueValuesAsync(selector: a => a.Row, storeID, filters[0]); // Ряд
                                                                                              
            else if (!string.IsNullOrEmpty(storeID))
                return await GetUniqueValuesAsync(selector: a => a.Zone, storeID); // Зона
            
            else return [];        
        }

        //// Методы-обёртки для удобства
        //public Task<List<string>> GetUniqueZonesAsync(string storeID) =>
        //    GetUniqueValuesAsync(selector: a => a.Zone, storeID);

        //public Task<List<string>> GetUniqueRowsAsync(string storeID, string zone) =>
        //    GetUniqueValuesAsync(selector: a => a.Row, storeID,  zone);

        //public Task<List<string>> GetUniquePlacesAsync(string storeID, string zone, string row) =>
        //    GetUniqueValuesAsync(selector: a => a.Place, storeID, zone, row);

        //public Task<List<string>> GetUniqueLevelsAsync(string storeID, string zone, string row, string place) =>
        //    GetUniqueValuesAsync(selector: a => a.Level, storeID, zone, row, place);
    }
}
