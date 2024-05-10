namespace QuickSell.Domain;

public class SaleItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    public Guid SaleId { get; set; }
    public virtual required Sale Sale { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }
}