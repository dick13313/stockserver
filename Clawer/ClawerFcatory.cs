using System;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;

namespace StockServer.Clawer
{
    public class ClawerFcatory
    {
        private SeasonReportClawer _seasonReportClawer;
        private MonthReportClawer _monthReportClawer;
        private StockHolderClawer _stockHolderClawer;
        private OptionLegalClawer _optionLegalClawer;
        private OptionDailyClawer _optionDailyClawer;
        private StockInfoClawer _stockInfoClawer;
        private StockDividendClawer _stockDividendClawer;
        private FundamentalDailyClawer _fundamentalDailyClawer;
        public ClawerFcatory(IServiceCollection services)
        {
            var provider = services.AddLogging(config => config.AddNLog()).BuildServiceProvider();
            _seasonReportClawer = provider.GetService<SeasonReportClawer>();
            _monthReportClawer = provider.GetService<MonthReportClawer>();
            _stockHolderClawer = provider.GetService<StockHolderClawer>();
            _optionLegalClawer = provider.GetService<OptionLegalClawer>();
            _optionDailyClawer = provider.GetService<OptionDailyClawer>();
            _stockInfoClawer = provider.GetService<StockInfoClawer>();
            _stockDividendClawer = provider.GetService<StockDividendClawer>();
            _fundamentalDailyClawer = provider.GetService<FundamentalDailyClawer>();
        }

        public SeasonReportClawer GetSeasonReportClawer()
        {
            return _seasonReportClawer;
        }

        public MonthReportClawer GetMonthReportClawer()
        {
            return _monthReportClawer;
        }

        public StockHolderClawer GetStockHolderClawer()
        {
            return _stockHolderClawer;
        }

        public OptionLegalClawer GetOptionLegalClawer()
        {
            return _optionLegalClawer;
        }

        public OptionDailyClawer GetOptionDailyClawer()
        {
            return _optionDailyClawer;
        }

        public StockInfoClawer GetStockInfoClawer()
        {
            return _stockInfoClawer;
        }

        public StockDividendClawer GetStockDividendClawer()
        {
            return _stockDividendClawer;
        }

        public FundamentalDailyClawer GetFundamentalDailyClawer()
        {
            return _fundamentalDailyClawer;
        }
    }
}