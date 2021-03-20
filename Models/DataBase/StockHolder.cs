using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("StockHolder")]
    public class StockHolder
    {
        [ExplicitKey]
        [Name("證券代號")]
        public string stock_id { get; set; }
        [ExplicitKey]
        [Index(0)]
        public string date_string { get; set; }
        [ExplicitKey]
        [Name("持股分級")]
        public string holder_level { get; set; }
        [Name("人數")]
        public int people_count { get; set; }
        [Name("股數")]
        public string stock_holder_count { get; set; }
    }
}