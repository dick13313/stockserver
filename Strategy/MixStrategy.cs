using System.Collections.Generic;
using System.Linq;

namespace StockServer.Strategy
{
    public class MixStrategy : IStrategy
    {
        private IStrategy[] _strategyArray;
        public MixStrategy(params IStrategy[] strategyArray)
        {
            _strategyArray = strategyArray;
        }
        public IEnumerable<int> GetResultIndexList()
        {
            return _strategyArray.SelectMany(strategy => strategy.GetResultIndexList()).Distinct();
        }
    }
}