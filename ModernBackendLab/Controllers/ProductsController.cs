
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernBackendLab.Data;
using ModernBackendLab.Models;
using ModernBackendLab.DTOs;

namespace ModernBackendLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    


[HttpGet("cursor_v1")]
public async Task<ActionResult<List<ProductResponse>>> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100,
    CancellationToken cancellationToken = default)
{
    if (page < 1 || pageSize < 1 || pageSize > 200)
        return BadRequest("Invalid page or pageSize. page must be >= 1 and pageSize must be between 1 and 200.");

    var products = await db.Products
        .AsNoTracking()
        .OrderBy(p => p.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Price,
            p.RowVersion))
        .ToListAsync(cancellationToken);

    return Ok(products);
}

[HttpGet("cursor_v2")]
public async Task<ActionResult<List<ProductResponse>>> GetProductsUsingKeyset(
    [FromQuery] int? afterId = null,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
{
    if (pageSize < 1 || pageSize > 200 || afterId < 0)
    {
        return BadRequest("Invalid afterId or pageSize.");
    }

    var query = db.Products
        .AsNoTracking()
        .AsQueryable();

    if (afterId.HasValue)
    {
        query = query.Where(p => p.Id > afterId.Value);
    }

    var products = await query
        .OrderBy(p => p.Id)
        .Take(pageSize)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Price,
            p.RowVersion))
        .ToListAsync(cancellationToken);

    return Ok(products);
}
[HttpGet("{id:int}")]
public async Task<ActionResult<ProductResponse>> GetById(
    int id,
    CancellationToken cancellationToken)
{
    var product = await db.Products
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new ProductResponse(
            p.Id, p.Name, p.Price,p.RowVersion))
        .FirstOrDefaultAsync(cancellationToken);

    if (product is null)
        return NotFound();

    return Ok(product);
}
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(
        Product product,
        CancellationToken cancellationToken)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetProducts),
            new { id = product.Id },
            product);
    }
    [HttpPost("transaction-test")]
public async Task<IActionResult> TransactionTest(
    CancellationToken cancellationToken)
{
    await using var transaction =
        await db.Database.BeginTransactionAsync(cancellationToken);

    try
    {
        db.Products.Add(new Product
        {
            Name = "Transaction-Test-1",
            Price = 100
        });

        await db.SaveChangesAsync(cancellationToken);

        db.Products.Add(new Product
        {
            Name = "Transaction-Test-2",
            Price = 200
        });

        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok("Both products saved successfully.");
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(
    int id,
    UpdateProductRequest request,
    CancellationToken cancellationToken)
{
    var product = await db.Products
        .FirstOrDefaultAsync(
            p => p.Id == id,
            cancellationToken);

    if (product is null)
        return NotFound();

    db.Entry(product)
        .Property(p => p.RowVersion)
        .OriginalValue = request.RowVersion;

    product.Name = request.Name;
    product.Price = request.Price;

    try
    {
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Conflict("Product was modified by another request. Fetch the latest version and retry.");
    }
}
}