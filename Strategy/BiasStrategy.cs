using System;
using System.Collections.Generic;
using System.Linq;
using StockServer.Models.DataBase;

namespace StockServer.Strategy
{
    public class BiasStrategy : IStrategy
    {
        private IEnumerable<Stock> _stockList;
        private int _inTime;
        private double[] biasArray;
        private List<int> indexList;
        public BiasStrategy(IEnumerable<Stock> stockList, int inTime)
        {
            _stockList = stockList;
            _inTime = inTime;
        }
        
        public BiasStrategy Calculate()
        {
            indexList = Enumerable.Range(0, _stockList.Count() - _inTime + 1).ToList();
            var maList = new MAStrategy(_stockList, _inTime).Calculate().outReal;
            biasArray = new double[maList.Length];
            //(当日收盘价-N期平均收盘价)/N期平均收盘价*100%
            for(int i=0; i<maList.Length; i++)
            {
                biasArray[i] = (Convert.ToDouble(_stockList.ElementAt(i + _inTime - 1).close) - maList [i]) / maList [i];
            }
            return this;
        }

        public BiasStrategy Over(double value)
        {
            indexList.RemoveAll(index => biasArray[index] < value);
            return this; 
        }

        public IEnumerable<int> GetResultIndexList()
        {
            return indexList.Select(index => index + _inTime - 1).ToList();
        }
    }
}