namespace LuxoVault.Interfaces;

public interface ISigned
{
    protected bool ValidateSignature(string json);
}