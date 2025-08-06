using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.Core.Models
{

    public class Location<T> : IEntity where T : ITransportable
    {
        public string Id { get; }
        public string Name { get; }
        public ELocationType PortType { get; }
        public T? CurrentItem { get; private set; }

        public Location(string id, string name, ELocationType portType)
        {
            Id = id;
            Name = name;
            PortType = portType;
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