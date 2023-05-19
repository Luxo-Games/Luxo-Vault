using LuxoVault.Interfaces;
using LuxoVault.Protobuf.Exceptions;
using ProtoBuf;

namespace LuxoVault.Protobuf;

public class ProtoVaultApi<T> : IVault<T> where T : IExtensible, new()
{
    private readonly HttpClient httpClient;
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The URL that ProtoVault will GET from
    /// </summary>
    public readonly string GetUrl;
    
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The Url that ProtoVault will POST to
    /// </summary>
    public readonly string PostUrl;
    
    /// <summary>
    /// File extension without leading dot.
    /// </summary>
    public readonly String FileExtension;

    public ProtoVaultApi(string url, string fileExtension) : this(url, url, fileExtension)
    {
    }

    public ProtoVaultApi(string getUrl, string postUrl, string fileExtension)
    {
        httpClient = new HttpClient();
        GetUrl = getUrl;
        PostUrl = postUrl;
        FileExtension = $".{fileExtension}";
    }

    public async Task SaveData(T data, string filename)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            Serializer.Serialize(stream, data);
            byte[] serializedData = stream.ToArray();

            ByteArrayContent content = new ByteArrayContent(serializedData);
            HttpResponseMessage response = await httpClient.PostAsync(PostUrl+filename+FileExtension, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to save data. Status code: {response.StatusCode}");
            }
        }
    }

    public async Task<T?> LoadData(string filename)
    {
        HttpResponseMessage response = await httpClient.GetAsync(GetUrl+filename+FileExtension);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpResponseException(response.StatusCode);
        }

        byte[] responseData = await response.Content.ReadAsByteArrayAsync();
        using MemoryStream stream = new MemoryStream(responseData);
        T? data = Serializer.Deserialize<T>(stream);
        return data;
    }
}