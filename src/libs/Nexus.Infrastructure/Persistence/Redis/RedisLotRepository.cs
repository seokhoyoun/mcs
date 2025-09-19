using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Transports;
using StackExchange.Redis;
using System.Diagnostics;
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

        private const string ID_SEPARATOR = ",";

        public RedisLotRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<Lot?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{LOT_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            return await ConvertHashToLotAsync(id, hashEntries, cancellationToken);
        }

        public async Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IServer server = _redis.GetServer(_redis.GetEndPoints().First());
            IEnumerable<RedisKey> keys = server.Keys(pattern: $"{LOT_KEY_PREFIX}*");

            List<Lot> lots = new List<Lot>();
            foreach (RedisKey key in keys)
            {
                string id = key.ToString().Substring(LOT_KEY_PREFIX.Length);
                Lot? lot = await GetByIdAsync(id, cancellationToken);
                if (lot != null)
                {
                    lots.Add(lot);
                }
            }

            return lots.AsReadOnly();
        }

        public async Task<IReadOnlyList<Lot>> GetAsync(Expression<Func<Lot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Lot> allLots = await GetAllAsync(cancellationToken);
            Func<Lot, bool> compiledPredicate = predicate.Compile();
            return allLots.Where(compiledPredicate).ToList().AsReadOnly();
        }

        public async Task<Lot> AddAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = new HashEntry[]
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
                new HashEntry("cassette_ids", string.Join(ID_SEPARATOR, entity.CassetteIds)),
                new HashEntry("lot_step_ids", string.Join(ID_SEPARATOR, entity.LotSteps.Select(step => step.Id)))
            };

            await _database.HashSetAsync($"{LOT_KEY_PREFIX}{entity.Id}", hashEntries);

            // [cleaned]
            foreach (LotStep lotStep in entity.LotSteps)
            {
                await SaveLotStepAsync(lotStep, cancellationToken);
            }
            return entity;
        }

        public async Task<IEnumerable<Lot>> AddRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            Task<Lot>[] tasks = entities.Select(entity => AddAsync(entity, cancellationToken)).ToArray();
            return await Task.WhenAll(tasks);
        }

        public async Task<Lot> UpdateAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken); // [cleaned]
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            Task<Lot>[] tasks = entities.Select(entity => UpdateAsync(entity, cancellationToken)).ToArray();
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] lotHashEntries = await _database.HashGetAllAsync($"{LOT_KEY_PREFIX}{id}");
            if (lotHashEntries.Length > 0)
            {
                string lotStepIdsJson = Helper.GetHashValue(lotHashEntries, "lot_step_ids");
                if (!string.IsNullOrEmpty(lotStepIdsJson))
                {
                    List<string> lotStepIds = JsonSerializer.Deserialize<List<string>>(lotStepIdsJson) ?? new List<string>();

                    foreach (string stepId in lotStepIds)
                    {
                        await _database.KeyDeleteAsync($"{LOT_STEP_KEY_PREFIX}{stepId}");
                    }
                }
            }

            return await _database.KeyDeleteAsync($"{LOT_KEY_PREFIX}{id}");
        }

        public async Task<bool> DeleteAsync(Lot entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Lot> entities, CancellationToken cancellationToken = default)
        {
            Task<bool>[] tasks = entities.Select(entity => DeleteAsync(entity, cancellationToken)).ToArray();
            bool[] results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Lot, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Lot> lots = await GetAsync(predicate, cancellationToken);
            return lots.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Lot, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                IServer server = _redis.GetServer(_redis.GetEndPoints().First());
                RedisKey[] keys = server.Keys(pattern: $"{LOT_KEY_PREFIX}*").ToArray();
                return keys.Length;
            }

            IReadOnlyList<Lot> filteredLots = await GetAsync(predicate, cancellationToken);
            return filteredLots.Count;
        }

        // --- Additional maintenance helpers ---

        /// <summary>
        /// Remove a LotStep from a Lot and delete the underlying step record.
        /// Note: Not part of ILotRepository interface to maintain backwards-compatibility.
        /// </summary>
        public async Task<bool> RemoveLotStepAsync(string lotId, string stepId, CancellationToken cancellationToken = default)
        {
            Lot? lot = await GetByIdAsync(lotId, cancellationToken);
            if (lot == null)
            {
                return false;
            }

            List<LotStep> remaining = new List<LotStep>();
            foreach (LotStep step in lot.LotSteps)
            {
                if (step.Id != stepId)
                {
                    remaining.Add(step);
                }
            }

            lot.LotSteps = remaining;

            await UpdateAsync(lot, cancellationToken);

            await _database.KeyDeleteAsync($"{LOT_STEP_KEY_PREFIX}{stepId}");
            return true;
        }

        /// <summary>
        /// Cleanup orphan LotSteps that are not referenced by any Lot.
        /// Returns the number of removed step records.
        /// </summary>
        public async Task<int> CleanupOrphanLotStepsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Lot> lots = await GetAllAsync(cancellationToken);
            HashSet<string> referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Lot lot in lots)
            {
                foreach (LotStep step in lot.LotSteps)
                {
                    if (step.Id != null)
                    {
                        referenced.Add(step.Id);
                    }
                }
            }

            IServer server = _redis.GetServer(_redis.GetEndPoints().First());
            IEnumerable<RedisKey> stepKeys = server.Keys(pattern: $"{LOT_STEP_KEY_PREFIX}*");

            int removed = 0;
            foreach (RedisKey key in stepKeys)
            {
                string full = key.ToString();
                string stepId;
                if (full.StartsWith(LOT_STEP_KEY_PREFIX, StringComparison.Ordinal))
                {
                    stepId = full.Substring(LOT_STEP_KEY_PREFIX.Length);
                }
                else
                {
                    stepId = full;
                }

                if (!referenced.Contains(stepId))
                {
                    bool deleted = await _database.KeyDeleteAsync(key);
                    if (deleted)
                    {
                        removed++;
                    }
                }
            }

            return removed;
        }

        // [cleaned]
        public async Task<IReadOnlyList<Lot>> GetLotsByStatusAsync(ELotStatus status, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Lot> allLots = await GetAllAsync(cancellationToken);
            return allLots.Where(lot => lot.Status == status).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Lot>> GetLotsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Lot> allLots = await GetAllAsync(cancellationToken);
            return allLots.Where(lot => lot.ReceivedTime >= startDate && lot.ReceivedTime <= endDate).ToList().AsReadOnly();
        }

        public async Task<bool> AddLotStepAsync(string lotId, LotStep lotStep, CancellationToken cancellationToken = default)
        {
            Lot? lot = await GetByIdAsync(lotId, cancellationToken);
            if (lot == null)
            {
                return false;
            }

            lot.LotSteps.Add(lotStep);
            await UpdateAsync(lot, cancellationToken);
            return true;
        }

        public async Task<bool> UpdateLotStepStatusAsync(string lotId, string stepId, ELotStepStatus status, CancellationToken cancellationToken = default)
        {
            // [cleaned]
            return await Task.FromResult(true);
        }

        public async Task<IReadOnlyList<LotStep>> GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken = default)
        {
            // [cleaned]
            HashEntry[] lotHashEntries = await _database.HashGetAllAsync($"{LOT_KEY_PREFIX}{lotId}");
            if (lotHashEntries.Length == 0)
            {
                return new List<LotStep>();
            }

            string lotStepIdsJson = Helper.GetHashValue(lotHashEntries, "lot_step_ids");
            if (string.IsNullOrEmpty(lotStepIdsJson))
            {
                return new List<LotStep>();
            }

            List<string> lotStepIds = JsonSerializer.Deserialize<List<string>>(lotStepIdsJson) ?? new List<string>();
            List<LotStep> lotSteps = new List<LotStep>();

            foreach (string stepId in lotStepIds)
            {
                LotStep? lotStep = await GetLotStepByIdAsync(stepId, cancellationToken);
                if (lotStep != null)
                {
                    lotSteps.Add(lotStep);
                }
            }

            return lotSteps;
        }


        private async Task<Lot> ConvertHashToLotAsync(string id, HashEntry[] hashEntries, CancellationToken cancellationToken = default)
        {
            string cassetteIdsValue = hashEntries.FirstOrDefault(e => e.Name == "cassette_ids").Value; 
            List<string> cassetteIds = string.IsNullOrEmpty(cassetteIdsValue) ? new List<string>() : cassetteIdsValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            Debug.Assert(cassetteIds != null, "Cassette IDs should not be null");
            Lot lot = new Lot(
                id: id,
                name: Helper.GetHashValue(hashEntries, "name"),
                status: Enum.Parse<ELotStatus>(Helper.GetHashValue(hashEntries, "status")),
                priority: Helper.GetHashValueAsInt(hashEntries, "priority"),
                receivedTime: DateTime.Parse(Helper.GetHashValue(hashEntries, "received_time")),
                purpose: Helper.GetHashValue(hashEntries, "purpose"),
                evalNo: Helper.GetHashValue(hashEntries, "eval_no"),
                partNo: Helper.GetHashValue(hashEntries, "part_no"),
                qty: Helper.GetHashValueAsInt(hashEntries, "qty"),
                option: Helper.GetHashValue(hashEntries, "option"),
                line: Helper.GetHashValue(hashEntries, "line"),
                cassetteIds: cassetteIds
            );

            // [cleaned]
            string lotStepIdsValue = Helper.GetHashValue(hashEntries, "lot_step_ids"); if (!string.IsNullOrEmpty(lotStepIdsValue)) { List<string> lotStepIds = lotStepIdsValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string stepId in lotStepIds)
                {
                    LotStep? lotStep = await GetLotStepByIdAsync(stepId, cancellationToken);
                    if (lotStep != null)
                    {
                        lot.LotSteps.Add(lotStep);
                    }
                }
            }

            return lot;
        }


        private async Task<LotStep?> GetLotStepByIdAsync(string stepId, CancellationToken cancellationToken = default)
        {
            HashEntry[] stepHashEntries = await _database.HashGetAllAsync($"{LOT_STEP_KEY_PREFIX}{stepId}");
            if (stepHashEntries.Length == 0)
            {
                return null;
            }

            return await ConvertHashToLotStepAsync(stepId, stepHashEntries, cancellationToken);
        }

        private async Task<LotStep> ConvertHashToLotStepAsync(string id, HashEntry[] hashEntries, CancellationToken cancellationToken = default)
        {
            LotStep lotStep = new LotStep(
                id: id,
                lotId: Helper.GetHashValue(hashEntries, "lot_id"),
                name: Helper.GetHashValue(hashEntries, "name"),
                loadingType: Helper.GetHashValueAsInt(hashEntries, "loading_type"),
                dpcType: Helper.GetHashValue(hashEntries, "dpc_type"),
                chipset: Helper.GetHashValue(hashEntries, "chipset"),
                pgm: Helper.GetHashValue(hashEntries, "pgm"),
                planPercent: Helper.GetHashValueAsInt(hashEntries, "plan_percent"),
                status: Enum.Parse<ELotStatus>(Helper.GetHashValue(hashEntries, "status"))
            );

            string cassetteIdsValue = Helper.GetHashValue(hashEntries, "cassette_ids");
            if (!string.IsNullOrEmpty(cassetteIdsValue))
            {
                List<string> cassetteIds = cassetteIdsValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                lotStep.CassetteIds = cassetteIds;
                foreach (string cassetteId in cassetteIds)
                {
                    Cassette? cassette = await GetCassetteByIdAsync(cassetteId, cancellationToken);
                    if (cassette != null)
                    {
                        lotStep.Cassettes.Add(cassette);
                    }
                }
            }

            string planGroupIdsValue = Helper.GetHashValue(hashEntries, "plan_group_ids");
            if (!string.IsNullOrEmpty(planGroupIdsValue))
            {
                List<string> planGroupIds = planGroupIdsValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string planGroupId in planGroupIds)
                {
                    PlanGroup? planGroup = await GetPlanGroupByIdAsync(planGroupId, cancellationToken);
                    if (planGroup != null)
                    {
                        lotStep.PlanGroups.Add(planGroup);
                    }
                }
            }

            return lotStep;
        }

        private async Task<Cassette?> GetCassetteByIdAsync(string cassetteId, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{CASSETTE_KEY_PREFIX}{cassetteId}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string cassetteName = Helper.GetHashValue(hashEntries, "name");

            string trayIdsJson = Helper.GetHashValue(hashEntries, "tray_ids");
            List<Tray> trays = new List<Tray>();

            if (!string.IsNullOrEmpty(trayIdsJson))
            {
                List<string> trayIds = JsonSerializer.Deserialize<List<string>>(trayIdsJson) ?? new List<string>();
                foreach (string trayId in trayIds)
                {
                    Tray? tray = await GetTrayByIdAsync(trayId, cancellationToken);
                    if (tray != null)
                    {
                        trays.Add(tray);
                    }
                }
            }

            return new Cassette(cassetteId, cassetteName, trays);
        }

        private async Task<Tray?> GetTrayByIdAsync(string trayId, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{TRAY_KEY_PREFIX}{trayId}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            return null;
        }

        private async Task<PlanGroup?> GetPlanGroupByIdAsync(string planGroupId, CancellationToken cancellationToken = default)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{PLAN_GROUP_KEY_PREFIX}{planGroupId}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string planGroupName = Helper.GetHashValue(hashEntries, "name");
            string groupTypeStr = Helper.GetHashValue(hashEntries, "group_type");

            if (Enum.TryParse<EPlanGroupType>(groupTypeStr, out EPlanGroupType groupType))
            {
                return new PlanGroup(planGroupId, planGroupName, groupType);
            }

            return null;
        }

        private async Task SaveLotStepAsync(LotStep lotStep, CancellationToken cancellationToken = default)
        {
            List<string> cassetteIdsToPersist = (lotStep.CassetteIds != null && lotStep.CassetteIds.Count > 0)
                ? lotStep.CassetteIds
                : lotStep.Cassettes.Select(c => c.Id).ToList();

            HashEntry[] stepHashEntries = new HashEntry[]
            {
                new HashEntry("lot_id", lotStep.LotId),
                new HashEntry("name", lotStep.Name),
                new HashEntry("loading_type", lotStep.LoadingType),
                new HashEntry("dpc_type", lotStep.DpcType),
                new HashEntry("chipset", lotStep.Chipset),
                new HashEntry("pgm", lotStep.PGM),
                new HashEntry("plan_percent", lotStep.PlanPercent),
                new HashEntry("status", lotStep.Status.ToString()),
                new HashEntry("cassette_ids", string.Join(ID_SEPARATOR, cassetteIdsToPersist)),
                new HashEntry("plan_group_ids", string.Join(ID_SEPARATOR, lotStep.PlanGroups.Select(pg => pg.Id)))
            };

            await _database.HashSetAsync($"{LOT_STEP_KEY_PREFIX}{lotStep.Id}", stepHashEntries);
        }

    

     
    }
}






