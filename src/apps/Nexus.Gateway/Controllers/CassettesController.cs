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
        /// 새로운 카세트를 생성하고 트레이와 메모리로 채웁니다.
        /// </summary>
        /// <param name="command">카세트 생성 명령</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>생성된 카세트 ID</returns>
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

                string cassetteId = await _cassetteCreationService.CreateCassetteAsync(command, cancellationToken);

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
        /// 카세트 ID로 카세트 정보를 조회합니다.
        /// </summary>
        /// <param name="id">카세트 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>카세트 정보</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetCassette(string id, CancellationToken cancellationToken = default)
        {
            // 향후 구현 예정 - 현재는 placeholder
            return Ok(new { Id = id, Message = "Cassette retrieved successfully" });
        }
    }
}
