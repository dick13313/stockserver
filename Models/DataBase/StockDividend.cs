using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("StockDividend")]
    public class StockDividend
    {
        [ExplicitKey]
        public string stock_id { get; set; }
        [ExplicitKey]
        public string time_string { get; set; }
        public int year { get; set; }
        public int? season { get; set; }
        public decimal dividend { get; set; }
    }
}