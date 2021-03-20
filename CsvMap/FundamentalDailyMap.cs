using System;
using System.Globalization;
using CsvHelper.Configuration;
using StockServer.Models.DataBase;

namespace StockServer.CsvMap
{
    public class FundamentalDailyMap: ClassMap<FundamentalDaily>
    {
        public FundamentalDailyMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.date).Ignore();
            Map(m => m.dividend_yield).ConvertUsing(row => {
                var field = row.GetField("殖利率(%)");
                if(!field.ToString().Contains("-"))
                     return Convert.ToDecimal(field);
                else
                    return null;
            });
            Map(m => m.pe_ratio).ConvertUsing(row => {
                var field = row.GetField("本益比");
                if(!field.ToString().Contains("-"))
                     return Convert.ToDecimal(field);
                else
                    return null;
            });
            Map(m => m.price_book_ratio).ConvertUsing(row => {
                var field = row.GetField("股價淨值比");
                if(!field.ToString().Contains("-"))
                     return Convert.ToDecimal(field);
                else
                    return null;
            });
        }
    }
}