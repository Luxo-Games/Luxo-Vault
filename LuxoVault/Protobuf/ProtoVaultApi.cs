using LuxoVault.Interfaces;
using LuxoVault.Protobuf.Exceptions;
using ProtoBuf;

namespace LuxoVault.Protobuf;

public class ProtoVaultApi<T> : IVault<T> where T : IExtensible, new()
{
    private readonly HttpClient httpClient;
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly string GetUrl;
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly string PostUrl;

    public ProtoVaultApi(string url) : this(url, url)
    {
    }

    public ProtoVaultApi(string getUrl, string postUrl)
    {
        httpClient = new HttpClient();
        GetUrl = getUrl;
        PostUrl = postUrl;
    }

    public async Task SaveData(T data, string filename)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            Serializer.Serialize(stream, data);
            byte[] serializedData = stream.ToArray();

            ByteArrayContent content = new ByteArrayContent(serializedData);
            HttpResponseMessage response = await httpClient.PostAsync(PostUrl+filename, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to save data. Status code: {response.StatusCode}");
            }
        }
    }

    public async Task<T?> LoadData(string filename)
    {
        HttpResponseMessage response = await httpClient.GetAsync(GetUrl+filename);

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