using System;
using System.Collections.Generic;
using System.Linq;
using StockServer.Repository;
using StockServer.Models.DataBase;
using TicTacTec.TA.Library;

namespace StockServer.Strategy
{
    public class KDStrategy : IStrategy
    {
        private IEnumerable<Stock> _stockList;
        public double[] outSlowK;
        public double[] outSlowD;

        private List<int> indexList;
        private int _inFastK;
        private int _inSlowK;
        private int _inSlowD;

        public KDStrategy(IEnumerable<Stock> stockList, int inFastK = 9, int inSlowK = 3, int inSlowD = 3)
        {
            _stockList = stockList;
            _inFastK = inFastK;
            _inSlowK = inSlowK;
            _inSlowD = inSlowD;
        }

        public KDStrategy Calculate()
        {
            int outBegIdx, outNBElement;
            outSlowK = new double[_stockList.Count() - (_inFastK + _inSlowK)];
            outSlowD = new double[_stockList.Count() - (_inFastK + _inSlowK)];
            indexList = Enumerable.Range(0, _stockList.Count() - (_inFastK + _inSlowK)).ToList();
            Core.Stoch(
                0, 
                _stockList.Count()-1, 
                _stockList.Select(x => Convert.ToSingle(x.max)).ToArray(),
                _stockList.Select(x => Convert.ToSingle(x.min)).ToArray(),
                _stockList.Select(x => Convert.ToSingle(x.close)).ToArray(),
                _inFastK,
                _inSlowK,
                Core.MAType.Sma,
                _inSlowD,
                Core.MAType.Sma,
                out outBegIdx,
                out outNBElement,
                outSlowK,
                outSlowD
            );
            return this;
        }

        public KDStrategy GoldenCross()
        {
            indexList.RemoveAll(index => !(outSlowK[index] > outSlowD[index] && outSlowK[index-1] < outSlowD[index-1]));
            return this;
        }

        public KDStrategy DeathCross() 
        {
            indexList.RemoveAll(index => !(outSlowK[index] < outSlowD[index] && outSlowK[index-1] > outSlowD[index-1]));
            return this;
        }

        public KDStrategy K_BiggerThan(double value)
        {
            indexList.RemoveAll(index => outSlowK[index] <= value);
            return this;
        }

        public KDStrategy K_SmallerThan(double value)
        {
            indexList.RemoveAll(index => outSlowK[index] >= value);
            return this;
        }

        public KDStrategy D_BiggerThan(double value)
        {
            indexList.RemoveAll(index => outSlowD[index] <= value);
            return this;
        }

        public KDStrategy D_SmallerThan(double value)
        {
            indexList.RemoveAll(index => outSlowD[index] >= value);
            return this;
        }

        public IEnumerable<int> GetResultIndexList()
        {
            return indexList.Select(index => index + _inFastK + _inSlowK).ToList(); // 配合原本_stockList 的 index, 前 (_inFastK + _inSlowK) 天會沒資料
        }

        public IEnumerable<double> GetResultKList()
        {
            return indexList.Select(index => this.outSlowK[index]).ToList();
        }

        public IEnumerable<double> GetResultDList()
        {
            return indexList.Select(index => this.outSlowD[index]).ToList();
        }
    }
}