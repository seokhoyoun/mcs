using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Lots.Enums
{
    public enum ELotStepStatus
    {
        Waiting,
        Assigned,
        Processing,
        Completed,
        Error
    }
}
