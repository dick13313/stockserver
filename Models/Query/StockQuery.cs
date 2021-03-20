using System;

namespace StockServer.Models.Query
{
    public class StockQuery
    {
        public string stock_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }
}
