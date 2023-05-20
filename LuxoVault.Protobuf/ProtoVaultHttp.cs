using LuxoVault.Interfaces;
using LuxoVault.Protobuf.Exceptions;
using ProtoBuf;

namespace LuxoVault.Protobuf;

/// <summary>
/// A Vault that sends Protobuf data to your backend application using http/https
/// </summary>
/// <typeparam name="T">DTO Class implementing the IExtensible interface of protobuf-net</typeparam>
public class ProtoVaultHttp<T> : IVault<T> where T : IExtensible, new()
{
    private readonly HttpClient httpClient;
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The URL that ProtoVault will GET from
    /// </summary>
    public readonly string GetUrl;
    
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The URL that ProtoVault will POST to
    /// </summary>
    public readonly string PostUrl;
    
    /// <summary>
    /// File extension without leading dot
    /// </summary>
    public readonly String FileExtension;

    /// <param name="url">The URL that ProtoVault will POST to and GET from</param>
    /// <param name="fileExtension">File extension without leading dot</param>
    public ProtoVaultHttp(string url, string fileExtension) : this(url, url, fileExtension)
    {
    }

    /// <param name="getUrl">The URL that ProtoVault will GET from</param>
    /// <param name="postUrl">The URL that ProtoVault will POST to</param>
    /// <param name="fileExtension">File extension without leading dot</param>
    public ProtoVaultHttp(string getUrl, string postUrl, string fileExtension)
    {
        httpClient = new HttpClient();
        GetUrl = getUrl;
        PostUrl = postUrl;
        FileExtension = $".{fileExtension}";
    }

    /// <summary>
    /// POST data to your backend application
    /// </summary>
    /// <param name="data">DTO that will be serialized and POSTed</param>
    /// <param name="filename">The serialized DTO will have this file name.</param>
    /// <exception cref="HttpResponseException">The request was not successful. You can get the status code from the exception</exception>
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
                throw new HttpResponseException(response.StatusCode);
            }
        }
    }

    /// <summary>
    /// GET data from your backend application
    /// </summary>
    /// <param name="filename">Name of the file you want to GET</param>
    /// <returns>DTO containing your data</returns>
    /// <exception cref="HttpResponseException">The request was not successful. You can get the status code from the exception</exception>
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