using System;

namespace stockserver.Models.View
{
    public class OptionLegalSummary
    {
        public DateTime date { get; set; }
        public string name { get; set; }
        public int lot_buy { get; set; }
        public long lot_buy_amount { get; set; }
        public int lot_sell { get; set; }
        public long lot_sell_amount { get; set; }
        public int open_interest_buy { get; set; }
        public long open_interest_buy_amount { get; set; }
        public int open_interest_sell { get; set; }
        public long open_interest_sell_amount { get; set; }
    }
}