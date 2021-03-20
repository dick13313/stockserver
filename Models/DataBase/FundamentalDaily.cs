using System;
using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("FundamentalDaily")]
    public class FundamentalDaily
    {
        //"證券代號","證券名稱","殖利率(%)","股利年度","本益比","股價淨值比","財報年/季"
        [ExplicitKey]
        public DateTime date { get; set; }
        [ExplicitKey]
        [Name("證券代號")]
        public string stock_id { get; set; }
        [Name("殖利率(%)")]
        public decimal? dividend_yield { get; set; }
        [Name("本益比")]
        public decimal? pe_ratio { get; set; }
        [Name("股價淨值比")]
        public decimal? price_book_ratio { get; set; }
    }
}