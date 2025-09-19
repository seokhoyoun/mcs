using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Shared.Bases;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nexus.Infrastructure.Persistence.Redis
{
    public class RedisLocationRepository : ILocationRepository
    {
        #region Fields

        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        private const string AREA_KEY_PREFIX = "area:";

        private const string CASSETTE_LOCATION_KEY_PREFIX = "cassette_location:";
        private const string TRAY_LOCATION_KEY_PREFIX = "tray_location:";
        private const string MEMORY_LOCATION_KEY_PREFIX = "memory_location:";
        private const string MARKER_LOCATION_KEY_PREFIX = "marker_location:";

        private const string CASSETTE_LOCATIONS_ALL_KEY = "cassette_locations:all";
        private const string TRAY_LOCATIONS_ALL_KEY = "tray_locations:all";
        private const string MEMORY_LOCATIONS_ALL_KEY = "memory_locations:all";
        private const string MARKER_LOCATIONS_ALL_KEY = "marker_locations:all";

        private const string ID_SEPARATOR = ",";

        #endregion

        #region Constructor

        public RedisLocationRepository(IConnectionMultiplexer connection)
        {
            _redis = connection;
            _database = connection.GetDatabase();
        }

        #endregion

        #region IRepository<Location, string> Implementation

        public async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<Location> locations = new List<Location>();

            RedisValue[] cassetteIds = await _database.SetMembersAsync(CASSETTE_LOCATIONS_ALL_KEY);
            foreach (RedisValue id in cassetteIds)
            {
                CassetteLocation? loc = await GetCassetteLocationByIdAsync(id.ToString());
                if (loc != null)
                {
                    locations.Add(loc);
                }
            }

            RedisValue[] trayIds = await _database.SetMembersAsync(TRAY_LOCATIONS_ALL_KEY);
            foreach (RedisValue id in trayIds)
            {
                TrayLocation? loc = await GetTrayLocationByIdAsync(id.ToString());
                if (loc != null)
                {
                    locations.Add(loc);
                }
            }

            RedisValue[] memoryIds = await _database.SetMembersAsync(MEMORY_LOCATIONS_ALL_KEY);
            foreach (RedisValue id in memoryIds)
            {
                MemoryLocation? loc = await GetMemoryLocationByIdAsync(id.ToString());
                if (loc != null)
                {
                    locations.Add(loc);
                }
            }

            RedisValue[] markerIds = await _database.SetMembersAsync(MARKER_LOCATIONS_ALL_KEY);
            foreach (RedisValue id in markerIds)
            {
                MarkerLocation? loc = await GetMarkerLocationByIdAsync(id.ToString());
                if (loc != null)
                {
                    locations.Add(loc);
                }
            }

            return locations.AsReadOnly();
        }

        public async Task<IReadOnlyList<Location>> GetAsync(Expression<Func<Location, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Location> all = await GetAllAsync(cancellationToken);
            Func<Location, bool> compiled = predicate.Compile();
            return all.Where(compiled).ToList().AsReadOnly();
        }

        public async Task<Location?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            CassetteLocation? cassette = await GetCassetteLocationByIdAsync(id);
            if (cassette != null)
            {
                return cassette;
            }

            TrayLocation? tray = await GetTrayLocationByIdAsync(id);
            if (tray != null)
            {
                return tray;
            }

            MemoryLocation? memory = await GetMemoryLocationByIdAsync(id);
            if (memory != null)
            {
                return memory;
            }

            MarkerLocation? marker = await GetMarkerLocationByIdAsync(id);
            if (marker != null)
            {
                return marker;
            }

            return null;
        }

        public async Task<Location> AddAsync(Location entity, CancellationToken cancellationToken = default)
        {
            switch (entity)
            {
                case CassetteLocation cl:
                    await SaveCassetteLocationAsync(cl);
                    break;
                case TrayLocation tl:
                    await SaveTrayLocationAsync(tl);
                    break;
                case MemoryLocation ml:
                    await SaveMemoryLocationAsync(ml);
                    break;
                case MarkerLocation ml:
                    await SaveMarkerLocationAsync(ml);
                    break;
                default:
                    throw new ArgumentException($"Unsupported location type: {entity.GetType()}");
            }

            return entity;
        }

        public async Task<IEnumerable<Location>> AddRangeAsync(IEnumerable<Location> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<Location>> tasks = entities.Select(e => AddAsync(e, cancellationToken));
            return await Task.WhenAll(tasks);
        }

        public async Task<Location> UpdateAsync(Location entity, CancellationToken cancellationToken = default)
        {
            return await AddAsync(entity, cancellationToken);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<Location> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<Location>> tasks = entities.Select(e => UpdateAsync(e, cancellationToken));
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (await _database.KeyExistsAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{id}"))
            {
                await _database.KeyDeleteAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{id}");
                await _database.SetRemoveAsync(CASSETTE_LOCATIONS_ALL_KEY, id);
                return true;
            }
            if (await _database.KeyExistsAsync($"{TRAY_LOCATION_KEY_PREFIX}{id}"))
            {
                await _database.KeyDeleteAsync($"{TRAY_LOCATION_KEY_PREFIX}{id}");
                await _database.SetRemoveAsync(TRAY_LOCATIONS_ALL_KEY, id);
                return true;
            }
            if (await _database.KeyExistsAsync($"{MEMORY_LOCATION_KEY_PREFIX}{id}"))
            {
                await _database.KeyDeleteAsync($"{MEMORY_LOCATION_KEY_PREFIX}{id}");
                await _database.SetRemoveAsync(MEMORY_LOCATIONS_ALL_KEY, id);
                return true;
            }
            if (await _database.KeyExistsAsync($"{MARKER_LOCATION_KEY_PREFIX}{id}"))
            {
                await _database.KeyDeleteAsync($"{MARKER_LOCATION_KEY_PREFIX}{id}");
                await _database.SetRemoveAsync(MARKER_LOCATIONS_ALL_KEY, id);
                return true;
            }
            return false;
        }

        public Task<bool> DeleteAsync(Location entity, CancellationToken cancellationToken = default)
        {
            return DeleteAsync(entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<Location> entities, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<bool>> tasks = entities.Select(e => DeleteAsync(e, cancellationToken));
            bool[] results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Location, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Location> all = await GetAsync(predicate, cancellationToken);
            return all.Any();
        }

        public async Task<int> CountAsync(Expression<Func<Location, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                long cassette = await _database.SetLengthAsync(CASSETTE_LOCATIONS_ALL_KEY);
                long tray = await _database.SetLengthAsync(TRAY_LOCATIONS_ALL_KEY);
                long memory = await _database.SetLengthAsync(MEMORY_LOCATIONS_ALL_KEY);
                long marker = await _database.SetLengthAsync(MARKER_LOCATIONS_ALL_KEY);
                return (int)(cassette + tray + memory + marker);
            }

            IReadOnlyList<Location> filtered = await GetAsync(predicate, cancellationToken);
            return filtered.Count;
        }

        #endregion

        #region ILocationRepository Implementation

        public async Task<IReadOnlyList<Location>> GetLocationsByAreaAsync(string areaId, CancellationToken cancellationToken = default)
        {
            HashEntry[] areaHash = await _database.HashGetAllAsync($"{AREA_KEY_PREFIX}{areaId}");
            if (areaHash.Length == 0)
            {
                return Array.Empty<Location>();
            }

            List<Location> result = new List<Location>();

            string cassetteLocationIdsValue = Helper.GetHashValue(areaHash, "cassette_location_ids");
            string trayLocationIdsValue = Helper.GetHashValue(areaHash, "tray_location_ids");
            string setIdsValue = Helper.GetHashValue(areaHash, "set_ids");

            string[] cassetteLocationIds = cassetteLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            string[] trayLocationIds = trayLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            string[] setIds = setIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            foreach (string id in cassetteLocationIds)
            {
                CassetteLocation? loc = await GetCassetteLocationByIdAsync(id);
                if (loc != null)
                {
                    result.Add(loc);
                }
            }

            foreach (string id in trayLocationIds)
            {
                TrayLocation? loc = await GetTrayLocationByIdAsync(id);
                if (loc != null)
                {
                    result.Add(loc);
                }
            }

            foreach (string setId in setIds)
            {
                HashEntry[] setHash = await _database.HashGetAllAsync($"set:{setId}");
                if (setHash.Length == 0)
                {
                    continue;
                }

                string memoryLocationIdsValue = Helper.GetHashValue(setHash, "memory_location_ids");
                string[] memoryLocationIds = memoryLocationIdsValue.Split(ID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string memId in memoryLocationIds)
                {
                    MemoryLocation? mem = await GetMemoryLocationByIdAsync(memId);
                    if (mem != null)
                    {
                        result.Add(mem);
                    }
                }
            }

            return result.AsReadOnly();
        }

        public async Task<IReadOnlyList<Location>> GetLocationsByTypeAsync(ELocationType locationType, CancellationToken cancellationToken = default)
        {
            switch (locationType)
            {
                case ELocationType.Cassette:
                    {
                        List<CassetteLocation> list = new List<CassetteLocation>();
                        RedisValue[] ids = await _database.SetMembersAsync(CASSETTE_LOCATIONS_ALL_KEY);
                        foreach (RedisValue id in ids)
                        {
                            CassetteLocation? loc = await GetCassetteLocationByIdAsync(id.ToString());
                            if (loc != null)
                            {
                                list.Add(loc);
                            }
                        }
                        return list.Cast<Location>().ToList().AsReadOnly();
                    }
                case ELocationType.Tray:
                    {
                        List<TrayLocation> list = new List<TrayLocation>();
                        RedisValue[] ids = await _database.SetMembersAsync(TRAY_LOCATIONS_ALL_KEY);
                        foreach (RedisValue id in ids)
                        {
                            TrayLocation? loc = await GetTrayLocationByIdAsync(id.ToString());
                            if (loc != null)
                            {
                                list.Add(loc);
                            }
                        }
                        return list.Cast<Location>().ToList().AsReadOnly();
                    }
                case ELocationType.Memory:
                    {
                        List<MemoryLocation> list = new List<MemoryLocation>();
                        RedisValue[] ids = await _database.SetMembersAsync(MEMORY_LOCATIONS_ALL_KEY);
                        foreach (RedisValue id in ids)
                        {
                            MemoryLocation? loc = await GetMemoryLocationByIdAsync(id.ToString());
                            if (loc != null)
                            {
                                list.Add(loc);
                            }
                        }
                        return list.Cast<Location>().ToList().AsReadOnly();
                    }
                case ELocationType.Marker:
                    {
                        List<MarkerLocation> list = new List<MarkerLocation>();
                        RedisValue[] ids = await _database.SetMembersAsync(MARKER_LOCATIONS_ALL_KEY);
                        foreach (RedisValue id in ids)
                        {
                            MarkerLocation? loc = await GetMarkerLocationByIdAsync(id.ToString());
                            if (loc != null)
                            {
                                list.Add(loc);
                            }
                        }
                        return list.Cast<Location>().ToList().AsReadOnly();
                    }
                default:
                    return Array.Empty<Location>();
            }
        }

        #endregion

        #region Private Helpers
     

        private async Task<CassetteLocation?> GetCassetteLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ELocationType locationType = Helper.GetHashValueAsEnum<ELocationType>(hashEntries, "location_type");
            ELocationStatus status = Helper.GetHashValueAsEnum<ELocationStatus>(hashEntries, "status");
            string currentItemId = Helper.GetHashValue(hashEntries, "current_item_id");
            string parentId = Helper.GetHashValue(hashEntries, "parent_id");
            string childrenValue = Helper.GetHashValue(hashEntries, "children");
            string isVisibleRaw = Helper.GetHashValue(hashEntries, "is_visible");
            string isRelRaw = Helper.GetHashValue(hashEntries, "is_relative_position");
            bool isVisible = true;
            if (!string.IsNullOrEmpty(isVisibleRaw))
            {
                if (bool.TryParse(isVisibleRaw, out bool parsed))
                {
                    isVisible = parsed;
                }
            }
            bool isRelative = false;
            if (!string.IsNullOrEmpty(isRelRaw))
            {
                if (bool.TryParse(isRelRaw, out bool parsedRel))
                {
                    isRelative = parsedRel;
                }
            }
            int xValue = Helper.GetHashValueAsInt(hashEntries, "x");
            int yValue = Helper.GetHashValueAsInt(hashEntries, "y");
            int zValue = Helper.GetHashValueAsInt(hashEntries, "z");
            int rotateX = Helper.GetHashValueAsInt(hashEntries, "rotate_x");
            int rotateY = Helper.GetHashValueAsInt(hashEntries, "rotate_y");
            int rotateZ = Helper.GetHashValueAsInt(hashEntries, "rotate_z");
            if (rotateX == 0 && rotateY == 0 && rotateZ == 0)
            {
                int legacyYaw = Helper.GetHashValueAsInt(hashEntries, "rotate");
                rotateY = legacyYaw;
            }

            int widthValue = Helper.GetHashValueAsInt(hashEntries, "width");
            int heightValue = Helper.GetHashValueAsInt(hashEntries, "height");
            int depthValue = Helper.GetHashValueAsInt(hashEntries, "depth");

            CassetteLocation loc = new CassetteLocation(id, name) { Status = status };
            loc.CurrentItemId = currentItemId;
            loc.ParentId = parentId;
            loc.IsVisible = isVisible;
            loc.IsRelativePosition = isRelative;
            loc.Position = new Position((uint)xValue, (uint)yValue, (uint)zValue);
            if (widthValue > 0)
            {
                loc.Width = (uint)widthValue;
            }
            if (heightValue > 0)
            {
                loc.Height = (uint)heightValue;
            }
            if (depthValue > 0)
            {
                loc.Depth = (uint)depthValue;
            }
            loc.Rotation = new Rotation(rotateX, rotateY, rotateZ);
            loc.Children = string.IsNullOrWhiteSpace(childrenValue)
                ? new List<string>()
                : childrenValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            return loc;
        }


        private async Task<TrayLocation?> GetTrayLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{TRAY_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ELocationType locationType = Helper.GetHashValueAsEnum<ELocationType>(hashEntries, "location_type");
            ELocationStatus status = Helper.GetHashValueAsEnum<ELocationStatus>(hashEntries, "status");
            string currentItemId = Helper.GetHashValue(hashEntries, "current_item_id");
            string parentId = Helper.GetHashValue(hashEntries, "parent_id");
            string childrenValue = Helper.GetHashValue(hashEntries, "children");
            string isVisibleRaw = Helper.GetHashValue(hashEntries, "is_visible");
            string isRelRaw = Helper.GetHashValue(hashEntries, "is_relative_position");
            bool isVisible = true;
            if (!string.IsNullOrEmpty(isVisibleRaw))
            {
                if (bool.TryParse(isVisibleRaw, out bool parsed))
                {
                    isVisible = parsed;
                }
            }
            bool isRelative = false;
            if (!string.IsNullOrEmpty(isRelRaw))
            {
                if (bool.TryParse(isRelRaw, out bool parsedRel))
                {
                    isRelative = parsedRel;
                }
            }
            int xValue = Helper.GetHashValueAsInt(hashEntries, "x");
            int yValue = Helper.GetHashValueAsInt(hashEntries, "y");
            int zValue = Helper.GetHashValueAsInt(hashEntries, "z");
            int rotateX = Helper.GetHashValueAsInt(hashEntries, "rotate_x");
            int rotateY = Helper.GetHashValueAsInt(hashEntries, "rotate_y");
            int rotateZ = Helper.GetHashValueAsInt(hashEntries, "rotate_z");
            if (rotateX == 0 && rotateY == 0 && rotateZ == 0)
            {
                int legacyYaw = Helper.GetHashValueAsInt(hashEntries, "rotate");
                rotateY = legacyYaw;
            }

            int widthValue = Helper.GetHashValueAsInt(hashEntries, "width");
            int heightValue = Helper.GetHashValueAsInt(hashEntries, "height");
            int depthValue = Helper.GetHashValueAsInt(hashEntries, "depth");

            TrayLocation loc = new TrayLocation(id, name) { Status = status };
            loc.CurrentItemId = currentItemId;
            loc.ParentId = parentId;
            loc.IsVisible = isVisible;
            loc.IsRelativePosition = isRelative;
            loc.Position = new Position((uint)xValue, (uint)yValue, (uint)zValue);
            if (widthValue > 0)
            {
                loc.Width = (uint)widthValue;
            }
            if (heightValue > 0)
            {
                loc.Height = (uint)heightValue;
            }
            if (depthValue > 0)
            {
                loc.Depth = (uint)depthValue;
            }
            loc.Rotation = new Rotation(rotateX, rotateY, rotateZ);
            loc.Children = string.IsNullOrWhiteSpace(childrenValue)
                ? new List<string>()
                : childrenValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            return loc;
        }


        private async Task<MemoryLocation?> GetMemoryLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{MEMORY_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ELocationType locationType = Helper.GetHashValueAsEnum<ELocationType>(hashEntries, "location_type");
            ELocationStatus status = Helper.GetHashValueAsEnum<ELocationStatus>(hashEntries, "status");
            string currentItemId = Helper.GetHashValue(hashEntries, "current_item_id");
            string parentId = Helper.GetHashValue(hashEntries, "parent_id");
            string childrenValue = Helper.GetHashValue(hashEntries, "children");
            string isVisibleRaw = Helper.GetHashValue(hashEntries, "is_visible");
            string isRelRaw = Helper.GetHashValue(hashEntries, "is_relative_position");
            bool isVisible = true;
            if (!string.IsNullOrEmpty(isVisibleRaw))
            {
                if (bool.TryParse(isVisibleRaw, out bool parsed))
                {
                    isVisible = parsed;
                }
            }
            bool isRelative = false;
            if (!string.IsNullOrEmpty(isRelRaw))
            {
                if (bool.TryParse(isRelRaw, out bool parsedRel))
                {
                    isRelative = parsedRel;
                }
            }
            int xValue = Helper.GetHashValueAsInt(hashEntries, "x");
            int yValue = Helper.GetHashValueAsInt(hashEntries, "y");
            int zValue = Helper.GetHashValueAsInt(hashEntries, "z");
            int rotateX = Helper.GetHashValueAsInt(hashEntries, "rotate_x");
            int rotateY = Helper.GetHashValueAsInt(hashEntries, "rotate_y");
            int rotateZ = Helper.GetHashValueAsInt(hashEntries, "rotate_z");
            if (rotateX == 0 && rotateY == 0 && rotateZ == 0)
            {
                int legacyYaw = Helper.GetHashValueAsInt(hashEntries, "rotate");
                rotateY = legacyYaw;
            }

            int widthValue = Helper.GetHashValueAsInt(hashEntries, "width");
            int heightValue = Helper.GetHashValueAsInt(hashEntries, "height");
            int depthValue = Helper.GetHashValueAsInt(hashEntries, "depth");

            MemoryLocation loc = new MemoryLocation(id, name) { Status = status };
            loc.CurrentItemId = currentItemId;
            loc.ParentId = parentId;
            loc.IsVisible = isVisible;
            loc.IsRelativePosition = isRelative;
            loc.Position = new Position((uint)xValue, (uint)yValue, (uint)zValue);
            if (widthValue > 0)
            {
                loc.Width = (uint)widthValue;
            }
            if (heightValue > 0)
            {
                loc.Height = (uint)heightValue;
            }
            if (depthValue > 0)
            {
                loc.Depth = (uint)depthValue;
            }
            loc.Rotation = new Rotation(rotateX, rotateY, rotateZ);
            loc.Children = string.IsNullOrWhiteSpace(childrenValue)
                ? new List<string>()
                : childrenValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            return loc;
        }


        private async Task<MarkerLocation?> GetMarkerLocationByIdAsync(string id)
        {
            HashEntry[] hashEntries = await _database.HashGetAllAsync($"{MARKER_LOCATION_KEY_PREFIX}{id}");
            if (hashEntries.Length == 0)
            {
                return null;
            }

            string name = Helper.GetHashValue(hashEntries, "name");
            ELocationType locationType = Helper.GetHashValueAsEnum<ELocationType>(hashEntries, "location_type");
            ELocationStatus status = Helper.GetHashValueAsEnum<ELocationStatus>(hashEntries, "status");
            string parentId = Helper.GetHashValue(hashEntries, "parent_id");
            string childrenValue = Helper.GetHashValue(hashEntries, "children");
            string isVisibleRaw = Helper.GetHashValue(hashEntries, "is_visible");
            string isRelRaw = Helper.GetHashValue(hashEntries, "is_relative_position");
            string markerRoleRaw = Helper.GetHashValue(hashEntries, "marker_role");
            bool isVisible = true;
            if (!string.IsNullOrEmpty(isVisibleRaw))
            {
                if (bool.TryParse(isVisibleRaw, out bool parsed))
                {
                    isVisible = parsed;
                }
            }
            bool isRelative = false;
            if (!string.IsNullOrEmpty(isRelRaw))
            {
                if (bool.TryParse(isRelRaw, out bool parsedRel))
                {
                    isRelative = parsedRel;
                }
            }
            int xValue = Helper.GetHashValueAsInt(hashEntries, "x");
            int yValue = Helper.GetHashValueAsInt(hashEntries, "y");
            int zValue = Helper.GetHashValueAsInt(hashEntries, "z");
            int rotateX = Helper.GetHashValueAsInt(hashEntries, "rotate_x");
            int rotateY = Helper.GetHashValueAsInt(hashEntries, "rotate_y");
            int rotateZ = Helper.GetHashValueAsInt(hashEntries, "rotate_z");
            if (rotateX == 0 && rotateY == 0 && rotateZ == 0)
            {
                int legacyYaw = Helper.GetHashValueAsInt(hashEntries, "rotate");
                rotateY = legacyYaw;
            }

            int widthValue = Helper.GetHashValueAsInt(hashEntries, "width");
            int heightValue = Helper.GetHashValueAsInt(hashEntries, "height");
            int depthValue = Helper.GetHashValueAsInt(hashEntries, "depth");

            MarkerLocation loc = new MarkerLocation(id, name) { Status = status };
            loc.ParentId = parentId;
            loc.IsVisible = isVisible;
            loc.IsRelativePosition = isRelative;
            loc.Position = new Position((uint)xValue, (uint)yValue, (uint)zValue);
            if (widthValue > 0)
            {
                loc.Width = (uint)widthValue;
            }
            if (heightValue > 0)
            {
                loc.Height = (uint)heightValue;
            }
            if (depthValue > 0)
            {
                loc.Depth = (uint)depthValue;
            }
            loc.Rotation = new Rotation(rotateX, rotateY, rotateZ);
            if (!string.IsNullOrEmpty(markerRoleRaw))
            {
                EMarkerRole parsedRole;
                if (Enum.TryParse(markerRoleRaw, out parsedRole))
                {
                    loc.MarkerRole = parsedRole;
                }
            }
            loc.Children = string.IsNullOrWhiteSpace(childrenValue)
                ? new List<string>()
                : childrenValue.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            return loc;
        }


        private async Task SaveCassetteLocationAsync(CassetteLocation loc)
        {
            string currentItemId = loc.CurrentItemId ?? string.Empty;
            string childrenValue = string.Join(",", loc.Children);

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", loc.Id),
                new HashEntry("name", loc.Name),
                new HashEntry("location_type", loc.LocationType.ToString()),
                new HashEntry("status", loc.Status.ToString()),
                new HashEntry("current_item_id", currentItemId),
                new HashEntry("parent_id", loc.ParentId ?? string.Empty),
                new HashEntry("children", childrenValue),
                new HashEntry("is_visible", loc.IsVisible.ToString()),
                new HashEntry("is_relative_position", loc.IsRelativePosition.ToString()),
                new HashEntry("x", loc.Position.X.ToString()),
                new HashEntry("y", loc.Position.Y.ToString()),
                new HashEntry("z", loc.Position.Z.ToString()),
                new HashEntry("width", loc.Width.ToString()),
                new HashEntry("height", loc.Height.ToString()),
                new HashEntry("depth", loc.Depth.ToString()),
                new HashEntry("rotate_x", (loc.Rotation != null ? loc.Rotation.X : 0).ToString()),
                new HashEntry("rotate_y", (loc.Rotation != null ? loc.Rotation.Y : 0).ToString()),
                new HashEntry("rotate_z", (loc.Rotation != null ? loc.Rotation.Z : 0).ToString())
            };

            await _database.HashSetAsync($"{CASSETTE_LOCATION_KEY_PREFIX}{loc.Id}", entries);
            await _database.SetAddAsync(CASSETTE_LOCATIONS_ALL_KEY, loc.Id);
        }

        private async Task SaveTrayLocationAsync(TrayLocation loc)
        {
            string currentItemId = loc.CurrentItemId ?? string.Empty;
            string childrenValue = string.Join(",", loc.Children);

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", loc.Id),
                new HashEntry("name", loc.Name),
                new HashEntry("location_type", loc.LocationType.ToString()),
                new HashEntry("status", loc.Status.ToString()),
                new HashEntry("current_item_id", currentItemId),
                new HashEntry("parent_id", loc.ParentId ?? string.Empty),
                new HashEntry("children", childrenValue),
                new HashEntry("is_visible", loc.IsVisible.ToString()),
                new HashEntry("is_relative_position", loc.IsRelativePosition.ToString()),
                new HashEntry("x", loc.Position.X.ToString()),
                new HashEntry("y", loc.Position.Y.ToString()),
                new HashEntry("z", loc.Position.Z.ToString()),
                new HashEntry("width", loc.Width.ToString()),
                new HashEntry("height", loc.Height.ToString()),
                new HashEntry("depth", loc.Depth.ToString()),
                new HashEntry("rotate_x", (loc.Rotation != null ? loc.Rotation.X : 0).ToString()),
                new HashEntry("rotate_y", (loc.Rotation != null ? loc.Rotation.Y : 0).ToString()),
                new HashEntry("rotate_z", (loc.Rotation != null ? loc.Rotation.Z : 0).ToString())
            };

            await _database.HashSetAsync($"{TRAY_LOCATION_KEY_PREFIX}{loc.Id}", entries);
            await _database.SetAddAsync(TRAY_LOCATIONS_ALL_KEY, loc.Id);
        }

        private async Task SaveMemoryLocationAsync(MemoryLocation loc)
        {
            string currentItemId = loc.CurrentItemId ?? string.Empty;
            string childrenValue = string.Join(",", loc.Children);

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", loc.Id),
                new HashEntry("name", loc.Name),
                new HashEntry("location_type", loc.LocationType.ToString()),
                new HashEntry("status", loc.Status.ToString()),
                new HashEntry("current_item_id", currentItemId),
                new HashEntry("parent_id", loc.ParentId ?? string.Empty),
                new HashEntry("children", childrenValue),
                new HashEntry("is_visible", loc.IsVisible.ToString()),
                new HashEntry("is_relative_position", loc.IsRelativePosition.ToString()),
                new HashEntry("x", loc.Position.X.ToString()),
                new HashEntry("y", loc.Position.Y.ToString()),
                new HashEntry("z", loc.Position.Z.ToString()),
                new HashEntry("width", loc.Width.ToString()),
                new HashEntry("height", loc.Height.ToString()),
                new HashEntry("depth", loc.Depth.ToString()),
                new HashEntry("rotate_x", (loc.Rotation != null ? loc.Rotation.X : 0).ToString()),
                new HashEntry("rotate_y", (loc.Rotation != null ? loc.Rotation.Y : 0).ToString()),
                new HashEntry("rotate_z", (loc.Rotation != null ? loc.Rotation.Z : 0).ToString())
            };

            await _database.HashSetAsync($"{MEMORY_LOCATION_KEY_PREFIX}{loc.Id}", entries);
            await _database.SetAddAsync(MEMORY_LOCATIONS_ALL_KEY, loc.Id);
        }

        private async Task SaveMarkerLocationAsync(MarkerLocation loc)
        {
            string childrenValue = string.Join(",", loc.Children);

            HashEntry[] entries = new HashEntry[]
            {
                new HashEntry("id", loc.Id),
                new HashEntry("name", loc.Name),
                new HashEntry("location_type", loc.LocationType.ToString()),
                new HashEntry("status", loc.Status.ToString()),
                new HashEntry("parent_id", loc.ParentId ?? string.Empty),
                new HashEntry("children", childrenValue),
                new HashEntry("is_visible", loc.IsVisible.ToString()),
                new HashEntry("is_relative_position", loc.IsRelativePosition.ToString()),
                new HashEntry("x", loc.Position.X.ToString()),
                new HashEntry("y", loc.Position.Y.ToString()),
                new HashEntry("z", loc.Position.Z.ToString()),
                new HashEntry("width", loc.Width.ToString()),
                new HashEntry("height", loc.Height.ToString()),
                new HashEntry("depth", loc.Depth.ToString()),
                new HashEntry("rotate_x", (loc.Rotation != null ? loc.Rotation.X : 0).ToString()),
                new HashEntry("rotate_y", (loc.Rotation != null ? loc.Rotation.Y : 0).ToString()),
                new HashEntry("rotate_z", (loc.Rotation != null ? loc.Rotation.Z : 0).ToString()),
                new HashEntry("marker_role", loc.MarkerRole.ToString())
            };

            await _database.HashSetAsync($"{MARKER_LOCATION_KEY_PREFIX}{loc.Id}", entries);
            await _database.SetAddAsync(MARKER_LOCATIONS_ALL_KEY, loc.Id);
        }

        #endregion
    }
}





