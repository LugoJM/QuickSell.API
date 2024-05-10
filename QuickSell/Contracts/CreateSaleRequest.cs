namespace QuickSell.Contracts;

public class CreateSaleRequest
{
    public ICollection<SaleItemRequest> SaleItems { get; set; } = [];
}

public class SaleItemRequest
{
    public int Quantity { get; set; }
    public Guid ProductId { get; set; }
}