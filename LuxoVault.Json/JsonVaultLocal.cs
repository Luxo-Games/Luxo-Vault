using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LuxoVault.Interfaces;
using LuxoVault.Json.Exceptions;

namespace LuxoVault.Json;

/// <summary>
/// A Vault saving your files locally as signed JSON
/// </summary>
/// <typeparam name="T">DTO that can be serialized to JSON</typeparam>
public class JsonVaultLocal <T> : IVault<T>
{
    /// <summary>
    /// Absolute path your JSON files will be stored at
    /// </summary>
    public readonly string Path;
    private readonly string secret;
    
    /// <param name="path">The absolute path your JSON files will be stored at</param>
    /// <param name="secret">The secret that will be used to create and validate signatures. KEEP THIS SOMEWHERE SAFE!</param>
    public JsonVaultLocal(string path, string secret)
    {
        Path = path;
        this.secret = secret;
    }

    /// <summary>
    /// Serialize a DTO to JSON, signs it, and stores the file locally
    /// </summary>
    /// <param name="data">The DTO that will be serialized to JSON</param>
    /// <param name="filename">Name of the JSON file without the file extension</param>
    public async Task SaveData(T data, string filename)
    {
        filename += ".json";
        string jsonData = AddSignatureToData(data);
        await File.WriteAllTextAsync(Path+filename, jsonData);
    }

    /// <summary>
    /// Loads a signed JSON file from local disk and validates it's signature
    /// </summary>
    /// <param name="filename">Name of the JSON file without the file extension</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">No JSON file with the specified name was found</exception>
    /// <exception cref="InvalidSignatureException">Thrown when your data has been tampered with</exception>
    public async Task<T?> LoadData(string filename)
    {
        filename += ".json";
        if (!File.Exists(Path+filename))
        {
            throw new FileNotFoundException($"{Path}{filename}.json file not found.");
        }

        string jsonData = await File.ReadAllTextAsync(Path+filename);

        if (!ValidateSignature(jsonData)) throw new InvalidSignatureException();
        jsonData = RemoveSignatureFromData(jsonData);

        T? data = JsonSerializer.Deserialize<T>(jsonData);
        return data;
    }

    #region Signature Logic

    private string GenerateSignature(string data)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(data);
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

        using HMACSHA256 hmac = new HMACSHA256(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(jsonBytes);
        string signature = Convert.ToBase64String(hashBytes);
        return signature;
    }

    private string AddSignatureToData(T data)
    {
        string json = JsonSerializer.Serialize(data);
        string signature = GenerateSignature(json);

        JsonDocument jsonDocument = JsonDocument.Parse(json);
        using MemoryStream stream = new MemoryStream();
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
            if (!jsonDocument.RootElement.TryGetProperty("signature", out JsonElement signatureElement))
                return jsonData;
            // Create a copy of the JSON document without the signature property
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();

                    // Copy properties from the original JSON, excluding the signature property
                    foreach (JsonProperty property in jsonDocument.RootElement.EnumerateObject())
                    {
                        if (property.Name == "signature") continue;
                        property.WriteTo(writer);
                    }

                    writer.WriteEndObject();
                }

                // Convert the modified JSON object back to a string
                jsonData = Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        return jsonData;
    }

    #endregion
}