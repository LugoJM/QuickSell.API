namespace QuickSell.Domain;

public class Sale
{
    public Guid Id { get; set; }
    public double SubTotal { get; set; }
    public double Tax { get; set; }
    public double Total { get; set; }
    public int ProductsInSale { get; set; }
    public virtual ICollection<SaleItem> SaleItems { get; set; } 
}