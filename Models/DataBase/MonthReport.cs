using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("MonthReport")]
    public class MonthReport
    {
        [ExplicitKey]
        [Name("公司代號")]
        // [Index(2)]
        public string stock_id { get; set; }
        [ExplicitKey]
        public int year { get; set; }
        [ExplicitKey]
        public int month { get; set; }

        [Name("營業收入-當月營收")]
        // [Index(5)]
        public string revenue { get; set; }

        [Name("備註")]
        // [Index(13)]
        public string remark { get; set; }
    }
}