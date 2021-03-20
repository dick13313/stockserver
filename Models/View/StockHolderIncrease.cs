namespace StockServer.Models.View
{
    public class StockHolderIncrease
    {
        public string stock_id { get; set; }
        public string stock_name { get; set; }
        public decimal thisweek_holder_percent { get; set; }
        public decimal lastweek_holder_percent { get; set; }
        public decimal up_percent { get; set; }
    }
}