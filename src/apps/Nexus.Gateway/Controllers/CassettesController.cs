using Microsoft.AspNetCore.Mvc;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;

namespace Nexus.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CassettesController : ControllerBase
    {
        private readonly ICassetteCreationService _cassetteCreationService;
        private readonly ILogger<CassettesController> _logger;

        public CassettesController(
            ICassetteCreationService cassetteCreationService,
            ILogger<CassettesController> logger)
        {
            _cassetteCreationService = cassetteCreationService;
            _logger = logger;
        }

        /// <summary>
        /// ���ο� ī��Ʈ�� �����ϰ� Ʈ���̿� �޸𸮷� ä��ϴ�.
        /// </summary>
        /// <param name="command">ī��Ʈ ���� ���</param>
        /// <param name="cancellationToken">��� ��ū</param>
        /// <returns>������ ī��Ʈ ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> CreateCassette(
            [FromBody] CreateCassetteCommand command, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received cassette creation request for ID: {CassetteId}", command.CassetteId);

                var cassetteId = await _cassetteCreationService.CreateCassetteAsync(command, cancellationToken);

                _logger.LogInformation("Cassette created successfully with ID: {CassetteId}", cassetteId);

                return CreatedAtAction(
                    nameof(GetCassette), 
                    new { id = cassetteId }, 
                    cassetteId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cassette creation failed - already exists: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid cassette creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during cassette creation");
                return StatusCode(500, "Internal server error occurred while creating cassette");
            }
        }

        /// <summary>
        /// ī��Ʈ ID�� ī��Ʈ ������ ��ȸ�մϴ�.
        /// </summary>
        /// <param name="id">ī��Ʈ ID</param>
        /// <param name="cancellationToken">��� ��ū</param>
        /// <returns>ī��Ʈ ����</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetCassette(string id, CancellationToken cancellationToken = default)
        {
            // ���� ���� ���� - ����� placeholder
            return Ok(new { Id = id, Message = "Cassette retrieved successfully" });
        }
    }
}