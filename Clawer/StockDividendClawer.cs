using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class StockDividendClawer
    {
        private ILogger<StockDividendClawer> _logger;
        private IHttpClientFactory _clientFactory;
        private StockDividendRepository _stockDividendRepository;
        public StockDividendClawer(ILogger<StockDividendClawer> logger, IHttpClientFactory clientFactory, StockDividendRepository stockDividendRepository)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _stockDividendRepository = stockDividendRepository;
        }

        //https://mops.twse.com.tw/mops/web/t05st09_2
        public async Task ExecuteAsync(string stock_id)
        {
            var html = await GetHtmlAsync(stock_id);
            var stockDividendList = ParseHtml(html, stock_id).Where(stockDividend => !_stockDividendRepository.IsExist(stockDividend));
            _stockDividendRepository.Insert(stockDividendList);
        }

        public async Task<string> GetHtmlAsync(string stock_id)
        {
            using (var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = await client.PostAsync(
                    "https://mops.twse.com.tw/mops/web/ajax_t05st09_2", 
                    new FormUrlEncodedContent(
                        new [] {
                            new KeyValuePair<string,string>("encodeURIComponent", "1"),
                            new KeyValuePair<string,string>("step", "1"),
                            new KeyValuePair<string,string>("firstin", "1"),
                            new KeyValuePair<string,string>("off", "1"),
                            new KeyValuePair<string,string>("keyword4", ""),
                            new KeyValuePair<string,string>("code1", ""),
                            new KeyValuePair<string,string>("TYPEK2", ""),
                            new KeyValuePair<string,string>("checkbtn", ""),
                            new KeyValuePair<string,string>("queryName", "co_id"),
                            new KeyValuePair<string,string>("inpuType", "co_id"),
                            new KeyValuePair<string,string>("TYPEK", "all"),
                            new KeyValuePair<string,string>("isnew", "false"),
                            new KeyValuePair<string,string>("co_id", stock_id),
                            new KeyValuePair<string,string>("date1", "100"),
                            new KeyValuePair<string,string>("date2", (DateTime.Now.Year - 1911).ToString()),
                            new KeyValuePair<string,string>("qryType", "1"),
                        }
                    )
                );
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = Encoding.GetEncoding("utf-8").GetString(bytes);
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取股息資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<StockDividend> ParseHtml(string html, string stock_id)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableNodes = doc.DocumentNode.SelectNodes("//table[@class=\"hasBorder\"]");
            foreach (var tableNode in tableNodes)
            {
                var trNodes = tableNode.SelectNodes("./tr");
                int trIndex = 0;
                Dictionary<string, int> headers = new Dictionary<string, int>();

                // header
                var thNodes_0 = trNodes[0].SelectNodes("./th");
                var thNodes_1 = trNodes[1].SelectNodes("./th");
                int headerIndex = 0;
                foreach (var thNode_0 in thNodes_0)
                {
                    if(thNode_0.InnerText != "股東配發內容")
                        headers.Add(thNode_0.InnerText, headerIndex++);
                    else {
                        foreach (var thNode_1 in thNodes_1)
                        {
                            headers.Add(thNode_1.InnerText.Replace(" ",""), headerIndex++);
                        }
                    }
                }
                
                foreach (var trNode in trNodes)
                {
                    //data
                    var tdNodes = trNode.SelectNodes("./td");
                    if (tdNodes != null)
                    {
                        string timeString = tdNodes[headers["股利所屬年(季)度"]].InnerText.Replace("&nbsp;","");
                        Regex regex = new Regex("([0-9]*)年(第([1-4])季|年度)", RegexOptions.Singleline);
                        var matchs = regex.Match(timeString);

                        int year;
                        int? season = null;
                        if(matchs.Groups.Count >= 2)
                            year = Convert.ToInt32(matchs.Groups[1].Value);
                        else
                            throw new Exception("Not Find year Data");
                        if(matchs.Groups.Count >= 4 && matchs.Groups[3].Value != "")
                            season = Convert.ToInt32(matchs.Groups[3].Value);

                        var stockDividend = new StockDividend()
                        {
                            stock_id = stock_id,
                            time_string = timeString,
                            year = year,
                            season = season,
                            dividend = Convert.ToDecimal(tdNodes[headers["盈餘分配之現金股利(元/股)"]].InnerText.Replace("&nbsp;",""))
                        };
                        yield return stockDividend;
                    }
                    trIndex++;
                }
            }
        }
    }
}