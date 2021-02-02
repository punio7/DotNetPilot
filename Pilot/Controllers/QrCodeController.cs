using Microsoft.AspNetCore.Mvc;
using Pilot.Logic.Managers;

namespace Pilot.Controllers
{
    public class QrCodeController : Controller
    {
        private readonly QrCodeManager qrCodeManager;

        public QrCodeController(QrCodeManager qrCodeManager)
        {
            this.qrCodeManager = qrCodeManager;
        }
        
        [ResponseCache(Duration = int.MaxValue)]
        public IActionResult SiteUrl()
        {
            var (content, mimeType) = qrCodeManager.GetSiteUrlQrCode();
            return File(content, mimeType);
        }
    }
}
