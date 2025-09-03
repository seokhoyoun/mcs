using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Extensions;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Gateway.Services.Commands;
using Nexus.Gateway.Services.Interfaces;
using Nexus.Shared.Application.Interfaces;

namespace Nexus.Gateway.Services
{
    public class CassetteCreationService : ICassetteCreationService
    {
        private readonly ITransportsRepository _transportsRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<CassetteCreationService> _logger;

        public CassetteCreationService(
            ITransportsRepository transportsRepository,
            IEventPublisher eventPublisher,
            ILogger<CassetteCreationService> logger)
        {
            _transportsRepository = transportsRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<string> CreateCassetteAsync(CreateCassetteCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating cassette with ID: {CassetteId} at location: {LocationId}", 
                    command.CassetteId, command.LocationId);

                // 카세트가 이미 존재하는지 확인
                var existingCassette = await _transportsRepository.GetByIdAsync(command.CassetteId, cancellationToken);
                if (existingCassette != null)
                {
                    throw new InvalidOperationException($"Cassette with ID '{command.CassetteId}' already exists.");
                }
               
                // 카세트 생성
                var cassette = new Cassette(command.CassetteId, command.CassetteName, new List<Tray>());
                cassette.InitializeFullCassette();

                // 각 메모리를 개별적으로 저장
                foreach (var tray in cassette.Trays)
                {
                    foreach (var memory in tray.Memories)
                    {
                        await _transportsRepository.AddAsync(memory, cancellationToken);
                        _logger.LogDebug("Memory saved: {MemoryId}", memory.Id);
                    }

                    // 트레이 저장
                    await _transportsRepository.AddAsync(tray, cancellationToken);
                    _logger.LogDebug("Tray saved: {TrayId} with {MemoryCount} memories", 
                        tray.Id, tray.Memories.Count);
                }

                // 카세트 저장
                await _transportsRepository.AddAsync(cassette, cancellationToken);

                _logger.LogInformation("Cassette created successfully: {CassetteId} with {TrayCount} trays, " +
                    "total {MemoryCount} memories at location {LocationId}", 
                    cassette.Id, cassette.TrayCount, cassette.TotalMemoryCount, command.LocationId);

                return cassette.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cassette with ID: {CassetteId}", command.CassetteId);
                throw;
            }
        }

    }
}