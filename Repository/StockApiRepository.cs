using Microsoft.Data.SqlClient;
using Dapper;
using System.Transactions;
using Dapper.Contrib.Extensions;
using StockServer.Models.DataBase;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;

namespace StockServer.Repository
{
    public class StockApiRepository
    {
        private readonly ILogger<StockApiRepository> _logger;
        private IHttpClientFactory _clientFactory;
        public StockApiRepository(ILogger<StockApiRepository> logger, IHttpClientFactory clientFactory) 
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public IEnumerable<Stock> GetStocks(string stock_id, DateTime stateDate, DateTime endDate)
        {
            using(var client = _clientFactory.CreateClient())
            {
                Dictionary<string,string> datas = new Dictionary<string, string>() {
                    { "stock_id" , "2330" },
                    { "start_date", "2019-01-01"},
                    { "end_date", "2020-04-13"}                 
                };
                // client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                
                string s = JsonSerializer.Serialize(datas);
                var response = client.PostAsync(
                    "http://35.236.179.186/api/Stock/Query", 
                    new StringContent(JsonSerializer.Serialize(datas), System.Text.Encoding.UTF8, "application/json")
                ).GetAwaiter().GetResult();
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stock>>(result);
                }
                else {
                    _logger.LogWarning("http error");
                    return null;
                }
            }
        }
    }
}