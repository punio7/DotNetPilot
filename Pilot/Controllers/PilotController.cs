using Microsoft.AspNetCore.Mvc;
using Pilot.Logic.Managers;
using Pilot.Logic.WinKeys;

namespace Pilot.Controllers
{
    public class PilotController : Controller
    {
        public KeyEventsManager KeyEventsManager { get; set; }

        public PilotController()
        {
            KeyEventsManager = new KeyEventsManager();
        }

        public IActionResult LaunchMedia()
        {
            KeyEventsManager.SendKeyEvent(KeyCode.LAUNCH_MEDIA_SELECT);
            return Json(true);
        }

        public IActionResult Play()
        {
            KeyEventsManager.SendKeyEvent(KeyCode.MEDIA_PLAY_PAUSE);
            return Json(true);
        }

        public IActionResult Previous()
        {
            KeyEventsManager.SendKeyEvent(KeyCode.MEDIA_PREV_TRACK);
            return Json(true);
        }

        public IActionResult Next()
        {
            KeyEventsManager.SendKeyEvent(KeyCode.MEDIA_NEXT_TRACK);
            return Json(true);
        }

        public IActionResult Stop()
        {
            KeyEventsManager.SendKeyEvent(KeyCode.MEDIA_STOP);
            return Json(true);
        }

        public IActionResult Info()
        {
            return Json(SongManager.Instance.CurrentSong);
        }

        public IActionResult Image()
        {
            return File(SongManager.Instance.CurrentSong.Image, SongManager.Instance.CurrentSong.ImageMimeType);
        }
    }
}