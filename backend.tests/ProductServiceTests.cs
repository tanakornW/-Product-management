using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace backend.tests;

public class ProductServiceTests
{
    private static (ProductService service, ProductDbContext context) CreateService(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new ProductDbContext(options);
        var service = new ProductService(context, NullLogger<ProductService>.Instance);

        return (service, context);
    }

    [Fact]
    public async Task CreateAsync_WithValidCode_ReturnsNormalizedCode()
    {
        var (service, _) = CreateService(nameof(CreateAsync_WithValidCode_ReturnsNormalizedCode));
        const string messyInput = "abcde abcde abcde abcde abcde abcde";

        var result = await service.CreateAsync(messyInput);

        Assert.Equal("ABCDE-ABCDE-ABCDE-ABCDE-ABCDE-ABCDE", result.Code);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        var (service, _) = CreateService(nameof(CreateAsync_DuplicateCode_ThrowsInvalidOperationException));
        var validCode = "ABCDE-ABCDE-ABCDE-ABCDE-ABCDE-ABCDE";

        await service.CreateAsync(validCode);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(validCode));
    }

    [Fact]
    public async Task CreateAsync_InvalidFormat_ThrowsArgumentException()
    {
        var (service, _) = CreateService(nameof(CreateAsync_InvalidFormat_ThrowsArgumentException));

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync("INVALID-CODE"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var (service, context) = CreateService(nameof(DeleteAsync_RemovesProduct));
        var created = await service.CreateAsync("ABCDE-ABCDE-ABCDE-ABCDE-ABCDE-ABCDE");

        await service.DeleteAsync(created.Id);

        Assert.Empty(context.Products);
    }

    [Fact]
    public async Task GetQrCodeAsync_ReturnsPngBytes()
    {
        var (service, _) = CreateService(nameof(GetQrCodeAsync_ReturnsPngBytes));
        var created = await service.CreateAsync("ABCDE-ABCDE-ABCDE-ABCDE-ABCDE-ABCDE");

        var bytes = await service.GetQrCodeAsync(created.Id);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }
}

