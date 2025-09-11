using System;
using System.Threading.Tasks;

namespace Nexus.Portal.Components.Pages.Monitoring
{
    public partial class Realtime : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}

