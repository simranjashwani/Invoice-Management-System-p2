namespace InvoiceManagementSystem.DAL.Entities
{
    public class PaymentMethod
    {
        public int MethodId { get; set; }

        public string MethodName { get; set; } = string.Empty;
    }
}