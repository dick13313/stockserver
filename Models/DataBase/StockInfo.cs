using System;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("TaiwanStockInfo")]
    public class StockInfo
    {
        [ExplicitKey]
        public string id { get; set; }
        public string stock_id { get; set; }
        public string stock_name { get; set; }
        public string type { get; set; }
        public string type_name { get; set; }
        public string industry_category { get; set; }
        public DateTime note_date { get; set; }
    }
}


