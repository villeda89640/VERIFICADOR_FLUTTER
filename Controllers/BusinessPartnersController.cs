using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SapApi.Models;
using System.Collections.Generic;
using System.Data.Odbc;

namespace SapApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessPartnersController : ControllerBase
    {
        private readonly HanaConfig _hanaConf;
        private readonly OpcionesConfig _opcionesConf;

        public BusinessPartnersController(
            IOptions<HanaConfig> hanaOptions,
            IOptions<OpcionesConfig> opcionesOptions)
        {
            _hanaConf = hanaOptions.Value;
            _opcionesConf = opcionesOptions.Value;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? cardCode = null)
        {
            var partners = new List<BusinessPartner>();
            string connStr = $"DSN={_hanaConf.Dsn};UID={_hanaConf.Uid};PWD={_hanaConf.Pwd}";
            string bd = _opcionesConf.Ambiente == "test" ? "TEST_MUNDO_TOOL" : "SBO_MUNDO_TOOL";

            string query = $"SELECT T0.\"CardCode\", T0.\"CardName\", T0.\"CardType\" FROM {bd}.OCRD T0";
            if (!string.IsNullOrEmpty(cardCode))
                query += $" WHERE T0.\"CardCode\" = ?";

            using var conn = new OdbcConnection(connStr);
            conn.Open();

            using var cmd = new OdbcCommand(query, conn);
            if (!string.IsNullOrEmpty(cardCode))
                cmd.Parameters.AddWithValue("CardCode", cardCode);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                partners.Add(new BusinessPartner
                {
                    CardCode = reader["CardCode"].ToString(),
                    CardName = reader["CardName"].ToString(),
                    CardType = reader["CardType"].ToString()
                });
            }

            if (!string.IsNullOrEmpty(cardCode))
                return partners.Count > 0 ? Ok(partners[0]) : NotFound();

            return Ok(partners);
        }
    }
}
