namespace StockServer.Models.View
{
    public class SeasonRevenueIncrease
    {
        public string stock_id { get; set; }
        public string stock_name { get; set; }
        public decimal eps { get; set; }
        public decimal last_year_eps { get; set; }
        public decimal increase_eps_rate { get; set; }
        public long gross_profit { get; set; } //營業毛利（毛損）淨額
        public long operating_income { get; set; } //營業收入
    }
}