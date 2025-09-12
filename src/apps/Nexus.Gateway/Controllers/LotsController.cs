using Microsoft.AspNetCore.Mvc;
using Nexus.Gateway.Services;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;

namespace Nexus.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LotsController : ControllerBase
    {
        private readonly ILotCreationService _lotCreationService;

        public LotsController(ILotCreationService lotCreationService)
        {
            _lotCreationService = lotCreationService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateLot([FromBody] CreateLotCommand command, CancellationToken cancellationToken = default)
        {
            string lotId = await _lotCreationService.CreateLotAsync(command, cancellationToken);
            return Ok(lotId);
        }
    }
}
