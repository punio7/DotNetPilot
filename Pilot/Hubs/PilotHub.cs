using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Pilot.Hubs
{
    public class PilotHub : Hub
    {
        public static class ClientMethods
        {
            public static readonly string SongUpdate = "songUpdate";
            public static readonly string SongUpdatePosition = "songUpdatePosition";
        }

        public PilotHub() : base()
        {
        }

        public async Task SongUpdateAlert()
        {
            await Clients.All.SendAsync(ClientMethods.SongUpdate);
        }
    }
}
