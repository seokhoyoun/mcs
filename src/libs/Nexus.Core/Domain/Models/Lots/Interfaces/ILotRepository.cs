using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Shared.Bases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots.Interfaces
{
    public interface ILotRepository : IRepository<Lot, string>
    {
        /// <summary>
        /// Ư�� ������ ��� Lot�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="status">��ȸ�� Lot ����</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�ش� ������ Lot ���</returns>
        Task<IReadOnlyList<Lot>> GetLotsByStatusAsync(ELotStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ư�� �ð� ���� ���� ������ Lot�� ��ȸ�մϴ�.
        /// </summary>
        /// <param name="startDate">���� �Ͻ�</param>
        /// <param name="endDate">���� �Ͻ�</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>�ش� �Ⱓ�� ������ Lot ���</returns>
        Task<IReadOnlyList<Lot>> GetLotsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ư�� Lot�� Step�� �߰��մϴ�.
        /// </summary>
        /// <param name="lotId">Lot ID</param>
        /// <param name="lotStep">�߰��� LotStep</param>
        /// <param name="cancellationToken">�۾� ��� ��ū</param>
        /// <returns>���� ����</returns>
        Task<bool> AddLotStepAsync(string lotId, LotStep lotStep, CancellationToken cancellationToken = default);

  
        Task<IReadOnlyList<LotStep>> GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken = default);


        Task<bool> AddCassetteToStepAsync(string lotId, string stepId, string cassetteId, CancellationToken cancellationToken = default);
    }
}