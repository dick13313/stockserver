using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace StockServer.Clawer
{
    public class StockInfoClawer
    {
        private ILogger<StockInfoClawer> _logger;
        private IHttpClientFactory _clientFactory;
        public StockInfoClawer(ILogger<StockInfoClawer> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task ExecuteAsync()
        {
            var html = await GetHtmlAsync();
            var stockInfoList = ParseHtml(html).ToList();
        }

        public async Task<string> GetHtmlAsync()
        {
            using (var client = _clientFactory.CreateClient())
            {
                var response =  await client.GetAsync(
                    $"https://isin.twse.com.tw/isin/class_main.jsp?owncode=&stockname=&isincode=&market=1&issuetype=1&industry_code=&Page=1&chklike=Y"
                );

                using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.GetEncoding(950)))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public IEnumerable<StockInfo> ParseHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableNodes = doc.DocumentNode.SelectNodes("//table[@class=\"h4\"]");
            foreach (var tableNode in tableNodes)
            {
                var trNodes = tableNode.SelectNodes("./tr");
                Dictionary<string, int> headers = new Dictionary<string, int>();
                int trIndex = 0;
                foreach (var trNode in trNodes)
                {
                    //header
                    var tdNodes = trNode.SelectNodes("./td");
                    
                    if (tdNodes != null)
                    {
                        int tdIndex = 0;
                        foreach (var tdNode in tdNodes)
                        {
                            if(trIndex == 0) // header
                            {
                                headers.Add(tdNode.InnerText, tdIndex);
                            }
                            else // data
                            {
                                yield return new StockInfo () {
                                    id = tdNodes[headers["頁面編號"]].InnerText.Trim(),
                                    stock_id = tdNodes[headers["有價證券代號"]].InnerText.Trim(),
                                    stock_name = tdNodes[headers["有價證券名稱"]].InnerText.Trim(),
                                    type = tdNodes[headers["有價證券別"]].InnerText.Trim(),
                                    type_name = tdNodes[headers["市場別"]].InnerText.Trim(),
                                    industry_category = tdNodes[headers["產業別"]].InnerText.Trim(),
                                    note_date = DateTime.Now,
                                };
                            }
                            tdIndex++;
                        }
                    }
                    trIndex++;
                }
            }

        }
    }
}