using NBitcoin;

namespace BitcoinPOS_App
{
    public static class Constants
    {
        public const string SettingsXPubKey = "xpub-key";

        public static readonly Network NetworkInUse =
#if DEBUG
                Network.TestNet
#else
                Network.Main
#endif
            ;

        public const int BitcoinDecimals = 8;

        public const string LastId = "last-used-id";

        public static int MinutesBetweenExchangePriceChecks = 1;
    }
}