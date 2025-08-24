using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Transports;
using StackExchange.Redis;
using System.Linq.Expressions;
using System.Text.Json;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisLotRepository : ILotRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private const string LOT_KEY_PREFIX = "lot:";
        private const string LOT_STEP_KEY_PREFIX = "lot_step:";
        private const string PLAN_GROUP_KEY_PREFIX = "plan_group:";
        private const string CASSETTE_KEY_PREFIX = "cassette:";
        private const string TRAY_KEY_PREFIX = "tray:";

        public RedisLotRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<Lot?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var hashEntries = await _database.HashGetAllAsync($"{LOT_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
                return null;

            return await ConvertHashToLotAsync(id, hashEntries, cancellationToken);
        }

        public async Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{LOT_KEY_PREFIX}*");

            var lots = new List<Lot>();
            foreach (var key in keys)
            {
                var id = key.ToString().Substring(LOT_KEY_PREFIX.Length);
                var lot = await GetByIdAsync(id, cancellationToken);
                if (lot != null)
                    lots.Add(lot);
            }

            return lots.AsReadOnly();
        }

        public async Task<IReadOnlyList<Lot>> GetAsync(Expression<Func<Lot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var allLots = await GetAllAsync(cancellationToken);
            var compiledPredicate = predicate.Compile();
            return allLots.Where(compiledPredicate).ToList().AsReadOnly();
        }

        public async Task<Lot> AddAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            var hashEntries = new HashEntry[]
            {
                new HashEntry("name", entity.Name),
                new HashEntry("status", entity.Status.ToString()),
                new HashEntry("priority", entity.Priority),
                new HashEntry("received_time", entity.ReceivedTime.ToString("o")),
                new HashEntry("purpose", entity.Purpose),
                new HashEntry("eval_no", entity.EvalNo),
                new HashEntry("part_no", entity.PartNo),
                new HashEntry("qty", entity.Qty),
                new HashEntry("option", entity.Option),
                new HashEntry("line", entity.Line),
                new HashEntry("cassette_ids", JsonSerializer.Serialize(entity.CassetteIds))
            };

            await _database.HashSetAsync($"{LOT_KEY_PREFIX}{entity.Id}", hashEntries);
            return entity;
        }

        public async Task<IEnumerable<Lot>> AddRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => AddAsync(entity, cancellationToken));
            return await Task.WhenAll(tasks);
        }

        public async Task<Lot> UpdateAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken); // HSet은 업데이트와 추가가 동일
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => UpdateAsync(entity, cancellationToken));
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            // Lot 삭제 시 관련된 LotStep들도 함께 삭제
            var lotSteps = await GetLotStepsByLotIdAsync(id, cancellationToken);
            foreach (var step in lotSteps)
            {
                await _database.KeyDeleteAsync($"{LOT_STEP_KEY_PREFIX}{step.Id}");
            }

            return await _database.KeyDeleteAsync($"{LOT_KEY_PREFIX}{id}");
        }

        public async Task<bool> DeleteAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            var tasks = entities.Select(entity => DeleteAsync(entity, cancellationToken));
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Lot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var lots = await GetAsync(predicate, cancellationToken);
            return lots.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Lot, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{LOT_KEY_PREFIX}*").ToArray();
                return keys.Length;
            }

            var filteredLots = await GetAsync(predicate, cancellationToken);
            return filteredLots.Count;
        }

        // ILotRepository 특화 메서드들
        public async Task<IReadOnlyList<Lot>> GetLotsByStatusAsync(ELotStatus status, CancellationToken cancellationToken = default)
        {
            var allLots = await GetAllAsync(cancellationToken);
            return allLots.Where(lot => lot.Status == status).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Lot>> GetLotsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var allLots = await GetAllAsync(cancellationToken);
            return allLots.Where(lot => lot.ReceivedTime >= startDate && lot.ReceivedTime <= endDate).ToList().AsReadOnly();
        }

        public async Task<bool> AddLotStepAsync(string lotId, LotStep lotStep, CancellationToken cancellationToken = default)
        {
            var lot = await GetByIdAsync(lotId, cancellationToken);
            if (lot == null) return false;

            lot.LotSteps.Add(lotStep);
            await UpdateAsync(lot, cancellationToken);
            return true;
        }

        public async Task<bool> UpdateLotStepStatusAsync(string lotId, string stepId, ELotStepStatus status, CancellationToken cancellationToken = default)
        {
            // LotStep은 별도 저장소에서 관리되므로 LotStepRepository에서 처리
            return true;
        }

        // 완전한 Lot 객체 생성을 위한 비동기 변환 메서드
        private async Task<Lot> ConvertHashToLotAsync(string id, HashEntry[] hashEntries, CancellationToken cancellationToken = default)
        {
            var cassetteIdsJson = hashEntries.FirstOrDefault(e => e.Name == "cassette_ids").Value;
            var cassetteIds = !cassetteIdsJson.IsNull
                ? JsonSerializer.Deserialize<List<string>>(cassetteIdsJson!)
                : new List<string>();

            var lot = new Lot(
                id: id,
                name: GetHashValue(hashEntries, "name"),
                status: Enum.Parse<ELotStatus>(GetHashValue(hashEntries, "status")),
                priority: GetHashValueAsInt(hashEntries, "priority"),
                receivedTime: DateTime.Parse(GetHashValue(hashEntries, "received_time")),
                purpose: GetHashValue(hashEntries, "purpose"),
                evalNo: GetHashValue(hashEntries, "eval_no"),
                partNo: GetHashValue(hashEntries, "part_no"),
                qty: GetHashValueAsInt(hashEntries, "qty"),
                option: GetHashValue(hashEntries, "option"),
                line: GetHashValue(hashEntries, "line"),
                cassetteIds: cassetteIds
            );

            // 관련된 LotStep들 조회 및 로드
            var lotSteps = await GetLotStepsByLotIdAsync(id, cancellationToken);
            lot.LotSteps.AddRange(lotSteps);

            return lot;
        }

        // LotId로 LotStep들 조회
        private async Task<List<LotStep>> GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken = default)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{LOT_STEP_KEY_PREFIX}*");

            var lotSteps = new List<LotStep>();
            foreach (var key in keys)
            {
                var stepId = key.ToString().Substring(LOT_STEP_KEY_PREFIX.Length);
                var stepHashEntries = await _database.HashGetAllAsync(key);

                if (stepHashEntries.Length > 0)
                {
                    var stepLotId = GetHashValue(stepHashEntries, "lot_id");
                    if (stepLotId == lotId)
                    {
                        var lotStep = await ConvertHashToLotStepAsync(stepId, stepHashEntries, cancellationToken);
                        lotSteps.Add(lotStep);
                    }
                }
            }

            return lotSteps;
        }

        // LotStep을 완전한 객체로 변환
        private async Task<LotStep> ConvertHashToLotStepAsync(string id, HashEntry[] hashEntries, CancellationToken cancellationToken = default)
        {
            var lotStep = new LotStep(
                id: id,
                lotId: GetHashValue(hashEntries, "lot_id"),
                name: GetHashValue(hashEntries, "name"),
                loadingType: GetHashValueAsInt(hashEntries, "loading_type"),
                dpcType: GetHashValue(hashEntries, "dpc_type"),
                chipset: GetHashValue(hashEntries, "chipset"),
                pgm: GetHashValue(hashEntries, "pgm"),
                planPercent: GetHashValueAsInt(hashEntries, "plan_percent"),
                status: Enum.Parse<ELotStatus>(GetHashValue(hashEntries, "status"))
            );

            // Cassette 객체들 로드
            var cassetteIdsJson = GetHashValue(hashEntries, "cassette_ids");
            if (!string.IsNullOrEmpty(cassetteIdsJson))
            {
                var cassetteIds = JsonSerializer.Deserialize<List<string>>(cassetteIdsJson) ?? new List<string>();
                foreach (string cassetteId in cassetteIds)
                {
                    var cassette = await GetCassetteByIdAsync(cassetteId, cancellationToken);
                    if (cassette != null)
                        lotStep.Cassettes.Add(cassette);
                }
            }

            // PlanGroup 객체들 로드
            var planGroupIdsJson = GetHashValue(hashEntries, "plan_group_ids");
            if (!string.IsNullOrEmpty(planGroupIdsJson))
            {
                var planGroupIds = JsonSerializer.Deserialize<List<string>>(planGroupIdsJson) ?? new List<string>();
                foreach (string planGroupId in planGroupIds)
                {
                    var planGroup = await GetPlanGroupByIdAsync(planGroupId, cancellationToken);
                    if (planGroup != null)
                        lotStep.PlanGroups.Add(planGroup);
                }
            }

            return lotStep;
        }

        // cassette:{cassetteId}에서 Cassette 객체 조회
        private async Task<Cassette?> GetCassetteByIdAsync(string cassetteId, CancellationToken cancellationToken = default)
        {
            var hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{cassetteId}");
            if (hashEntries.Length == 0)
                return null;

            var cassetteName = GetHashValue(hashEntries, "name");

            // Tray 목록 로드
            var trayIdsJson = GetHashValue(hashEntries, "tray_ids");
            var trays = new List<Tray>();

            if (!string.IsNullOrEmpty(trayIdsJson))
            {
                var trayIds = JsonSerializer.Deserialize<List<string>>(trayIdsJson) ?? new List<string>();
                foreach (string trayId in trayIds)
                {
                    var tray = await GetTrayByIdAsync(trayId, cancellationToken);
                    if (tray != null)
                        trays.Add(tray);
                }
            }

            return new Cassette(cassetteId, cassetteName, trays);
        }

        // tray:{trayId}에서 Tray 객체 조회
        private async Task<Tray?> GetTrayByIdAsync(string trayId, CancellationToken cancellationToken = default)
        {
            var hashEntries = await _database.HashGetAllAsync($"{TRAY_KEY_PREFIX}{trayId}");
            if (hashEntries.Length == 0)
                return null;

            // 실제 Tray 생성자에 맞게 수정 필요
            // 임시로 빈 Tray 객체 반환
            return null;
        }

        // plan_group:{planGroupId}에서 PlanGroup 객체 조회
        private async Task<PlanGroup?> GetPlanGroupByIdAsync(string planGroupId, CancellationToken cancellationToken = default)
        {
            var hashEntries = await _database.HashGetAllAsync($"{PLAN_GROUP_KEY_PREFIX}{planGroupId}");
            if (hashEntries.Length == 0)
                return null;

            var planGroupName = GetHashValue(hashEntries, "name");
            var groupTypeStr = GetHashValue(hashEntries, "group_type");

            if (Enum.TryParse<EPlanGroupType>(groupTypeStr, out var groupType))
            {
                return new PlanGroup(planGroupId, planGroupName, groupType);
            }

            return null;
        }

        // 헬퍼 메서드들
        private static string GetHashValue(HashEntry[] hashEntries, string fieldName)
        {
            return hashEntries.FirstOrDefault(e => e.Name == fieldName).Value.ToString();
        }

        private static int GetHashValueAsInt(HashEntry[] hashEntries, string fieldName)
        {
            var value = GetHashValue(hashEntries, fieldName);
            return int.TryParse(value, out var result) ? result : 0;
        }

        Task<IReadOnlyList<LotStep>> ILotRepository.GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<LotStep?> GetLotStepByIdAsync(string lotId, string stepId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddCassetteToStepAsync(string lotId, string stepId, string cassetteId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}