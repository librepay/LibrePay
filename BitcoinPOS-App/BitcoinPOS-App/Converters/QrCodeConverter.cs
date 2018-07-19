using System;
using System.Globalization;
using System.IO;
using BitcoinPOS_App.Models;
using QRCoder;
using Xamarin.Forms;

namespace BitcoinPOS_App.Converters
{
    public class QrCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(Payment))
                return null;

            var payment = (Payment) value;

            if (string.IsNullOrWhiteSpace(payment.Address) || payment.Value <= 0)
                return null;

            var generator = new PayloadGenerator.BitcoinAddress(
                payment.Address
                , System.Convert.ToDouble(payment.Value)
                , label: "Pagamento Bitcoin POS"
            );
            var payload = generator.ToString();

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeAsPngArray = qrCode.GetGraphic(20);

            // this memory stream should not be disposed here 
            var ms = new MemoryStream(qrCodeAsPngArray);
            qrCodeAsPngArray = null;

            return ImageSource.FromStream(() => ms);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}