using Task9.Models.DTOs;

namespace Task9.Services;

public interface IWarehouseService
{
    Task<int> AddProduct(ProductDTO productDto);
    Task<int> AddProductProcedure(ProductDTO productDto);
}