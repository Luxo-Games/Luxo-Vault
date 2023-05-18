using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using ProtoBuf;
using Vault.Interfaces;

public class ProtoVaultLocal<T> : IVault<T> where T : IMessage<T>
{
    /// <summary>
    /// The absolute Path where the File should be saved
    /// </summary>
    public readonly String Path;

    /// <summary>
    /// File extension without leading dot.
    /// </summary>
    public readonly String FileExtension;
    
    public ProtoVaultLocal(String path, String fileExtention)
    {
        Path = path;
        FileExtension = "."+fileExtention;
    }

    
    /// <summary>
    /// Saves an instance of a class T to a local binary file.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="filename"></param>
    public async Task SaveData(T data, String filename)
    {
        try
        {
            string filePath = GetFilePath(filename);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                data.WriteTo(fileStream);
                await fileStream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while loading data: {ex.Message}");
            throw;
        }
    }

    
    /// <summary>
    /// Load local binary file.
    /// </summary>
    /// <param name="filename">the name of the file you want to load.</param>
    /// <returns>An instance of the specified class.</returns>
    /// <exception cref="FileNotFoundException">No file with the specified name was found</exception>
    /// <exception cref="IOException">There was an error while reading the file</exception>
    public async Task<T> LoadData(String filename) 
    {
        try
        {
            string filePath = GetFilePath(filename);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);

                T data = await Task.Run(() =>
                {
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        return Serializer.Deserialize<T>(memoryStream);
                    }
                });

                return data;
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An error occurred while loading data: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while loading data: {ex.Message}");
            throw;
        }
    }

    private string GetFilePath(string filename)
    {
        return System.IO.Path.Join(Path, filename + FileExtension);
    }
}
