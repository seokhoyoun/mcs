using Microsoft.Extensions.Logging;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots.Services
{
    public class LotService 
    {
        private readonly ILogger _logger;
        private readonly ILotRepository _lotRepository;

        public LotService(ILogger<LotService> logger, ILotRepository lotRepository)
        {
            _logger = logger;
            _lotRepository = lotRepository;
        }
        public async Task<LotStep?> GetLotStepAsync(string lotId, string stepId)
        {
            var lot = await _lotRepository.GetByIdAsync(lotId);
            return lot?.LotSteps.FirstOrDefault(s => s.Id == stepId);
        }

        public async Task<bool> AddCassetteToStepAsync(string lotId, string stepId, string cassetteId)
        {
            var lot = await _lotRepository.GetByIdAsync(lotId);
            var step = lot?.LotSteps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                // Cassette 추가 로직
                Debug.Assert(lot != null);
                await _lotRepository.UpdateAsync(lot);
                return true;
            }
            return false;
        }
    }
}
