using System;
using CsvHelper.Configuration.Attributes;
using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("OptionLegal")]
    public class OptionLegal
    {
        ////日期,商品名稱,身份別,多方交易口數,多方交易契約金額(千元),空方交易口數,空方交易契約金額(千元),多空交易口數淨額,多空交易契約金額淨額(千元),多方未平倉口數,多方未平倉契約金額(千元),空方未平倉口數,空方未平倉契約金額(千元),多空未平倉口數淨額,多空未平倉契約金額淨額(千元)
        [ExplicitKey]
        [Name("日期")]
        public DateTime date { get; set; }
        [ExplicitKey]
        [Name("商品名稱")]
        public string name { get; set; }
        [ExplicitKey]
        [Name("身份別")]
        public string legal_name { get; set; }
        [Name("多方交易口數")]
        public int lot_buy { get; set; }
        [Name("多方交易契約金額(千元)")]
        public long lot_buy_amount { get; set; }
        [Name("空方交易口數")]
        public int lot_sell { get; set; }
        [Name("空方交易契約金額(千元)")]
        public long lot_sell_amount { get; set; }
        [Name("多方未平倉口數")]
        public int open_interest_buy { get; set; }
        [Name("多方未平倉契約金額(千元)")]
        public long open_interest_buy_amount { get; set; }
        [Name("空方未平倉口數")]
        public int open_interest_sell { get; set; }
        [Name("空方未平倉契約金額(千元)")]
        public long open_interest_sell_amount { get; set; }
    }
}