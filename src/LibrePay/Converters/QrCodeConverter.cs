using System;
using System.Globalization;
using System.IO;
using LibrePay.Models;
using QRCoder;
using Xamarin.Forms;

namespace LibrePay.Converters
{
    public class QrCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));
            if (targetType != typeof(ImageSource))
                throw new ArgumentException("Invalid target type", nameof(targetType));
            if (!value.GetType().IsAssignableFrom(typeof(Payment)))
                throw new ArgumentException("Invalid value type", nameof(value));

            var payment = (Payment) value;

            if (string.IsNullOrWhiteSpace(payment.Address) || payment.ValueBitcoin <= 0)
                return null;

            var generator = new PayloadGenerator.BitcoinAddress(
                payment.Address
                , System.Convert.ToDouble(payment.ValueBitcoin)
                , label: $"Pagamento {App.Name}"
            );
            var payload = generator.ToString();

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeAsPngArray = qrCode.GetGraphic(20);

            // this memory stream should not be disposed here 
            var ms = new MemoryStream(qrCodeAsPngArray);

            return ImageSource.FromStream(() => ms);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
