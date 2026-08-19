namespace VGS.RetailOS.Modules.ReturnsManagement.Return.BO;
public class ReturnBO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string ReturnNumber { get; set; } = null!;
    public string ReturnType { get; set; } = null!;
    public Guid StoreId { get; set; }
    public decimal TotalAmount { get; set; }
}
