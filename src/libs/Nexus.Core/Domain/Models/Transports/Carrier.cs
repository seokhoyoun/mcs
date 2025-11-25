using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using Nexus.Core.Domain.Models.Transports.Specifications;
using Nexus.Core.Domain.Models.Transports.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.Domain.Models.Transports
{
    /// <summary>
    /// 다른 Cargo를 담을 수 있는 범용 운반체입니다. 운반체 자체도 Cargo로 취급됩니다.
    /// </summary>
    public sealed class Carrier : ICargo
    {
        private readonly List<ICargo> _contents = new List<ICargo>();

        public Carrier(string id, string name, ECargoKind kind, CargoDimension size, decimal weight, CarrierSpecification specification)
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

            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            Id = id;
            Name = name;
            Kind = kind;
            Size = size;
            Weight = weight;
            Specification = specification;
        }

        public string Id { get; }

        public string Name { get; }

        public ECargoKind Kind { get; }

        public CargoDimension Size { get; }

        public decimal Weight { get; }

        public CarrierSpecification Specification { get; }

        public IReadOnlyList<ICargo> Contents => _contents.AsReadOnly();

        public decimal TotalLoadedWeight
        {
            get
            {
                decimal totalWeight = 0m;
                foreach (ICargo cargo in _contents)
                {
                    totalWeight += cargo.Weight;
                }
                return totalWeight;
            }
        }

        public int LoadedCount => _contents.Count;

        public bool CanLoad(ICargo cargo)
        {
            if (cargo == null)
            {
                throw new ArgumentNullException(nameof(cargo));
            }

            if (Specification.MaxItemCount.HasValue)
            {
                int limit = Specification.MaxItemCount.Value;
                if (_contents.Count >= limit)
                {
                    return false;
                }
            }

            bool allowedKind = Specification.IsKindAllowed(cargo.Kind);
            if (!allowedKind)
            {
                return false;
            }

            if (Specification.MaxSize != null)
            {
                CargoDimension limitSize = Specification.MaxSize;
                bool fits = cargo.Size.FitsIn(limitSize);
                if (!fits)
                {
                    return false;
                }
            }

            if (Specification.MaxWeight.HasValue)
            {
                decimal candidateWeight = TotalLoadedWeight + cargo.Weight;
                decimal limitWeight = Specification.MaxWeight.Value;
                if (candidateWeight > limitWeight)
                {
                    return false;
                }
            }

            return true;
        }

        public void Load(ICargo cargo)
        {
            if (!CanLoad(cargo))
            {
                throw new InvalidOperationException("Carrier cannot load the given cargo due to capacity or rule violation.");
            }

            _contents.Add(cargo);
        }

        public bool Unload(string cargoId)
        {
            if (string.IsNullOrWhiteSpace(cargoId))
            {
                return false;
            }

            ICargo? target = _contents.FirstOrDefault(item => item.Id == cargoId);
            if (target != null)
            {
                return _contents.Remove(target);
            }

            return false;
        }

        public ICargo? Find(string cargoId)
        {
            if (string.IsNullOrWhiteSpace(cargoId))
            {
                return null;
            }

            ICargo? target = _contents.FirstOrDefault(item => item.Id == cargoId);
            if (target != null)
            {
                return target;
            }

            foreach (ICargo cargo in _contents)
            {
                Carrier? nestedCarrier = cargo as Carrier;
                if (nestedCarrier != null)
                {
                    ICargo? nested = nestedCarrier.Find(cargoId);
                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }

            return null;
        }
    }
}
