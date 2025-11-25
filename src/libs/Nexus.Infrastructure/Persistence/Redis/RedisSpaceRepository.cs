using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.ValueObjects;
using Nexus.Core.Domain.Shared.Bases;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    /// <summary>
    /// Space/CarrierLocation을 Redis에 저장/조회하는 단순 리포지토리입니다.
    /// </summary>
    public class RedisSpaceRepository : ISpaceRepository
    {
        private readonly IDatabase _database;

        private const string SPACES_ALL_KEY = "spaces:all";
        private const string SPACE_KEY_PREFIX = "space:";
        private const string CARRIER_LOCATION_KEY_PREFIX = "carrier_location:";
        private const string ID_SEPARATOR = ",";

        public RedisSpaceRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task<IReadOnlyList<Space>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            RedisValue[] ids = await _database.SetMembersAsync(SPACES_ALL_KEY);
            List<Space> spaces = new List<Space>();
            foreach (RedisValue id in ids)
            {
                Space? space = await GetByIdAsync(id!, cancellationToken);
                if (space != null)
                {
                    spaces.Add(space);
                }
            }
            return spaces.AsReadOnly();
        }

        public async Task<IReadOnlyList<Space>> GetAsync(Expression<Func<Space, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Space> all = await GetAllAsync(cancellationToken);
            Func<Space, bool> compiled = predicate.Compile();
            return all.Where(compiled).ToList().AsReadOnly();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Space, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Space> all = await GetAllAsync(cancellationToken);
            Func<Space, bool> compiled = predicate.Compile();
            return all.Any(compiled);
        }

        public async Task<int> CountAsync(Expression<Func<Space, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Space> all = await GetAllAsync(cancellationToken);
            if (predicate == null)
            {
                return all.Count;
            }
            Func<Space, bool> compiled = predicate.Compile();
            return all.Count(compiled);
        }

        public async Task<Space?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            HashEntry[] hash = await _database.HashGetAllAsync($"{SPACE_KEY_PREFIX}{id}");
            if (hash.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hash, "name");
            string specCode = Helper.GetHashValue(hash, "spec_code");
            string carrierLocationIdsValue = Helper.GetHashValue(hash, "carrier_location_ids");
            string childSpaceIdsValue = Helper.GetHashValue(hash, "child_space_ids");

            List<CarrierLocation> carrierLocations = new List<CarrierLocation>();
            if (!string.IsNullOrWhiteSpace(carrierLocationIdsValue))
            {
                string[] ids = carrierLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string locId in ids)
                {
                    CarrierLocation? cl = await GetCarrierLocationByIdAsync(locId);
                    if (cl != null)
                    {
                        carrierLocations.Add(cl);
                    }
                }
            }

            List<ISpace> childSpaces = new List<ISpace>();
            if (!string.IsNullOrWhiteSpace(childSpaceIdsValue))
            {
                string[] ids = childSpaceIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string childId in ids)
                {
                    Space? child = await GetByIdAsync(childId, cancellationToken);
                    if (child != null)
                    {
                        childSpaces.Add(child);
                    }
                }
            }

            SpaceSpecification spec = new SpaceSpecification(specCode, new List<string>(), new List<Core.Domain.Models.Transports.Enums.ECargoKind>());
            return new Space(id, name, spec, carrierLocations, childSpaces);
        }

        public async Task<Space> AddAsync(Space entity, CancellationToken cancellationToken = default)
        {
            await SaveSpaceAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<Space>> AddRangeAsync(IEnumerable<Space> entities, CancellationToken cancellationToken = default)
        {
            List<Space> saved = new List<Space>();
            foreach (Space space in entities)
            {
                Space savedSpace = await AddAsync(space, cancellationToken);
                saved.Add(savedSpace);
            }
            return saved;
        }

        public async Task<Space> UpdateAsync(Space entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Space> entities, CancellationToken cancellationToken = default)
        {
            foreach (Space space in entities)
            {
                await UpdateAsync(space, cancellationToken);
            }
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _database.KeyDeleteAsync($"{SPACE_KEY_PREFIX}{id}");
            await _database.SetRemoveAsync(SPACES_ALL_KEY, id);
            return true;
        }

        public async Task<bool> DeleteAsync(Space entity, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Space> entities, CancellationToken cancellationToken = default)
        {
            foreach (Space space in entities)
            {
                await DeleteAsync(space, cancellationToken);
            }
            return true;
        }

        private async Task SaveSpaceAsync(Space space)
        {
            string carrierLocationIds = string.Join(ID_SEPARATOR, space.CarrierLocations.Select(cl => cl.Id));
            string childSpaceIds = string.Join(ID_SEPARATOR, space.Spaces.Select(s =>
            {
                Space? child = s as Space;
                if (child != null)
                {
                    return child.Id;
                }
                return string.Empty;
            }).Where(id => !string.IsNullOrEmpty(id)));

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", space.Id),
                new HashEntry("name", space.Name),
                new HashEntry("spec_code", space.Specification.Code),
                new HashEntry("carrier_location_ids", carrierLocationIds),
                new HashEntry("child_space_ids", childSpaceIds)
            };

            await _database.HashSetAsync($"{SPACE_KEY_PREFIX}{space.Id}", entries);
            await _database.SetAddAsync(SPACES_ALL_KEY, space.Id);

            foreach (CarrierLocation cl in space.CarrierLocations)
            {
                await SaveCarrierLocationAsync(cl);
            }
        }

        private async Task<CarrierLocation?> GetCarrierLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{CARRIER_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            string specCode = Helper.GetHashValue(hashEntries, "spec_code");
            string allowedKindValue = Helper.GetHashValue(hashEntries, "allowed_kind");
            string currentItemId = Helper.GetHashValue(hashEntries, "current_item_id");
            string parentId = Helper.GetHashValue(hashEntries, "parent_id");
            string childrenValue = Helper.GetHashValue(hashEntries, "children");
            string isVisibleValue = Helper.GetHashValue(hashEntries, "is_visible");
            string isRelativePositionValue = Helper.GetHashValue(hashEntries, "is_relative_position");
            string xValue = Helper.GetHashValue(hashEntries, "x");
            string yValue = Helper.GetHashValue(hashEntries, "y");
            string zValue = Helper.GetHashValue(hashEntries, "z");
            string widthValue = Helper.GetHashValue(hashEntries, "width");
            string heightValue = Helper.GetHashValue(hashEntries, "height");
            string depthValue = Helper.GetHashValue(hashEntries, "depth");
            string rotateXValue = Helper.GetHashValue(hashEntries, "rotate_x");
            string rotateYValue = Helper.GetHashValue(hashEntries, "rotate_y");
            string rotateZValue = Helper.GetHashValue(hashEntries, "rotate_z");

            Core.Domain.Models.Transports.Enums.ECargoKind allowedKind = Core.Domain.Models.Transports.Enums.ECargoKind.Unknown;
            if (!string.IsNullOrWhiteSpace(allowedKindValue))
            {
                Enum.TryParse(allowedKindValue, out allowedKind);
            }

            CarrierLocation cl = new CarrierLocation(id, name, specCode, allowedKind);
            cl.CurrentItemId = currentItemId ?? string.Empty;
            cl.ParentId = parentId ?? string.Empty;
            cl.Children = string.IsNullOrWhiteSpace(childrenValue) ? new List<string>() : childrenValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            cl.IsVisible = string.Equals(isVisibleValue, "True", StringComparison.OrdinalIgnoreCase);
            cl.IsRelativePosition = string.Equals(isRelativePositionValue, "True", StringComparison.OrdinalIgnoreCase);
            cl.Position = new Nexus.Core.Domain.Shared.Bases.Position(ConvertToUInt(xValue), ConvertToUInt(yValue), ConvertToUInt(zValue));
            cl.Width = ConvertToUInt(widthValue);
            cl.Height = ConvertToUInt(heightValue);
            cl.Depth = ConvertToUInt(depthValue);
            cl.Rotation = new Nexus.Core.Domain.Shared.Bases.Rotation(ConvertToInt(rotateXValue), ConvertToInt(rotateYValue), ConvertToInt(rotateZValue));
            return cl;
        }

        private async Task SaveCarrierLocationAsync(CarrierLocation loc)
        {
            string currentItemId = loc.CurrentItemId ?? string.Empty;
            string childrenValue = string.Join(",", loc.Children);
            string parentId = loc.ParentId ?? string.Empty;

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", loc.Id),
                new HashEntry("name", loc.Name),
                new HashEntry("spec_code", loc.SpecificationCode),
                new HashEntry("allowed_kind", loc.AllowedKind.ToString()),
                new HashEntry("status", loc.Status.ToString()),
                new HashEntry("current_item_id", currentItemId),
                new HashEntry("parent_id", parentId),
                new HashEntry("children", childrenValue),
                new HashEntry("is_visible", loc.IsVisible.ToString()),
                new HashEntry("is_relative_position", loc.IsRelativePosition.ToString()),
                new HashEntry("x", loc.Position.X.ToString()),
                new HashEntry("y", loc.Position.Y.ToString()),
                new HashEntry("z", loc.Position.Z.ToString()),
                new HashEntry("width", loc.Width.ToString()),
                new HashEntry("height", loc.Height.ToString()),
                new HashEntry("depth", loc.Depth.ToString()),
                new HashEntry("rotate_x", loc.Rotation.X.ToString()),
                new HashEntry("rotate_y", loc.Rotation.Y.ToString()),
                new HashEntry("rotate_z", loc.Rotation.Z.ToString())
            };

            await _database.HashSetAsync($"{CARRIER_LOCATION_KEY_PREFIX}{loc.Id}", entries);
        }

        private static uint ConvertToUInt(string value)
        {
            if (uint.TryParse(value, out uint parsed))
            {
                return parsed;
            }
            return 0;
        }

        private static int ConvertToInt(string value)
        {
            if (int.TryParse(value, out int parsed))
            {
                return parsed;
            }
            return 0;
        }
    }
}
