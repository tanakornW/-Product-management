using backend.DTOs;

namespace backend.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(string code, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<byte[]> GetQrCodeAsync(int id, CancellationToken cancellationToken = default);
}

