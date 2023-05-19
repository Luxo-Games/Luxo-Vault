using LuxoVault.Interfaces;
using LuxoVault.Protobuf.Exceptions;
using ProtoBuf;

namespace LuxoVault.Protobuf;

public class ProtoVaultLocal<T> : IVault<T> where T : IExtensible
{
    /// <summary>
    /// The absolute Path where the File should be saved
    /// </summary>
    public readonly String Path;

    /// <summary>
    /// File extension without leading dot.
    /// </summary>
    public readonly String FileExtension;
    
    public ProtoVaultLocal(String path, String fileExtension)
    {
        Path = path;
        FileExtension = "."+fileExtension;
    }

    
    /// <summary>
    /// Saves an instance of a class T to a local binary file.
    /// </summary>
    /// <param name="data">The instance of the DTO</param>
    /// <param name="filename">The name the DTO should be saved as</param>
    public async Task SaveData(T data, String filename)
    { 
        string filePath = GetFilePath(filename);
    
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            Serializer.Serialize(fileStream, data);
            await fileStream.FlushAsync();
        }
    }

    
    /// <summary>
    /// Load local binary file.
    /// </summary>
    /// <param name="filename">the name of the file you want to load.</param>
    /// <returns>An instance of the specified class.</returns>
    /// <exception cref="FileNotFoundException">No file with the specified name was found</exception>
    /// <exception cref="IOException">There was an error while reading the file</exception>
    /// <exception cref="ReadFileSizeMismatchException">The amount of bytes read was less than requested</exception>
    public async Task<T?> LoadData(String filename) 
    {
        string filePath = GetFilePath(filename);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            byte[] buffer = new byte[fileStream.Length];
            int byteAmount = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            if (byteAmount < buffer.Length) throw new ReadFileSizeMismatchException();

            T? data = await Task.Run(() =>
            {
                using MemoryStream memoryStream = new MemoryStream(buffer);
                return Serializer.Deserialize<T>(memoryStream);
            });

                return data;
        }
    }

    private string GetFilePath(string filename)
    {
        return System.IO.Path.Join(Path, filename + FileExtension);
    }
}