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

        public const string LastId = "last-used-id";
    }
}