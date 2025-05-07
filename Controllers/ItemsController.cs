using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SapApi.Models;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace SapApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // ← Solo accesible con el rol "Admin" // ← Protección con JWT
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

            string query = $@"
            SELECT 
                T0.""ItemCode"", 
                T0.""ItemName"", 
                T0.""FrgnName"", 
                T0.""OnHand"", 
                T2.""BinCode"", 
                T3.""Price"" ,
                (
                    SELECT T3_12.""Price""
                    FROM {bd}.ITM1 T3_12
                    WHERE T3_12.""ItemCode"" = T0.""ItemCode"" AND T3_12.""PriceList"" = 12
                ) AS ""Mayoreo""
            FROM {bd}.OITM T0  
            INNER JOIN {bd}.OIBQ T1 ON T0.""ItemCode"" = T1.""ItemCode"" 
            INNER JOIN {bd}.OBIN T2 ON T1.""BinAbs"" = T2.""AbsEntry"" 
            INNER JOIN {bd}.ITM1 T3 ON T0.""ItemCode"" = T3.""ItemCode"" AND T3.""PriceList"" = 11
            WHERE T0.""ItemCode"" = ?";

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
                    Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                    Mayoreo = reader["Mayoreo"] != DBNull.Value ? Convert.ToDecimal(reader["Mayoreo"]) : 0,
                    // Agregar la URL de la imagen basada en el 'FrgnName'
                    ImageUrl = $"https://www.truper.com/media/import/imagenes/{reader["FrgnName"]}.jpg"
                };
                items.Add(item);
            }

            if (!string.IsNullOrEmpty(itemCode))
                return items.Count > 0 ? Ok(items[0]) : NotFound();

            return Ok(items);
        }

        // Método para obtener la imagen desde la URL externa
        [HttpGet("get-image/{imageName}")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            var imageUrl = $"https://www.truper.com/media/import/imagenes/{imageName}.jpg";  // URL de la imagen externa
            var httpClient = new HttpClient();
            
            // Descargar la imagen como bytes
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            
            // Devolver la imagen al cliente
            return File(imageBytes, "image/jpeg");  // Se devuelve la imagen como un archivo de tipo JPEG
        }
    }
}