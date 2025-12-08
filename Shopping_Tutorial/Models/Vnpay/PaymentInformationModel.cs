namespace Shopping_Tutorial.Models.Vnpay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContactUser { get; set; }

    }
}
