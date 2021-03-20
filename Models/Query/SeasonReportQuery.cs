using System;

namespace StockServer.Models.Query
{
    public class SeasonReportQuery
    {
        public int year { get; set; }
        public int season { get; set; }

        public SeasonReportQuery() {}
        public SeasonReportQuery(DateTime date)
        {
            var tuple = GetSeasonByDate(date);
            this.year = tuple.Item1;
            this.season = tuple.Item2;
        }

        private Tuple<int, int> GetSeasonByDate(DateTime date)
        {
            switch(date.Month)
            {
                case 1:
                    return Tuple.Create(date.Year - 1912, 4);
                case 2:
                    return Tuple.Create(date.Year - 1912, 4);
                case 3:
                    return Tuple.Create(date.Year - 1912, 4);
                case 4:
                    return Tuple.Create(date.Year - 1911, 1);
                case 5: 
                    if(date.Day <= 14)
                        return Tuple.Create(date.Year - 1911, 1);
                    else
                        return Tuple.Create(date.Year - 1911, 2);
                case 6:
                    return Tuple.Create(date.Year - 1911, 2);
                case 7:
                    return Tuple.Create(date.Year - 1911, 2);
                case 8:
                    if(date.Day <= 14)
                        return Tuple.Create(date.Year - 1911, 2);
                    else
                        return Tuple.Create(date.Year - 1911, 3);
                case 9:
                    return Tuple.Create(date.Year - 1911, 3);
                case 10:
                    return Tuple.Create(date.Year - 1911, 3);
                case 11:
                    if(date.Day <= 14)
                        return Tuple.Create(date.Year - 1911, 3);
                    else
                        return Tuple.Create(date.Year - 1911, 4);
                case 12:
                    return Tuple.Create(date.Year - 1911, 4);
                default:
                    throw new ArgumentException($"不合理的月份: {date.Month}");
            }
        }
    }
}