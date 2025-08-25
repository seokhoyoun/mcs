using Microsoft.AspNetCore.Mvc;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;

namespace Nexus.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly IAreaCreationService _areaCreationService;
        private readonly ILogger<AreasController> _logger;

        public AreasController(
            IAreaCreationService areaCreationService,
            ILogger<AreasController> logger)
        {
            _areaCreationService = areaCreationService;
            _logger = logger;
        }

        /// <summary>
        /// Area �ý����� �ʱ�ȭ�մϴ�.
        /// </summary>
        /// <param name="command">�ʱ�ȭ ���</param>
        /// <param name="cancellationToken">��� ��ū</param>
        /// <returns>�ʱ�ȭ ���</returns>
        [HttpPost]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> InitializeAreas(
            [FromBody] string jsonPayload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received area initialization request.");

                var result = await _areaCreationService.CreateAreaAsync(jsonPayload, cancellationToken);

                _logger.LogInformation("Area initialization completed: {Result}", result);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Area initialization conflict: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid area initialization request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during area initialization");
                return StatusCode(500, "Internal server error occurred while initializing areas");
            }
        }

    }
}