namespace LuxoVault.Interfaces;

public interface IVault <T>
{
    Task SaveData(T data, string filename);

    Task<T?> LoadData(string filename);
}