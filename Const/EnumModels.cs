
namespace StockServer.Const
{
    public class EnumModels
    {
        public enum SeasonReportType
        {
            綜合損益表 = 1,
            資產負債表 = 2
        }

        public enum OptionLegalType
        {
            TXF, // 臺股期貨
            EXF, // 電子期貨
            FXF, // 金融期貨
            MXF, // 小型臺指期貨
            T5F, // 臺灣50期貨
            STF, // 股票期貨
            ETF, // ETF期貨
            GTF, // 櫃買指數期貨
            XIF, // 非金電期貨
            G2F, // 富櫃200期貨
            TJF, // 東證期貨
            I5F, // 印度50期貨-己下市
            SPF, // 美國標普500期貨
            UNF, // 美國那斯達克100期貨
            UDF, // 美國道瓊期貨
        }

        public enum OptionDailyType
        {
            all, // 全部
            BRF, // 布蘭特原油期貨(BRF)
            G2F, // 富櫃200期貨(G2F)
            GDF, // 黃金期貨(GDF)
            GTF, // 櫃買期貨(GTF)
            MTX, // 小型臺指(MTX)
            RHF, // 美元兌人民幣期貨(RHF)
            RTF, // 小型美元兌人民幣期貨(RTF)
            SPF, // 美國標普500期貨(SPF)
            T5F, // 臺灣50期貨(T5F)
            TE, // 電子期貨(TE)
            TF, // 金融期貨(TF)
            TGF, // 臺幣黃金期貨(TGF)
            TJF, // 東證期貨(TJF)
            TX, // 臺股期貨(TX)
            UDF, // 美國道瓊期貨(UDF)
            UNF, // 美國那斯達克100期貨(UNF)
            XAF, // 澳幣兌美元期貨(XAF)
            XBF, // 英鎊兌美元期貨(XBF)
            XEF, // 歐元兌美元期貨(XEF)
            XIF, // 非金電期貨(XIF)
            XJF, // 美元兌日圓期貨(XJF)
            specialid, // 股票期貨
        }
    }
}