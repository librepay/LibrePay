using System;
using System.Globalization;
using LibrePay.Models;

namespace LibrePay.UnitTests
{
    public static class FakeData
    {
        public static readonly ExchangeRate ValidExchangeRate
            = new ExchangeRate(0.50M, "R$/BTC", DateTime.Now, CultureInfo.InvariantCulture);

        public static readonly Payment InvalidPaymentWoAddress = new Payment(null, 0.5M, null);

        public static readonly Payment ValidPayment = new Payment
        {
            Done = false,
            Id = 1,
            ValueFiat = 0.50M,
            ExchangeRate = ValidExchangeRate,
            Address = TestnetBtcAddress,
        };

        public const string TestnetBtcAddress = "2NGGJhXSwPu2THVaxh8txhxhZSxwshssAvJ";

        public static readonly Payment InvalidPaymentWoExchangeRate = new Payment(TestnetBtcAddress, 0.5M, null);

        public const string ValidXPub =
            "tpubDCiHcfHhKqUkdfbUKdTSHRFqqZvMmSJgbnPdvtkmWyDeeJY8FTBTwHfDFYnyxJaqYeHr85RbdmsWs7LqxQ29yr1RXZYnD6N5dGCLHyBip3h";

        public const string ValidXPubAddressDerivedNum2 = "tb1q89j25da5dzlm42em8zn293mc8w00lhx8t5nn23";
    }
}
