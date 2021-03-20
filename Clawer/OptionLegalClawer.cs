using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using StockServer.Models.DataBase;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;
using StockServer.Models.Query;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class OptionLegalClawer
    {
        private readonly ILogger<OptionLegalClawer> _logger;
        private OptionLegalRepository _optionLegalRepository;
        private IHttpClientFactory _clientFactory;
        public OptionLegalClawer(ILogger<OptionLegalClawer> logger, OptionLegalRepository optionLegalRepository, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _optionLegalRepository = optionLegalRepository;
            _clientFactory = clientFactory;
        }

        public async Task ExecuteAsync(OptionLegalQuery optionLegalQuery)
        {
            var csvString = await GetOptionLegalCsvByTaifexAsync(optionLegalQuery);
            var csvList = ReadCsv(csvString).Where(csv => !_optionLegalRepository.IsExist(csv));
            _optionLegalRepository.Insert(csvList);
        }

        public async Task<string> GetOptionLegalCsvByTaifexAsync(OptionLegalQuery optionLegalQuery)
        {
            using(var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = await client.PostAsync(
                    "https://www.taifex.com.tw/cht/3/dlFutContractsDateDown", 
                    new FormUrlEncodedContent(
                        new [] {
                            new KeyValuePair<string,string>("firstDate", DateTime.Now.AddYears(-3).ToString("yyyy/MM/dd HH:mm")),
                            new KeyValuePair<string,string>("lastDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm")),
                            new KeyValuePair<string,string>("queryStartDate", optionLegalQuery.queryStartDate.ToString("yyyy/MM/dd")),
                            new KeyValuePair<string,string>("queryEndDate", optionLegalQuery.queryEndDate.ToString("yyyy/MM/dd")),
                            new KeyValuePair<string,string>("commodityId", optionLegalQuery.optionType.ToString()),
                        }
                    )
                );
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = Encoding.GetEncoding(950).GetString(bytes);
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取期貨法人買賣超資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<OptionLegal> ReadCsv(string data)
        {
            using (var reader = new StringReader(data))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csvReader.GetRecords<OptionLegal>().ToList();
            }
        }
    }
}