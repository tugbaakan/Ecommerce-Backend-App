using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.Models;
using EcommerceApi.Models.Dto;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRedisCacheService _cacheService;
    private const string ProductsCacheKey = "products_list";

    public ProductsController(ApplicationDbContext context, IRedisCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        // Try to get from cache first
        var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(ProductsCacheKey);
        if (cachedProducts != null)
        {
            return cachedProducts;
        }

        // If not in cache, get from database
        var products = await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Description = p.Description,
                Quantity = p.Quantity,
                Price = p.Price
            })
            .ToListAsync();

        // Cache the results
        await _cacheService.SetAsync(ProductsCacheKey, products);

        return products;
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var cacheKey = $"product_{id}";
        
        // Try to get from cache first
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
        if (cachedProduct != null)
        {
            return cachedProduct;
        }

        // If not in cache, get from database
        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Description = p.Description,
                Quantity = p.Quantity,
                Price = p.Price
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        // Cache the result
        await _cacheService.SetAsync(cacheKey, product);

        return product;
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Description = createProductDto.Description,
            Quantity = createProductDto.Quantity,
            Price = createProductDto.Price
        };

        // Generate timestamp-based barcode
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        product.Barcode = timestamp.ToString("X"); // Convert to hexadecimal for shorter code

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Description = product.Description,
            Quantity = product.Quantity,
            Price = product.Price
        };

        // Invalidate the products list cache
        await _cacheService.RemoveAsync(ProductsCacheKey);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, CreateProductDto updateProductDto)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        existingProduct.Description = updateProductDto.Description;
        existingProduct.Quantity = updateProductDto.Quantity;
        existingProduct.Price = updateProductDto.Price;

        try
        {
            await _context.SaveChangesAsync();
            
            // Invalidate both the product cache and the products list cache
            await _cacheService.RemoveAsync($"product_{id}");
            await _cacheService.RemoveAsync(ProductsCacheKey);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Invalidate both the product cache and the products list cache
        await _cacheService.RemoveAsync($"product_{id}");
        await _cacheService.RemoveAsync(ProductsCacheKey);

        return NoContent();
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
} 