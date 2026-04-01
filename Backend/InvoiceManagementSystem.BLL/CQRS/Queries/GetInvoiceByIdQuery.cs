namespace InvoiceManagementSystem.BLL.CQRS.Queries
{
    public class GetInvoiceByIdQuery
    {
        public int Id { get; set; }

        public GetInvoiceByIdQuery(int id)
        {
            Id = id;
        }
    }
}