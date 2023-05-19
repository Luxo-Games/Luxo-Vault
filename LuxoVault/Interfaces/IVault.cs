namespace LuxoVault.Interfaces;

public interface IVault <T>
{
    Task SaveData(T data, String filename);

    Task<T?> LoadData(String filename);
}