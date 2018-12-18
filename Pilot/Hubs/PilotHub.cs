using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Pilot.Logic.Managers;
using Pilot.Logic.WinKeys;

namespace Pilot.Hubs
{
    public class PilotHub : Hub
    {
        public KeyEventsManager KeyEventsManager { get; set; }

        public PilotHub()
        {
            KeyEventsManager = new KeyEventsManager();
        }

        public async Task SendMessage(string message)
        {
            await Task.Run(() => {
                KeyEventsManager.SendKeyEvent(KeyCode.MEDIA_PLAY_PAUSE);
            });
        }
    }
}
