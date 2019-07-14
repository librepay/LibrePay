namespace LibrePay.Models
{
    public class Transaction
    {
        public string Id { get; }

        public decimal Value { get; set; }

        public ulong Confirmations { get; set; }

        public Transaction(string id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var tx = obj as Transaction;

            if (tx == null)
                return false;

            return Id == tx.Id;
        }

        public override int GetHashCode()
            => Id?.GetHashCode() ?? 0;

        public static bool operator ==(Transaction x, Transaction y)
            => x?.Id == y?.Id;

        public static bool operator !=(Transaction x, Transaction y)
            => !(x == y);
    }
}
