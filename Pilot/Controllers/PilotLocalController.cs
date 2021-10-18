using Microsoft.AspNetCore.Mvc;
using Pilot.Logic.Managers;
using Pilot.Logic.WinKeys;

namespace Pilot.Controllers
{
    public class PilotLocalController : Controller
    {
        private readonly KeyEventsManager keyEventsManager;

        public PilotLocalController(KeyEventsManager keyEventsManager)
        {
            this.keyEventsManager = keyEventsManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LaunchMedia()
        {
            keyEventsManager.SendKeyEvent(KeyCode.LAUNCH_MEDIA_SELECT);
            return Json(true);
        }

        public IActionResult Play()
        {
            keyEventsManager.SendKeyEvent(KeyCode.MEDIA_PLAY_PAUSE);
            return Json(true);
        }

        public IActionResult Previous()
        {
            keyEventsManager.SendKeyEvent(KeyCode.MEDIA_PREV_TRACK);
            return Json(true);
        }

        public IActionResult Next()
        {
            keyEventsManager.SendKeyEvent(KeyCode.MEDIA_NEXT_TRACK);
            return Json(true);
        }

        public IActionResult Stop()
        {
            keyEventsManager.SendKeyEvent(KeyCode.MEDIA_STOP);
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