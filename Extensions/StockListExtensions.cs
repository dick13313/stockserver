using System.Collections.Generic;
using System.Linq;
using StockServer.Strategy;
using StockServer.Models.DataBase;

namespace StockServer.Extensions
{
    public static class StockListExtensions
    {
        public static IEnumerable<Stock> UseStrategy(this IEnumerable<Stock> stockList, params IStrategy[] strategyArray)
        {
            var indexList = strategyArray.SelectMany(strategy => strategy.GetResultIndexList()).Distinct();
            return stockList
                .Select((stock, index) => new { stock, index })
                .Where(data => indexList.Contains(data.index))
                .Select(data => data.stock);
        }
        
    }
}