using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SapApi.Models;
using System.Collections.Generic;
using System.Data.Odbc;

namespace SapApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly HanaConfig _hanaConf;
        private readonly OpcionesConfig _opcionesConf;

        public ItemsController(
            IOptions<HanaConfig> hanaOptions,
            IOptions<OpcionesConfig> opcionesOptions)
        {
            _hanaConf = hanaOptions.Value;
            _opcionesConf = opcionesOptions.Value;
        }

        [HttpGet]
        public IActionResult GetItems([FromQuery] string? itemCode = null)
        {
            var items = new List<Item>();
            string connStr = $"DSN={_hanaConf.Dsn};UID={_hanaConf.Uid};PWD={_hanaConf.Pwd}";

            string bd = _opcionesConf.Ambiente == "test" ? "TEST_MUNDO_TOOL" : "SBO_MUNDO_TOOL";

            // Nueva consulta SQL con INNER JOIN
            string query = $@"
                SELECT T0.""ItemCode"", T0.""ItemName"", T0.""FrgnName"", T0.""OnHand"", 
                       T2.""BinCode"", T3.""Price""
                FROM {bd}.OITM T0
                INNER JOIN {bd}.OIBQ T1 ON T0.""ItemCode"" = T1.""ItemCode""
                INNER JOIN {bd}.OBIN T2 ON T1.""BinAbs"" = T2.""AbsEntry""
                INNER JOIN {bd}.ITM1 T3 ON T0.""ItemCode"" = T3.""ItemCode""
                WHERE T0.""ItemCode"" = ? AND T3.""PriceList"" = 11";

            using var conn = new OdbcConnection(connStr);
            conn.Open();

            using var cmd = new OdbcCommand(query, conn);
            if (!string.IsNullOrEmpty(itemCode))
                cmd.Parameters.AddWithValue("ItemCode", itemCode);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new Item
                {
                    ItemCode = reader["ItemCode"].ToString(),
                    ItemName = reader["ItemName"].ToString(),
                    FrgnName = reader["FrgnName"].ToString(),
                    OnHand = reader["OnHand"] != DBNull.Value ? Convert.ToInt32(reader["OnHand"]) : 0,
                    BinCode = reader["BinCode"].ToString(),
                    Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0
                };
                items.Add(item);
            }

            if (!string.IsNullOrEmpty(itemCode))
                return items.Count > 0 ? Ok(items[0]) : NotFound();

            return Ok(items);
        }
    }
}
