using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using StockServer.Repository;
using StockServer.Models.DataBase;
// using Newtonsoft.Json;

namespace StockServer.Schedule
{
    public class StockPriceCrawlerSchedule : IInvocable
    {
        private StockRepository _stockRepository;
        private IHttpClientFactory _clientFactory;

        public StockPriceCrawlerSchedule(StockRepository stockRepository, IHttpClientFactory clientFactory)
        {
            this._stockRepository = stockRepository;
            _clientFactory = clientFactory;;
        }

        public class Datas
        {
            public List<List<string>> data9 { get; set; }
            public List<string> fields9 { get; set; }
        }

        public async Task Invoke()
        {
            DateTime date = _stockRepository.GetMaxDate().AddDays(1);
            while(DateTime.Compare(date.Date, DateTime.Now.Date) <= 0) 
            {
                //假日不抓
                if(date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday) 
                {
                    date = date.AddDays(1);
                    continue;
                }
                else {
                    await CrawlerStockByDate(date);
                    date = date.AddDays(1);
                    Thread.Sleep(7000);
                }
            }
        }

        public async Task CrawlerStockByDate(DateTime date)
        {
            using (var client = _clientFactory.CreateClient())
            {
                string json = await client.GetStringAsync($"https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date={date.ToString("yyyyMMdd")}&type=ALLBUT0999&_=1586529875476");
                var resDatas = JsonSerializer.Deserialize<Datas>(json);
                /*
                    [0]  :"證券代號"
                    [1]  :"證券名稱"
                    [2]  :"成交股數"
                    [3]  :"成交筆數"
                    [4]  :"成交金額"
                    [5]  :"開盤價"
                    [6]  :"最高價"
                    [7]  :"最低價"
                    [8]  :"收盤價"
                    [9]  :"漲跌(+/-)"
                    [10] :"漲跌價差"
                    [11] :"最後揭示買價"
                    [12] :"最後揭示買量"
                    [13] :"最後揭示賣價"
                    [14] :"最後揭示賣量"
                    [15] :"本益比"
                */
                List<Stock> stockList = new List<Stock>();
                if (resDatas.data9 != null)
                {
                    var id = _stockRepository.GetMaxId() + 1;
                    resDatas.data9.ForEach(data =>
                    {
                        stockList.Add(new Stock()
                        {
                            id = id++,
                            stock_id = data[0],
                            trading_volume = data[2],
                            trading_money = data[4],
                            open = data[5],
                            max = data[6],
                            min = data[7],
                            close = data[8],
                            spread = data[10],
                            trading_turnover = data[3],
                            trading_date = date.Date
                        });
                    });
                    _stockRepository.Insert(stockList);
                }
            }
        }
    }
}