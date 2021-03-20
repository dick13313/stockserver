using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using StockServer.CsvMap;
using StockServer.Models.DataBase;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class FundamentalDailyClawer
    {
        private ILogger<FundamentalDailyClawer> _logger;
        private IHttpClientFactory _clientFactory;
        private FundamentalDailyRepository _fundamentalDailyRepository;
        public FundamentalDailyClawer(
            ILogger<FundamentalDailyClawer> logger, 
            IHttpClientFactory clientFactory, 
            FundamentalDailyRepository fundamentalDailyRepository) 
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _fundamentalDailyRepository = fundamentalDailyRepository;
        }
        public async Task ExecuteAsync(DateTime date)
        {
            var csvString = await GetCsvAsync(date.Date);
            var fundamentalDailyList = ReadCsv(csvString).Select(item => {
                item.date = date.Date;
                return item;
            });
            _fundamentalDailyRepository.Insert(fundamentalDailyList);
        }

        public async Task<string> GetCsvAsync(DateTime date)
        {
            using(var client = _clientFactory.CreateClient())
            {
                var response = await client.GetAsync(
                    $"https://www.twse.com.tw/exchangeReport/BWIBBU_d?response=csv&date={date.ToString("yyyyMMdd")}&selectType=ALL"
                );
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = Encoding.GetEncoding(950).GetString(bytes);
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取每日基本面資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<FundamentalDaily> ReadCsv(string data)
        {
            using (var reader = new StringReader(data))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvReader.Configuration.RegisterClassMap<FundamentalDailyMap>();
                while (csvReader.Read())
                {
                    FundamentalDaily fundamentalDaily = null;
                    try
                    {
                        if(!Regex.IsMatch(csvReader.Context.RawRecord,".*,\r\n")) // 過濾不正常資料
                            continue;
                        fundamentalDaily = csvReader.GetRecord<FundamentalDaily>();
                    }
                    catch (CsvHelper.TypeConversion.TypeConverterException ex)
                    {
                        _logger.LogDebug(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex.Message);
                    }
                    if(fundamentalDaily != null)
                        yield return fundamentalDaily;
                }
            }
        }
    }
}