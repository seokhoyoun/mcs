using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Areas;
using Nexus.Core.Domain.Models.Areas.Interfaces;
using Nexus.Core.Domain.Models.Areas.Services;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Stockers.Services
{
    public class StockerService : BaseDataService<Stocker, string>, IStockerService
    {
        private readonly IStockerRepository _stockerRepository;
        private readonly List<Stocker> _stockers;
        private bool _initialized = false;
        private readonly object _initLock = new object();
        private Task? _initTask;

        public IReadOnlyList<Stocker> Stockers => _stockers.AsReadOnly();

        public StockerService(ILogger<AreaService> logger, IStockerRepository stockerRepository) : base(logger, stockerRepository)
        {
            _stockerRepository = stockerRepository;
            _stockers = new List<Stocker>();
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }
            Task? startTask = null;
            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }
                if (_initTask == null)
                {
                    _initTask = InitializeCoreAsync(cancellationToken);
                }
                startTask = _initTask;
            }
            await startTask;
        }

        private async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                IReadOnlyList<Stocker> stockers = await _stockerRepository.GetAllAsync(cancellationToken);
                _stockers.Clear();
                if (stockers != null && stockers.Count > 0)
                {
                    _stockers.AddRange(stockers);
                }
                else
                {
                    _logger.LogWarning("초기화된 Stocker 데이터가 없습니다.");
                }
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StockerService 초기화 중 오류 발생");
                throw;
            }
        }

        public Task AssignCassetteAsync(string stockerId, string cassetteId, string portId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }
}

