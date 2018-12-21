using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Pilot.Hubs
{
    public class PilotHub : Hub
    {
        public PilotHub() : base()
        {
        }

        public async Task SongUpdateAlert()
        {
            await Clients.All.SendAsync("songUpdate");
        }
    }
}
