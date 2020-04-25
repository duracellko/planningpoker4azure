using System;
using Microsoft.AspNetCore.Mvc;

namespace Duracellko.PlanningPoker.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        public ConfigurationController(PlanningPokerClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
        }

        public PlanningPokerClientConfiguration ClientConfiguration { get; }

        [HttpGet]
        public ActionResult GetConfiguration()
        {
            return Ok(ClientConfiguration);
        }
    }
}
