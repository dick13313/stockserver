using System;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Const;
using StockServer.Models.Query;
using StockServer.Models.DataBase;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClawerController
    {
        private readonly ILogger<ClawerController> _logger;
        private ClawerFcatory _reportFcatory;
        private StockHolderDailyClawer _stockHolderDailyClawer;
        public ClawerController(ILogger<ClawerController> logger, ClawerFcatory reportFcatory, StockHolderDailyClawer stockHolderDailyClawer)
        {
            _logger = logger;
            _reportFcatory = reportFcatory;
            _stockHolderDailyClawer = stockHolderDailyClawer;
        }

        [HttpPost]
        public IActionResult RunSeasonReportClawer([FromBody]SeasonReportQuery seasonReportQuery)
        {
            foreach (EnumModels.SeasonReportType seasonReportType in Enum.GetValues(typeof(EnumModels.SeasonReportType)))
            {
                Task clawerTask = _reportFcatory.GetSeasonReportClawer().ExecuteAsync(seasonReportType, seasonReportQuery.year, seasonReportQuery.season); // 不管執行結果
            }
            return new JsonResult(new { Success = true });
        }

        [HttpPost]
        public IActionResult RunMonthReportClawer([FromBody]MonthReportQuery monthReportQuery)
        {
            Task clawerTask = _reportFcatory.GetMonthReportClawer().ExecuteAsync(monthReportQuery.year, monthReportQuery.month); // 不管執行結果
            return new JsonResult(new { Success = true });
        }

        [HttpPost]
        public IActionResult RunStockHolderClawer([FromBody]StockHolderQuery stockHolderQuery)
        {
            Task clawerTask = _reportFcatory.GetStockHolderClawer().ExecuteAsync(stockHolderQuery.stock_id, stockHolderQuery.dateString);
            return new JsonResult(new { Success = true });
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// optionType: 
        /// TXF (臺股期貨) \
        /// EXF (電子期貨) \
        /// FXF (金融期貨) \
        /// MXF (小型臺指期貨) \
        /// T5F (臺灣50期貨) \
        /// STF (股票期貨) \
        /// ETF (ETF期貨) \
        /// GTF (櫃買指數期貨) \
        /// XIF (非金電期貨) \
        /// G2F (富櫃200期貨) \
        /// TJF (東證期貨) \
        /// I5F (印度50期貨-己下市) \
        /// SPF (美國標普500期貨) \
        /// UNF (美國那斯達克100期貨) \
        /// UDF (美國道瓊期貨) 
        /// </remarks>
        /// <returns>Success = true or false</returns>
        [HttpPost]
        public IActionResult RunOptionLegalClawer([FromBody]OptionLegalQuery optionLegalQuery)
        {
            Task clawerTask = _reportFcatory.GetOptionLegalClawer().ExecuteAsync(optionLegalQuery);
            // await _reportFcatory.GetOptionLegalClawer().ExecuteAsync(optionLegalQuery);
            return new JsonResult(new { Success = true });
        }
        
        /// <summary>
        /// </summary>
        /// <remarks>
        /// all = 全部 \
        /// BRF = 布蘭特原油期貨(BRF) \
        /// G2F = 富櫃200期貨(G2F) \
        /// GDF = 黃金期貨(GDF) \
        /// GTF = 櫃買期貨(GTF) \
        /// MTX = 小型臺指(MTX) \
        /// RHF = 美元兌人民幣期貨(RHF) \
        /// RTF = 小型美元兌人民幣期貨(RTF) \
        /// SPF = 美國標普500期貨(SPF) \
        /// T5F = 臺灣50期貨(T5F) \
        /// TE, = 電子期貨(TE) \
        /// TF, = 金融期貨(TF) \
        /// TGF = 臺幣黃金期貨(TGF) \
        /// TJF = 東證期貨(TJF) \
        /// TX, = 臺股期貨(TX) \
        /// UDF = 美國道瓊期貨(UDF) \
        /// UNF = 美國那斯達克100期貨(UNF) \
        /// XAF = 澳幣兌美元期貨(XAF) \
        /// XBF = 英鎊兌美元期貨(XBF) \
        /// XEF = 歐元兌美元期貨(XEF) \
        /// XIF = 非金電期貨(XIF) \
        /// XJF = 美元兌日圓期貨(XJF) \
        /// specialid = 股票期貨 \
        /// </remarks>
        /// <returns>Success = true or false</returns>
        [HttpPost]
        public IActionResult RunOptionDailyClawer([FromBody]OptionDailyQuery optionDailyQuery)
        {
            Task clawerTask = _reportFcatory.GetOptionDailyClawer().ExecuteAsync(optionDailyQuery);
            return new JsonResult(new { Success = true });
        }

        [HttpPost]
        public IActionResult RunStockInfoClawer()
        {
            Task clawerTask = _reportFcatory.GetStockInfoClawer().ExecuteAsync();
            return new JsonResult(new { Success = true });
        }

        [HttpPost]
        public IActionResult RunStockDividendClawer(string stock_id)
        {
            Task clawerTask = _reportFcatory.GetStockDividendClawer().ExecuteAsync(stock_id);
            return new JsonResult(new { Success = true });
        }

        [HttpPost]
        public IActionResult RunFundamentalDailyClawer(DateTime date)
        {
            Task clawerTask = _reportFcatory.GetFundamentalDailyClawer().ExecuteAsync(date);
            return new JsonResult(new { Success = true });
        }
        
        [HttpPost]
        public async Task<IActionResult> TestAsync()
        {
            await _stockHolderDailyClawer.ExecuteAsync();
            return new JsonResult(new { Success = true }); 
        }
        
    }
}