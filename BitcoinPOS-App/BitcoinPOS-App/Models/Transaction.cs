namespace BitcoinPOS_App.Models
{
    public class Transaction
    {
        public string Id { get; set; }

        public decimal Value { get; set; }

        public ulong Confirmations { get; set; }

        public override bool Equals(object obj)
        {
            var tx = obj as Transaction;

            if (tx == null)
                return false;

            return Id == tx.Id;
        }

        public override int GetHashCode()
            => Id?.GetHashCode() ?? 0;
    }
}