using Dapper.Contrib.Extensions;

namespace StockServer.Models.DataBase
{
    [Table("SeasonReport")]
    public class SeasonReport
    {
        [ExplicitKey]
        public string stock_id { get; set; }
        [ExplicitKey]
        public int year { get; set; }
        [ExplicitKey]
        public int season { get; set; }
        [ExplicitKey]
        public string item { get; set; }
        public string value { get; set; }
        public int seq { get; set; }
        [ExplicitKey]
        public int type { get; set; }
    }
}