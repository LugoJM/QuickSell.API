﻿namespace QuickSell.Domain;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }

    public virtual ICollection<SaleItem> SaleItems { get; set; }
}