using System.Collections.Generic;

namespace StockServer.Strategy
{
    public interface IStrategy
    {
        IEnumerable<int> GetResultIndexList();
    }
}