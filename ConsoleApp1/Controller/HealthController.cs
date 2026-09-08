using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIO_QH.Controller
{
    public struct Health()
    {
        public bool Connectivity { get; set; } = true;
        public bool Database { get; set; } = true;
        public bool Redis { get; set; } = true;
        public bool Socket { get; set; } = true;
        //public bool SurveilanceTxt{ get; set; } = true;
        //public bool MastersFile { get; set; }
        //public DateTime? MastersTimeStamp { get; set; }
    }

    [ApiController]
    [Route("quoteHandler/[controller]")]
    public class HealthController() : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var healthFlags = new Health();
            healthFlags.Connectivity = true;
            //healthFlags.Database = PostgreSql.isConnected();
            //healthFlags.Redis = await _rd.HeathCheck();
            //healthFlags.Socket = ConnectionService.IsConnected;
            //healthFlags.SurveilanceTxt = 
            //healthFlags.MastersFile = _mst.IsAvailable();
            //healthFlags.MastersTimeStamp = null;
            return Ok(healthFlags);
        }
    }
}
