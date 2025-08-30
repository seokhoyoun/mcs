using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Stockers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Stockers.Services
{
    public class StockerService : IStockerService
    {
        private readonly IStockerRepository _stockerRepository;
        private readonly List<Stocker> _stockers;

        public IReadOnlyList<Stocker> Stockers => _stockers.AsReadOnly();

        public StockerService(IStockerRepository stockerRepository)
        {
            _stockerRepository = stockerRepository;
            _stockers = new List<Stocker>();
        }

        public async Task InitializeStockerService()
        {
            var stockers = await _stockerRepository.GetAllStockersAsync();
            _stockers.AddRange(stockers);
        }

        public Task AssignCassetteAsync(string stockerId, string cassetteId, string portId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

