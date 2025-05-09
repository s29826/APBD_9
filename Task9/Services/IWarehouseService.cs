namespace Task9.Services;

public interface IWarehouseService
{
    Task<bool> DoesProductExist(int idProduct);
    Task<bool> DoesWarehouseExist(int idWarehouse);
    Task<bool> CanAddToOrder(int idProduct, int amount);
    Task<bool> IsItDone(int idOrder);
}