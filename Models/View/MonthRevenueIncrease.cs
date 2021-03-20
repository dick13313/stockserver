namespace StockServer.Models.View
{
    public class MonthRevenueIncrease
    {
        public string stock_id { get; set; }
        public string stock_name { get; set; }
        public decimal last_month_increase { get; set; } // 和上個月比增加的倍數
        public decimal last_year_increase { get; set; }  // 和去年同月比增加的倍數
        public int this_revenue { get; set; }
        public int last_month_revenue { get; set; }
        public int last_year_revenue { get; set; }
    }
}