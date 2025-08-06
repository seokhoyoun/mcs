using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.Core.Models
{

    public class Location<T> : IEntity where T : ITransportable
    {
        public string Id { get; }
        public string Name { get; }
        public ELocationType LocationType { get; }
        public ELocationStatus Status { get; set; }
        public T? CurrentItem { get; private set; }

        public Location(string id, string name, ELocationType locationType)
        {
            Id = id;
            Name = name;
            LocationType = locationType;
        }

        public void Load(T item)
        {
            CurrentItem = item;
        }

        public T? Unload()
        {
            var item = CurrentItem;
            CurrentItem = default;
            return item;
        }
    }
}