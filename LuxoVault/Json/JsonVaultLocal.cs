using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LuxoVault.Interfaces;
using LuxoVault.Json.Exceptions;

namespace LuxoVault.Json;

public class JsonVaultLocal <T> : IVault<T>
{
    public readonly String path;
    private readonly String secret;
    
    public JsonVaultLocal(String path, String secret)
    {
        this.path = path;
        this.secret = secret;
    }

    public async Task SaveData(T data, string filename)
    {

        string jsonData = AddSignatureToData(data);
        await File.WriteAllTextAsync(filename, jsonData);
    }

    public async Task<T?> LoadData(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("Vault file not found.");
        }

        string jsonData = await File.ReadAllTextAsync(filename);

        if (!ValidateSignature(jsonData)) throw new InvalidSignatureException();
        jsonData = RemoveSignatureFromData(jsonData);

        T? data = JsonSerializer.Deserialize<T>(jsonData);
        return data;
    }

    #region Signature Logic

    private string GenerateSignature(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

        using (HMACSHA256 hmac = new HMACSHA256(secretBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(jsonBytes);
            string signature = Convert.ToBase64String(hashBytes);
            return signature;
        }
    }

    private string AddSignatureToData(T data)
    {
        string json = JsonSerializer.Serialize(data);
        string signature = GenerateSignature(json);

        JsonDocument jsonDocument = JsonDocument.Parse(json);
        using (MemoryStream stream = new MemoryStream())
        {
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();

                // Copy the existing properties from the original JSON
                foreach (JsonProperty property in jsonDocument.RootElement.EnumerateObject())
                {
                    property.WriteTo(writer);
                }

                // Add the signature as a new property
                writer.WriteString("signature", signature);

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    private bool ValidateSignature(string json)
    {
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(utf8Bytes);
        if (!JsonDocument.TryParseValue(ref reader, out JsonDocument? jsonDocument))
        {
            // Invalid JSON string
            return false;
        }

        if (!jsonDocument.RootElement.TryGetProperty("signature", out JsonElement signatureElement))
        {
            // Signature property not found
            return false;
        }

        string? signature = signatureElement.GetString();
        if (string.IsNullOrEmpty(signature))
        {
            // Invalid or empty signature
            return false;
        }

        // Remove the signature from the JSON string
        string jsonStringWithoutSignature = RemoveSignatureFromData(json);

        // Generate the expected signature
        string expectedSignature = GenerateSignature(jsonStringWithoutSignature);

        // Compare the generated signature with the expected signature
        return string.Equals(signature, expectedSignature);
    }
    
    private string RemoveSignatureFromData(string jsonData)
    {
        // Parse the JSON document to access properties
        using (JsonDocument jsonDocument = JsonDocument.Parse(jsonData))
        {
            // Check if the signature property exists
            if (jsonDocument.RootElement.TryGetProperty("signature", out JsonElement signatureElement))
            {
                // Create a copy of the JSON document without the signature property
                using (MemoryStream stream = new MemoryStream())
                {
                    using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                    {
                        writer.WriteStartObject();

                        // Copy properties from the original JSON, excluding the signature property
                        foreach (JsonProperty property in jsonDocument.RootElement.EnumerateObject())
                        {
                            if (property.Name != "signature")
                            {
                                property.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                    }

                    // Convert the modified JSON object back to a string
                    jsonData = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        return jsonData;
    }

    #endregion
}