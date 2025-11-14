using System.Text.RegularExpressions;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace backend.Services;

public class ProductService(ProductDbContext dbContext, ILogger<ProductService> logger) : IProductService
{
    private static readonly Regex ProductCodePattern =
        new(@"^[A-Z0-9]{5}(-[A-Z0-9]{5}){5}$", RegexOptions.Compiled);

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = dbContext.Products.OrderByDescending(p => p.CreatedAt);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = NormalizeCode(searchTerm, strictFormat: false);
            query = query.Where(p => p.Code.Contains(normalized));
        }

        var products = await query.ToListAsync(cancellationToken);
        return products.Select(ToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("รหัสสินค้าต้องไม่ว่างเปล่า", nameof(code));
        }

        var normalized = NormalizeCode(code);

        if (!ProductCodePattern.IsMatch(normalized))
        {
            throw new ArgumentException("รูปแบบรหัสสินค้าไม่ถูกต้อง (ต้องเป็น xxxxx-xxxxx-xxxxx-xxxxx-xxxxx-xxxxx)", nameof(code));
        }

        var exists = await dbContext.Products.AnyAsync(p => p.Code == normalized, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("รหัสสินค้านี้ถูกใช้งานแล้ว");
        }

        var product = new Product
        {
            Code = normalized,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created product id {ProductId}", product.Id);
        return ToDto(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException("ไม่พบรหัสสินค้าสำหรับลบ");
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]> GetQrCodeAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException("ไม่พบรหัสสินค้าสำหรับสร้าง QR");
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(product.Code, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(8);
    }

    private static ProductDto ToDto(Product product) => new(product.Id, product.Code, product.CreatedAt);

    private static string NormalizeCode(string code, bool strictFormat = true)
    {
        var trimmed = code.Trim().ToUpperInvariant();

        if (!strictFormat)
        {
            return trimmed.Replace(" ", string.Empty);
        }

        var segments = trimmed.Replace(" ", string.Empty).Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 6 && segments.All(s => s.Length == 5))
        {
            return string.Join('-', segments);
        }

        // If the user entered a continuous string without dashes, format it.
        var alphanumeric = new string(trimmed.Where(char.IsLetterOrDigit).ToArray());
        if (alphanumeric.Length != 30)
        {
            return trimmed;
        }

        var formatted = string.Join('-', Enumerable.Range(0, 6).Select(i => alphanumeric.Substring(i * 5, 5)));
        return formatted;
    }
}

