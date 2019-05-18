namespace LibrePay
{
    public static class MessengerKeys
    {
        #region Payment

        public const string PaymentFullyReceived = "payment-fully-received";

        public const string PaymentPartiallyReceived = "payment-partially-received";

        #endregion

        #region Main

        public const string MainFinishPayment = "finish-receiving-payment";

        public const string MainCheckXPubExistence = "check-xpub";

        #endregion

        #region Settings

        public const string SettingsFailedLoadSettings = "failed-load-settings";

        #endregion
    }
}