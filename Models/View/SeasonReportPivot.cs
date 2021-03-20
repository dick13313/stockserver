namespace StockServer.Models.View
{
    public class SeasonReportPivot
    {
        public string stock_id { get; set; }
        public decimal eps { get; set; }
        public long gross_profit { get; set; }
        public long operating_income { get; set; }
    }
}