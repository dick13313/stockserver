using System;

namespace stockserver.Models.View
{
    public class StockCalculateModel
    {
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
        public decimal percent { get; set; }
        public decimal average_percent_n { get; set; }
        public decimal sum_percent_n { get; set; }
    }
}