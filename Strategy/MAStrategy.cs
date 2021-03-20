using System;
using System.Collections.Generic;
using System.Linq;
using StockServer.Repository;
using StockServer.Models.DataBase;
using TicTacTec.TA.Library;

namespace StockServer.Strategy
{
    public class MAStrategy : IStrategy
    {
        private IEnumerable<Stock> _stockList;
        private int _inTime;
        public double[] outReal;

        private List<int> indexList;

        public MAStrategy(IEnumerable<Stock> stockList, int inTime)
        {
            _stockList = stockList;
            _inTime = inTime;
        }

        public MAStrategy Calculate()
        {
            int outBegIdx, outNBElement;
            outReal = new double[_stockList.Count() - _inTime + 1];
            indexList = Enumerable.Range(0, _stockList.Count() - _inTime + 1).ToList();
            Core.Sma(
                0, 
                _stockList.Count()-1,
                _stockList.Select(stock => Convert.ToSingle(stock.close)).ToArray(),
                _inTime,
                out outBegIdx,
                out outNBElement,
                outReal
            );
            return this;
        }

        public MAStrategy ClosePriceSmallerThanMA()
        {
            indexList.RemoveAll(index => Convert.ToSingle(_stockList.ElementAt(index + _inTime - 1).close) >= outReal[index]);
            return this;
        }
        public MAStrategy ClosePriceBiggerThanMA()
        {
            indexList.RemoveAll(index => Convert.ToSingle(_stockList.ElementAt(index + _inTime - 1).close) <= outReal[index]);
            return this;
        }

        public MAStrategy GoldenCross()
        {
            indexList.RemoveAll(index => {
                if(index == 0)
                    return true;
                var today_close = Convert.ToSingle(_stockList.ElementAt(index + _inTime - 1).close);
                var yesterday_close = Convert.ToSingle(_stockList.ElementAt(index + _inTime - 2).close);
                return !(today_close > outReal[index] && yesterday_close < outReal[index - 1]);
            });
            return this;
        }

        public MAStrategy DeathCross() 
        {
            indexList.RemoveAll(index => {
                if(index == 0)
                    return true;
                var today_close = Convert.ToSingle(_stockList.ElementAt(index + _inTime - 1).close);
                var yesterday_close = Convert.ToSingle(_stockList.ElementAt(index + _inTime - 2).close);
                return !(today_close < outReal[index] && yesterday_close > outReal[index - 1]);
            });
            return this;
        }

        public IEnumerable<int> GetResultIndexList()
        {
            return indexList.Select(index => index + _inTime - 1).ToList();
        }
    }
}