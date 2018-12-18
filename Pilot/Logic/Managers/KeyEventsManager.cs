using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pilot.Logic.WinKeys;

namespace Pilot.Logic.Managers
{
    public class KeyEventsManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);



        public void SendKeyEvent(KeyCode keyCode)
        {
            keybd_event((byte)keyCode, 0, (int)KeyEventInfo.EXTENDEDKEY, 0);
            keybd_event((byte)keyCode, 0, (int)KeyEventInfo.KEYUP, 0);
        }
    }
}
