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

        public IReadOnlyList<Stocker> Stockers => _stockers.AsReadOnly();

        public StockerService(ILogger<AreaService> logger, IStockerRepository stockerRepository) : base(logger, stockerRepository)
        {
            _stockerRepository = stockerRepository;
            _stockers = new List<Stocker>();
        }
        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var stockers = await _stockerRepository.GetAllAsync();
            _stockers.AddRange(stockers);
        }

        public Task AssignCassetteAsync(string stockerId, string cassetteId, string portId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }
}

