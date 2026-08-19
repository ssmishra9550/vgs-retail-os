namespace VGS.RetailOS.Contracts.V1.ReturnsManagement.Requests;
public class CreateReturnRequest
{
    public Guid StoreId { get; set; }
    public string ReturnType { get; set; } = "CustomerReturn";
    public decimal TotalAmount { get; set; }
}
