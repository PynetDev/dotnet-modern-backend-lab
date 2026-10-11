
namespace ModernBackendLab.Models;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }
    public byte[] RowVersion { get; set; } = [];
}