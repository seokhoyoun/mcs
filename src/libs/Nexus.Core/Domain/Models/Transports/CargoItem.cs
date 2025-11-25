using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.ValueObjects;
using System;

namespace Nexus.Core.Domain.Models.Transports
{
    /// <summary>
    /// 적재 대상이 되는 단일 물건을 나타냅니다. 캐리어가 아닌 단순 화물입니다.
    /// </summary>
    public sealed class CargoItem : ICargo
    {
        public CargoItem(string id, string name, ECargoKind kind, CargoDimension size, decimal weight)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            if (size == null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            Id = id;
            Name = name;
            Kind = kind;
            Size = size;
            Weight = weight;
        }

        public string Id { get; }

        public string Name { get; }

        public ECargoKind Kind { get; }

        public CargoDimension Size { get; }

        public decimal Weight { get; }
    }
}
