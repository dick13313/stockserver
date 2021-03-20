using System;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("TWStockPrice_history_1990")]
    public class Stock
    {
        [ExplicitKey]
        public int id { get; set; }
        public string stock_id { get; set; }
        public string trading_volume { get; set; }
        public string trading_money { get; set; }
        public string open { get; set; }
        public string max { get; set; }
        public string min { get; set; }
        public string close { get; set; }
        public string spread { get; set; }
        public string trading_turnover { get; set; }
        public DateTime trading_date { get; set; }
    }
}
