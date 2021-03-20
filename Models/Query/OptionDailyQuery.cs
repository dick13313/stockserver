using System;
using StockServer.Const;

namespace StockServer.Models.Query
{
    public class OptionDailyQuery
    {
        public DateTime queryStartDate { get; set; }
        public DateTime queryEndDate { get; set; }
        public EnumModels.OptionDailyType optionType { get; set; }
    }
}