using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using Nexus.Core.Domain.Models.Plans;
using Nexus.Core.Domain.Models.Plans.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using StackExchange.Redis;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisLotRepository : ILotRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ITransportRepository _transportRepository;
        private const string LOT_KEY_PREFIX = "lot:";
        private const string LOT_STEP_KEY_PREFIX = "lot_step:";
        private const string LOTS_ALL_KEY = "lots:all";
        private const string LOT_STEPS_ALL_KEY = "lot_steps:all";
        private const string LOT_STEPS_BY_LOT_PREFIX = "lot:steps:"; // per-lot step id set
        private const string PLAN_GROUP_KEY_PREFIX = "plan_group:";
        private const string PLAN_GROUP_PLANS_SET_PREFIX = "plan_group:plans:";
        private const string PLAN_KEY_PREFIX = "plan:";
        private const string PLAN_STEPS_SET_PREFIX = "plan:steps:";
        private const string PLAN_STEP_KEY_PREFIX = "plan_step:";
        private const string PLAN_STEP_JOBS_SET_PREFIX = "plan_step:jobs:";
        private const string JOB_KEY_PREFIX = "job:";

        private const string ID_SEPARATOR = ",";

        public RedisLotRepository(IConnectionMultiplexer redis, ITransportRepository transportRepository)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _transportRepository = transportRepository;
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
            RedisValue[] ids = await _database.SetMembersAsync(LOTS_ALL_KEY);

            List<Lot> lots = new List<Lot>();
            foreach (RedisValue val in ids)
            {
                string id = val.ToString();
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

            // Maintain lots:all set
            await _database.SetAddAsync(LOTS_ALL_KEY, entity.Id);

            // Sync per-lot step id set with current entity state (no KEYS usage)
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{entity.Id}";
            RedisValue[] existingStepIds = await _database.SetMembersAsync(perLotStepSetKey);
            HashSet<string> existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RedisValue rv in existingStepIds)
            {
                existing.Add(rv.ToString());
            }

            HashSet<string> desired = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LotStep s in entity.LotSteps)
            {
                if (s != null && s.Id != null)
                {
                    desired.Add(s.Id);
                }
            }

            // Remove no-longer-desired step ids from per-lot set and global set
            foreach (string stepId in existing)
            {
                if (!desired.Contains(stepId))
                {
                    await _database.SetRemoveAsync(perLotStepSetKey, stepId);
                    await _database.SetRemoveAsync(LOT_STEPS_ALL_KEY, stepId);
                }
            }

            // Save/Upsert each step and ensure membership in sets
            foreach (LotStep lotStep in entity.LotSteps)
            {
                await SaveLotStepAsync(lotStep, cancellationToken);
                await _database.SetAddAsync(perLotStepSetKey, lotStep.Id);
                await _database.SetAddAsync(LOT_STEPS_ALL_KEY, lotStep.Id);
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
            // Delete associated lot steps using per-lot step id set (no KEYS)
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{id}";
            RedisValue[] stepIds = await _database.SetMembersAsync(perLotStepSetKey);
            foreach (RedisValue rv in stepIds)
            {
                string stepId = rv.ToString();
                if (!string.IsNullOrEmpty(stepId))
                {
                    await _database.KeyDeleteAsync($"{LOT_STEP_KEY_PREFIX}{stepId}");
                    await _database.SetRemoveAsync(LOT_STEPS_ALL_KEY, stepId);
                }
            }
            // Remove the per-lot step id set
            await _database.KeyDeleteAsync(perLotStepSetKey);

            // Remove lot id from the global set
            await _database.SetRemoveAsync(LOTS_ALL_KEY, id);

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
                long count = await _database.SetLengthAsync(LOTS_ALL_KEY);
                if (count < 0)
                {
                    return 0;
                }
                return (int)count;
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
                string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{lot.Id}";
                RedisValue[] stepIds = await _database.SetMembersAsync(perLotStepSetKey);
                foreach (RedisValue rv in stepIds)
                {
                    string sid = rv.ToString();
                    if (!string.IsNullOrEmpty(sid))
                    {
                        referenced.Add(sid);
                    }
                }
            }

            // Compare with global step id set
            RedisValue[] allStepIds = await _database.SetMembersAsync(LOT_STEPS_ALL_KEY);
            int removed = 0;
            foreach (RedisValue rv in allStepIds)
            {
                string stepId = rv.ToString();
                if (!referenced.Contains(stepId))
                {
                    bool deleted = await _database.KeyDeleteAsync($"{LOT_STEP_KEY_PREFIX}{stepId}");
                    await _database.SetRemoveAsync(LOT_STEPS_ALL_KEY, stepId);
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

            // Ensure step id sets are updated
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{lotId}";
            await _database.SetAddAsync(perLotStepSetKey, lotStep.Id);
            await _database.SetAddAsync(LOT_STEPS_ALL_KEY, lotStep.Id);
            return true;
        }

        public async Task<bool> UpdateLotStepStatusAsync(string lotId, string stepId, ELotStepStatus status, CancellationToken cancellationToken = default)
        {
            // [cleaned]
            return await Task.FromResult(true);
        }

        public async Task<IReadOnlyList<LotStep>> GetLotStepsByLotIdAsync(string lotId, CancellationToken cancellationToken = default)
        {
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{lotId}";
            RedisValue[] stepIds = await _database.SetMembersAsync(perLotStepSetKey);
            List<LotStep> lotSteps = new List<LotStep>();
            foreach (RedisValue rv in stepIds)
            {
                string stepId = rv.ToString();
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

            // Load lot steps from per-lot step id set (no KEYS, no hash list dependency)
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{id}";
            RedisValue[] stepIds = await _database.SetMembersAsync(perLotStepSetKey);
            foreach (RedisValue rv in stepIds)
            {
                string stepId = rv.ToString();
                if (!string.IsNullOrEmpty(stepId))
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
                    ITransportable? transport = await _transportRepository.GetByIdAsync(cassetteId, cancellationToken);
                    Cassette? cassette = transport as Cassette;
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

        // Cassette/Tray 조회는 TransportRepository를 통해 위임 처리합니다.

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
                PlanGroup pg = new PlanGroup(planGroupId, planGroupName, groupType);

                // Optional: Load nested plans and steps (no location resolution for jobs here)
                RedisValue[] planIds = await _database.SetMembersAsync($"{PLAN_GROUP_PLANS_SET_PREFIX}{planGroupId}");
                foreach (RedisValue pid in planIds)
                {
                    string planId = pid.ToString();
                    if (string.IsNullOrEmpty(planId))
                    {
                        continue;
                    }

                    HashEntry[] planHash = await _database.HashGetAllAsync($"{PLAN_KEY_PREFIX}{planId}");
                    if (planHash.Length == 0)
                    {
                        continue;
                    }

                    string planName = Helper.GetHashValue(planHash, "name");
                    Plan plan = new Plan(planId, planName);

                    // Load steps
                    RedisValue[] stepIds = await _database.SetMembersAsync($"{PLAN_STEPS_SET_PREFIX}{planId}");
                    foreach (RedisValue sid in stepIds)
                    {
                        string stepId = sid.ToString();
                        if (string.IsNullOrEmpty(stepId))
                        {
                            continue;
                        }

                        HashEntry[] stepHash = await _database.HashGetAllAsync($"{PLAN_STEP_KEY_PREFIX}{stepId}");
                        if (stepHash.Length == 0)
                        {
                            continue;
                        }

                        string stepName = Helper.GetHashValue(stepHash, "name");
                        int stepNo = Helper.GetHashValueAsInt(stepHash, "step_no");
                        string position = Helper.GetHashValue(stepHash, "position");
                        string actionStr = Helper.GetHashValue(stepHash, "action");
                        string statusStr = Helper.GetHashValue(stepHash, "status");

                        EPlanStepAction action;
                        if (!Enum.TryParse<EPlanStepAction>(actionStr, out action))
                        {
                            action = EPlanStepAction.None;
                        }
                        EPlanStepStatus stepStatus;
                        if (!Enum.TryParse<EPlanStepStatus>(statusStr, out stepStatus))
                        {
                            stepStatus = EPlanStepStatus.Pending;
                        }

                        PlanStep planStep = new PlanStep(stepId, stepName, stepNo, action, position);
                        planStep.Status = stepStatus;

                        // carrier ids (optional)
                        string carrierIdsVal = Helper.GetHashValue(stepHash, "carrier_ids");
                        if (!string.IsNullOrEmpty(carrierIdsVal))
                        {
                            List<string> carriers = carrierIdsVal.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList();
                            planStep.CarrierIds = carriers;
                        }

                        // Load jobs for this plan step
                        RedisValue[] jobIds = await _database.SetMembersAsync($"{PLAN_STEP_JOBS_SET_PREFIX}{stepId}");
                        foreach (RedisValue jrv in jobIds)
                        {
                            string jobId = jrv.ToString();
                            if (string.IsNullOrEmpty(jobId))
                            {
                                continue;
                            }

                            HashEntry[] jobHash = await _database.HashGetAllAsync($"{JOB_KEY_PREFIX}{jobId}");
                            if (jobHash.Length == 0)
                            {
                                continue;
                            }

                            string jobName = Helper.GetHashValue(jobHash, "name");
                            int jobNo = Helper.GetHashValueAsInt(jobHash, "job_no");
                            string fromLocationId = Helper.GetHashValue(jobHash, "from_location_id");
                            string toLocationId = Helper.GetHashValue(jobHash, "to_location_id");
                            string jobStatusStr = Helper.GetHashValue(jobHash, "status");

                            Job job = new Job(jobId, jobName, jobNo, fromLocationId, toLocationId);
                            EJobStatus jobStatus;
                            if (Enum.TryParse<EJobStatus>(jobStatusStr, out jobStatus))
                            {
                                job.Status = jobStatus;
                            }

                            planStep.Jobs.Add(job);
                        }

                        plan.PlanSteps.Add(planStep);
                    }

                    pg.Plans.Add(plan);
                }

                return pg;
            }

            return null;
        }

        private async Task SaveLotStepAsync(LotStep lotStep, CancellationToken cancellationToken = default)
        {
            List<string> cassetteIdsToPersist = new List<string>();
            if (lotStep.CassetteIds != null && lotStep.CassetteIds.Count > 0)
            {
                cassetteIdsToPersist = lotStep.CassetteIds;
            }
            else
            {
                foreach (Cassette cassette in lotStep.Cassettes)
                {
                    cassetteIdsToPersist.Add(cassette.Id);
                }
            }

            // LotStep에 포함된 카세트가 전달된 경우 TransportRepository를 통해 등록을 위임합니다.
            if (lotStep.Cassettes != null && lotStep.Cassettes.Count > 0)
            {
                foreach (Cassette cassette in lotStep.Cassettes)
                {
                    await _transportRepository.AddAsync(cassette, cancellationToken);
                }
            }

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

            // Maintain step id indexes
            string perLotStepSetKey = $"{LOT_STEPS_BY_LOT_PREFIX}{lotStep.LotId}";
            await _database.SetAddAsync(perLotStepSetKey, lotStep.Id);
            await _database.SetAddAsync(LOT_STEPS_ALL_KEY, lotStep.Id);

            // Persist plan groups and nested plans/steps/jobs
            if (lotStep.PlanGroups != null)
            {
                foreach (PlanGroup pg in lotStep.PlanGroups)
                {
                    await SavePlanGroupAsync(pg, cancellationToken);
                }
            }
        }

        private async Task SavePlanGroupAsync(PlanGroup planGroup, CancellationToken cancellationToken)
        {
            HashEntry[] pgHash = new HashEntry[]
            {
                new HashEntry("id", planGroup.Id),
                new HashEntry("name", planGroup.Name),
                new HashEntry("group_type", planGroup.GroupType.ToString())
            };

            await _database.HashSetAsync($"{PLAN_GROUP_KEY_PREFIX}{planGroup.Id}", pgHash);

            if (planGroup.Plans != null)
            {
                foreach (Plan plan in planGroup.Plans)
                {
                    await SavePlanAsync(planGroup.Id, plan, cancellationToken);
                }
            }
        }

        private async Task SavePlanAsync(string planGroupId, Plan plan, CancellationToken cancellationToken)
        {
            HashEntry[] pHash = new HashEntry[]
            {
                new HashEntry("id", plan.Id),
                new HashEntry("name", plan.Name),
                new HashEntry("plan_group_id", planGroupId)
            };

            await _database.HashSetAsync($"{PLAN_KEY_PREFIX}{plan.Id}", pHash);
            await _database.SetAddAsync($"{PLAN_GROUP_PLANS_SET_PREFIX}{planGroupId}", plan.Id);

            if (plan.PlanSteps != null)
            {
                foreach (PlanStep step in plan.PlanSteps)
                {
                    await SavePlanStepAsync(plan.Id, step, cancellationToken);
                }
            }
        }

        private async Task SavePlanStepAsync(string planId, PlanStep step, CancellationToken cancellationToken)
        {
            string carriers = string.Empty;
            if (step.CarrierIds != null && step.CarrierIds.Count > 0)
            {
                carriers = string.Join(ID_SEPARATOR, step.CarrierIds);
            }

            HashEntry[] sHash = new HashEntry[]
            {
                new HashEntry("id", step.Id),
                new HashEntry("name", step.Name),
                new HashEntry("plan_id", planId),
                new HashEntry("step_no", step.StepNo),
                new HashEntry("position", step.Position),
                new HashEntry("action", step.Action.ToString()),
                new HashEntry("status", step.Status.ToString()),
                new HashEntry("carrier_ids", carriers)
            };

            await _database.HashSetAsync($"{PLAN_STEP_KEY_PREFIX}{step.Id}", sHash);
            await _database.SetAddAsync($"{PLAN_STEPS_SET_PREFIX}{planId}", step.Id);

            if (step.Jobs != null)
            {
                foreach (Job job in step.Jobs)
                {
                    await SaveJobAsync(step.Id, job, cancellationToken);
                }
            }
        }

        private async Task SaveJobAsync(string planStepId, Job job, CancellationToken cancellationToken)
        {
            string fromId = string.Empty;
            if (job.FromLocationId != null)
            {
                fromId = job.FromLocationId;
            }
            string toId = string.Empty;
            if (job.ToLocationId != null)
            {
                toId = job.ToLocationId;
            }

            HashEntry[] jHash = new HashEntry[]
            {
                new HashEntry("id", job.Id),
                new HashEntry("name", job.Name),
                new HashEntry("plan_step_id", planStepId),
                new HashEntry("job_no", job.JobNo),
                new HashEntry("from_location_id", fromId),
                new HashEntry("to_location_id", toId),
                new HashEntry("status", job.Status.ToString())
            };

            await _database.HashSetAsync($"{JOB_KEY_PREFIX}{job.Id}", jHash);
            await _database.SetAddAsync($"{PLAN_STEP_JOBS_SET_PREFIX}{planStepId}", job.Id);
        }

    

     
    }
}






