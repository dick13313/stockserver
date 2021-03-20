using System;
using StockServer.Const;

namespace StockServer.Models.Query
{
    public class OptionLegalQuery
    {
        public DateTime queryStartDate { get; set; }
        public DateTime queryEndDate { get; set; }
        public EnumModels.OptionLegalType optionType { get; set; }
    }
}