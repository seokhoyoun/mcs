using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Shared.Application.DTO
{
    public class LocationState
    {
        public string Id { get; private set; }

        public string? CurrentItemId { get; private set; }

        public int Status { get; private set; }

        public LocationState(string id, string? currentItemId, int status)
        {
            Id = id;
            CurrentItemId = currentItemId;
            Status = status;
        }
    }
}
