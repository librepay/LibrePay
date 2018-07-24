namespace BitcoinPOS_App.Models
{
    public class Transaction
    {
        public string Id { get; set; }

        public decimal Value { get; set; }

        public ulong Confirmations { get; set; }
    }
}