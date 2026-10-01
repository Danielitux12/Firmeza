using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _productRepository.GetAllAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _productRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (!product.IsValid())
        {
            throw new InvalidOperationException("Product data is not valid.");
        }

        return await _productRepository.AddAsync(product, cancellationToken);
    }
}
