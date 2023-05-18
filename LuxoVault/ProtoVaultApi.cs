using Google.Protobuf;
using Vault.Interfaces;

namespace Vault;

public class ProtoVaultApi<T> : IVault<T> where T : IMessage<T>, new()
{
    private readonly HttpClient httpClient;
    public readonly String GetUrl;
    public readonly String PostUrl;
    private readonly MessageParser<T> messageParser;

    public ProtoVaultApi(HttpClient httpClient, string url) : this(httpClient, url, url) { }

    public ProtoVaultApi(HttpClient httpClient, string getUrl, string postUrl)
    {
        this.httpClient = httpClient;
        messageParser = new MessageParser<T>(() => new T());
        GetUrl = getUrl;
        PostUrl = postUrl;
    }

    public async Task SaveData(T data, string filename)
    {
        try
        {
            byte[] serializedData = data.ToByteArray();
            ByteArrayContent content = new ByteArrayContent(serializedData);
            HttpResponseMessage response = await httpClient.PostAsync($"api/save/{filename}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to save data. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while saving data: {ex.Message}");
            throw;
        }
    }

    public async Task<T> LoadData(string filename)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/load/{filename}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to load data. Status code: {response.StatusCode}");
            }

            byte[] responseData = await response.Content.ReadAsByteArrayAsync();
            T data = messageParser.ParseFrom(responseData);
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading data: {ex.Message}");
            throw;
        }
    }
}