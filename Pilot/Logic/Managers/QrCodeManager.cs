using System.Linq;
using Microsoft.Extensions.Configuration;
using Pilot.Logic.Configuration;
using QRCoder;

namespace Pilot.Logic.Managers
{
    public class QrCodeManager
    {
        private readonly IConfiguration configuration;

        public QrCodeManager(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public (byte[] content, string mimeType) GetSiteUrlQrCode()
        {
            string siteUrl = configuration.GetServerUrls().First();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            PayloadGenerator.Bookmark bookmark = new PayloadGenerator.Bookmark(siteUrl, "Adres do pilota");
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(bookmark);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            return (qrCodeBytes, "image/bmp");
        }
    }
}
