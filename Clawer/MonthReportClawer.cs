using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using StockServer.CsvMap;
using StockServer.Models.DataBase;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class MonthReportClawer
    {

        private MonthReportRepository _monthReportRepository;
        private ILogger<MonthReportClawer> _logger;
        private IHttpClientFactory _clientFactory;
        public MonthReportClawer(ILogger<MonthReportClawer> logger, MonthReportRepository monthReportRepository, IHttpClientFactory clientFactory) 
        {
            _monthReportRepository = monthReportRepository;
            _logger = logger;
            _clientFactory = clientFactory;
        }
        public async Task ExecuteAsync(int year, int month)
        {
             try
            {
                _logger.LogInformation("MonthReportClawer Execute Start");
                if(_monthReportRepository.IsExist(year, month)) {
                    _logger.LogDebug($"{year}, {month} data is exist");
                    return;
                }
                _logger.LogInformation($"MonthReportClawer Running, year = {year}, month = {month}");
                string data = await ReadMonthReportCsvByTWSEAsync(year, month);
                var reports = ReadCsv(data).Select(report => {
                    report.year = year;
                    report.month = month;
                    return report;
                });
                _monthReportRepository.Insert(reports);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"MonthReportClawer error\n{ex.Message}");
            }
        }

        public async Task<string> ReadMonthReportCsvByTWSEAsync(int year, int month)
        {
            using(var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = await client.PostAsync(
                    "https://mops.twse.com.tw/server-java/FileDownLoad", 
                    new StringContent($"step=9&functionName=show_file&filePath=%2Fhome%2Fhtml%2Fnas%2Ft21%2Fsii%2F&fileName=t21sc03_{year}_{month}.csv")
                );
                var result = await response.Content.ReadAsStringAsync();
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取財報資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<MonthReport> ReadCsv(string data)
        {
            using (var reader = new StringReader(data))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvReader.Configuration.RegisterClassMap<MonthReportMap>();
                return csvReader.GetRecords<MonthReport>().ToList();
            }
        }
    }
}