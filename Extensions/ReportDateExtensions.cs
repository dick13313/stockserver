using System;

namespace stockserver.Extensions
{
    public static class ReportDateExtensions
    {
        public static DateTime GetSeasonReportDate()
        {
            return DateTime.Now;
        }
    }
}