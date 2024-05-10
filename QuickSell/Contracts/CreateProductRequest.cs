namespace QuickSell.Contracts;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; } = 0;
    public int Stock { get; set; } = 0;
}