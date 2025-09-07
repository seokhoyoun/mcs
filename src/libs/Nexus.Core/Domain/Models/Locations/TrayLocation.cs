using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Transports;
using Nexus.Core.Domain.Models.Transports.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Locations
{
    public class TrayLocation : Location
    {
        public override ITransportable? CurrentItem
        {
            get => _currentItem;
            set => _currentItem = (Tray?)value;
        }

        private Tray? _currentItem;

        public TrayLocation(string id, string name, ELocationType locationType) : base(id, name, locationType)
        {
        }
    }
}
