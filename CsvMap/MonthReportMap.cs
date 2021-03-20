using System.Globalization;
using CsvHelper.Configuration;
using StockServer.Models.DataBase;

namespace StockServer.CsvMap
{
    public class MonthReportMap: ClassMap<MonthReport>
    {
        public MonthReportMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.year).Ignore();
            Map(m => m.month).Ignore();
        }
    }
}