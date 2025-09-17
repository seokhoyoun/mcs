using Nexus.Core.Domain.Shared.Bases;

namespace Nexus.Core.Domain.Standards
{
    public class DimensionStandard : IEntity
    {
        public string Id { get; }
        public string Name { get; }
        public string Category { get; }
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth { get; }

        public DimensionStandard(string id, string name, string category, uint width, uint height, uint depth)
        {
            Id = id;
            Name = name;
            Category = category;
            Width = width;
            Height = height;
            Depth = depth;
        }
    }
}

