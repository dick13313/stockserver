using System;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SqlController : ControllerBase
    {
        private SqlConnection _conn;

        public SqlController(SqlConnection conn)
        {
            _conn = conn;
        }
        
        [HttpPost]
        public IActionResult Query([FromBody]Body body)
        {
            try
            {
                return new JsonResult(_conn.Query(body.sql));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
            
        }

        [HttpPost]
        public IActionResult Execute([FromBody]Body body)
        {
            try
            {
                return new JsonResult(_conn.Execute(body.sql));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }


        public class Body
        {
            public string sql { get; set; }
        }
    
    }
}
