using System;
using System.Collections.Generic;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;

namespace StockServer.Repository
{
    public class OptionDailyRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<OptionDailyRepository> _logger;
        public OptionDailyRepository(ILogger<OptionDailyRepository> logger, SqlConnection conn) 
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<OptionDaily> optionDailyList)
        {
             try
            {
                using(var scope = new TransactionScope())
                {
                    foreach(var optionDaily in optionDailyList)
                    {
                        _conn.Insert(optionDaily);
                    }
                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public bool IsExist(OptionDaily optionLegal)
        {
            return _conn.ExecuteScalar<bool>("select 1 from OptionDaily where date=@date and type=@type and expired=@expired", optionLegal);
        }
    }
}