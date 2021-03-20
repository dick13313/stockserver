using System.Collections.Generic;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using StockServer.Models.DataBase;

namespace StockServer.Repository
{
    public class StockInfoRepository
    {
        private SqlConnection _conn;
        public StockInfoRepository(SqlConnection conn)
        {
            _conn = conn;
        }
        public IEnumerable<StockInfo> GetAll()
        {
            return _conn.GetAll<StockInfo>();
        }

        public StockInfo GetStockInfo(string stock_id)
        {
            return _conn.QueryFirstOrDefault<StockInfo>(
                "select * from TaiwanStockInfo where stock_id=@stock_id", new { stock_id }
            );
        }

        public IEnumerable<string> StockCategorys()
        {
            return _conn.Query<string>("select distinct industry_category from TaiwanStockInfo");
        }

        public IEnumerable<string> GetAllStockIdByType(string type_name)
        {
            return _conn.Query<string>("select stock_id from TaiwanStockInfo where len(stock_id)=4 and type_name=@type_name", new { type_name });
        }

        public IEnumerable<string> GetAllStockIdByTypeAndIgnoreCategorys(string type_name, IEnumerable<string> ignore_categorys)
        {
            return _conn.Query<string>(
                "select stock_id from TaiwanStockInfo where len(stock_id)=4 and type_name=@type_name and industry_category not in @ignore_categorys", 
                new { 
                    type_name,
                    ignore_categorys
                }
            );
        }
    }
}