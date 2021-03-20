using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class StockHolderClawer
    {
        private ILogger<StockHolderClawer> _logger;
        private StockHolderRepository _stockHolderRepository;
        private IHttpClientFactory _clientFactory;
        public StockHolderClawer(ILogger<StockHolderClawer> logger, StockHolderRepository stockHolderRepository, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _stockHolderRepository = stockHolderRepository;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// 爬最新一週資料
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync() 
        {
            try
            {
                var csvString = await GetCsvAsync();
                var stockHolderList = ReadCsv(csvString).Where(x => x.holder_level == "total" || Convert.ToInt32(x.holder_level) <= 15);
                _stockHolderRepository.Insert(stockHolderList);
            }
            catch(Exception ex)
            {
                _logger.LogWarning($"StockHolderClawer error\n{ex.Message}");
            }
        }

        public async Task ExecuteAsync(string stock_id, string dateString) 
        {
            try
            {
                var html = await GetStockHolderHtmlByTDCCAsync(stock_id, dateString);
                var stockHolderList = ParseStockHolderHtml(html, stock_id, dateString);
                _stockHolderRepository.Insert(stockHolderList);
            }
            catch(Exception ex)
            {
                _logger.LogWarning($"StockHolderClawer error\n{ex.Message}");
            }
        }

        public async Task<string> GetCsvAsync()
        {
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.GetAsync("https://smart.tdcc.com.tw/opendata/getOD.ashx?id=1-5");
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = Encoding.GetEncoding("UTF-8").GetString(bytes);

                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取股權分散表整週資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public async Task<IEnumerable<string>> GetDateStringListByTDCCAsync()
        {
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.PostAsync(
                    "https://www.tdcc.com.tw/smWeb/QryStockAjax.do",
                    new FormUrlEncodedContent(
                        new [] {
                            new KeyValuePair<string,string>("REQ_OPR","qrySelScaDates")
                        }
                    ));
                var result = await response.Content.ReadAsStringAsync();
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取集保戶股權日期資料...，{response.StatusCode}，{result}");
                return JsonSerializer.Deserialize<List<string>>(result);
            }
        }

        public async Task<string> GetStockHolderHtmlByTDCCAsync(string stock_id, string dateString)
        {
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.PostAsync(
                    "https://www.tdcc.com.tw/smWeb/QryStockAjax.do",
                    new FormUrlEncodedContent(
                        new [] {
                            new KeyValuePair<string,string>("scaDates", dateString),
                            new KeyValuePair<string,string>("scaDate", dateString),
                            new KeyValuePair<string,string>("SqlMethod", "StockNo"),
                            new KeyValuePair<string,string>("StockNo", stock_id),
                            new KeyValuePair<string,string>("radioStockNo", stock_id),
                            new KeyValuePair<string,string>("StockName", ""),
                            new KeyValuePair<string,string>("REQ_OPR", "SELECT"),
                            new KeyValuePair<string,string>("clkStockNo", stock_id),
                            new KeyValuePair<string,string>("clkStockName", "")
                            //scaDates=20190607&scaDate=20190607&SqlMethod=StockNo&StockNo=0050&radioStockNo=0050&StockName=&REQ_OPR=SELECT&clkStockNo=0050&clkStockName=
                        }
                    ));
                var result = await response.Content.ReadAsStringAsync();
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取集保戶股權資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<StockHolder> ParseStockHolderHtml(string html, string stock_id, string dateString)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableNodes = doc.DocumentNode.SelectNodes("//table[@class=\"mt\"]");
            var trNodes = tableNodes[1].SelectNodes("./tbody/tr");
            Dictionary<string, int> headerDictionary = new Dictionary<string, int>();
            List<StockHolder> stockHolderList = new List<StockHolder>();
            for(int trIndex=0; trIndex < trNodes.Count; trIndex++)
            {
                if(trIndex == 0) //header
                {
                    var tdNodes = trNodes[trIndex].SelectNodes("./td");
                    for(int tdIndex=0; tdIndex < tdNodes.Count; tdIndex++)
                    {
                        headerDictionary.Add(tdNodes[tdIndex].InnerText.Replace("　",""), tdIndex);
                    }
                }
                else //data
                {
                    var tdNodes = trNodes[trIndex].SelectNodes("./td");
                    
                    if(tdNodes[0].InnerText == "無此資料")
                        throw new Exception("無此資料");
                    string level;
                    if(tdNodes[headerDictionary["持股/單位數分級"]].InnerText.Replace("　","") == "合計")
                        level = "total";
                    else
                        level = tdNodes[headerDictionary["序"]].InnerText;

                    if(level == "total" || Convert.ToInt32(level) <= 15)
                    {
                        yield return  new StockHolder() {
                            stock_id = stock_id,
                            date_string = dateString,
                            holder_level = level,
                            people_count = Convert.ToInt32(tdNodes[headerDictionary["人數"]].InnerText.Replace(",","")),
                            stock_holder_count = tdNodes[headerDictionary["股數/單位數"]].InnerText.Replace(",",""),
                        };
                    }
                }
            }
        }

        public IEnumerable<StockHolder> ReadCsv(string data)
        {
            using (var reader = new StringReader(data))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // return csvReader.GetRecords<StockHolder>().ToList();
                while (csvReader.Read())
                {
                    StockHolder stockHolder = null;
                    try
                    {
                        stockHolder = csvReader.GetRecord<StockHolder>();
                    }
                    catch (CsvHelper.TypeConversion.TypeConverterException ex)
                    {
                        _logger.LogDebug(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex.Message);
                    }
                    if(stockHolder != null)
                        yield return stockHolder;
                }
            }
        }
    }
}