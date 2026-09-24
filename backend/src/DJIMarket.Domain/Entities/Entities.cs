namespace DJIMarket.Domain.Entities;

public enum SaleStatus
{
    Paid,
    Cancelled,
    Refunded
}

public sealed class Manager
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string Position { get; set; } = null!;
    public bool IsActive { get; set; }
    public string AvatarSeed { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public sealed class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Segment { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public sealed class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public sealed class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public bool IsActive { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public sealed class Sale
{
    public Guid Id { get; set; }
    public Guid ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTimeOffset SoldAt { get; set; }
    public SaleStatus Status { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}

public sealed class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
}
