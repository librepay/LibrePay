namespace BitcoinPOS_App.Models
{
    public class Payment
    {
        public bool Done { get; set; }

        public decimal Value { get; set; }

        public string Address { get; set; }

        public Payment()
        {
        }

        public Payment(decimal value)
        {
            Value = value;
        }
    }
}