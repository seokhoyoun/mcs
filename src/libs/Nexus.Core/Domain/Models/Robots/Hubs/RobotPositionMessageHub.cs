using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Core.Domain.Models.Robots.Hubs
{
    public class RobotPositionMessageHub : Hub
    {
        public async Task SendMessage(IEnumerable<Robot> robots)
        {
            await Clients.All.SendAsync("ReceiveRobotPosition", robots);
        }
    }
}
