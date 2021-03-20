using System;
using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("OptionDaily")]
    public class OptionDaily
    {
        [ExplicitKey]
        [Name("交易日期")]
        public DateTime date { get; set; }
        [ExplicitKey]
        [Name("契約")]
        public string type { get; set; }
        [ExplicitKey]
        [Name("到期月份(週別)")]
        public string expired { get; set; }
        [Name("開盤價")]
        public int open { get; set; }
        [Name("最高價")]
        public int max { get; set; }
        [Name("最低價")]
        public int min { get; set; }
        [Name("收盤價")]
        public int close { get; set; }
        [Name("成交量")]
        public long volume { get; set; }
        [Name("結算價")]
        public int close_price { get; set; }
        [Name("未沖銷契約數")]
        public int un_written { get; set; }


    }
}