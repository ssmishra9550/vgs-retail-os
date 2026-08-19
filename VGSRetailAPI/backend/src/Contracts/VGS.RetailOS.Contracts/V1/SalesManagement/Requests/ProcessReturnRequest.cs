using System;
using System.Collections.Generic;

namespace VGS.RetailOS.Contracts.V1.SalesManagement.Requests;

public class ProcessReturnRequest
{
    public string Reason { get; set; } = string.Empty;
    public List<ReturnItemRequest> Items { get; set; } = new();
}

public class ReturnItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}
