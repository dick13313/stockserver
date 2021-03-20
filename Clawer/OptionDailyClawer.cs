using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;
using StockServer.Models.Query;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class OptionDailyClawer
    {
        private readonly ILogger<OptionDailyClawer> _logger;
        private OptionDailyRepository _optionDailyRepository;
        private IHttpClientFactory _clientFactory;
        public OptionDailyClawer(ILogger<OptionDailyClawer> logger, OptionDailyRepository optionDailyRepository, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _optionDailyRepository = optionDailyRepository;
            _clientFactory = clientFactory;
        }

        public async Task ExecuteAsync(OptionDailyQuery optionDailyQuery)
        {
            try
            {
                var csvString = await GetOptionDailyCsvByTaifexAsync(optionDailyQuery);
                var csvList = ReadCsv(csvString).ToList().Where(csv => !_optionDailyRepository.IsExist(csv));
                _optionDailyRepository.Insert(csvList);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task<string> GetOptionDailyCsvByTaifexAsync(OptionDailyQuery optionDailyQuery)
        {
            using(var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = await client.PostAsync(
                    "https://www.taifex.com.tw/cht/3/dlFutDataDown", 
                    new FormUrlEncodedContent(
                        new [] {
                            new KeyValuePair<string,string>("down_type", "1"),
                            new KeyValuePair<string,string>("commodity_id", optionDailyQuery.optionType.ToString()),
                            new KeyValuePair<string,string>("commodity_id2", ""),
                            new KeyValuePair<string,string>("queryStartDate", optionDailyQuery.queryStartDate.ToString("yyyy/MM/dd")),
                            new KeyValuePair<string,string>("queryEndDate", optionDailyQuery.queryEndDate.ToString("yyyy/MM/dd")),
                        }
                    )
                );
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = Encoding.GetEncoding(950).GetString(bytes);
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取期貨資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<OptionDaily> ReadCsv(string data)
        {
            using (var reader = new StringReader(data))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csvReader.Read())
                {
                    OptionDaily optionDaily = null;
                    try
                    {
                        optionDaily = csvReader.GetRecord<OptionDaily>();
                        optionDaily.expired = optionDaily.expired.Trim();
                    }
                    catch (CsvHelper.TypeConversion.TypeConverterException ex)
                    {
                        _logger.LogDebug(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex.Message);
                    }
                    if(optionDaily != null)
                        yield return optionDaily;
                }
            }
        }
    }
}