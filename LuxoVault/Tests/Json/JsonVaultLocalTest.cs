using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LuxoVault.Json;
using LuxoVault.Json.Exceptions;
using LuxoVault.Tests.Json.Utils;
using NUnit.Framework;

namespace LuxoVault.Tests.Json;

public class JsonVaultLocalTest
{
    public class JsonVaultLocalTests
    {
        private const string Secret = "mySecret";

        [Test]
        public async Task SaveData_ValidData_FileCreated()
        {
            var TestData = RandomData.GenerateRandomData();
            // Arrange
            string filename = "testfile.json";
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);

            // Act
            await vault.SaveData(TestData, filename);

            // Assert
            Assert.True(File.Exists(filename));

            // Clean up
            File.Delete(filename);
        }

        [Test]
        public async Task LoadData_ValidFile_ReturnsData()
        {
            RandomData? testData = RandomData.GenerateRandomData();
            // Arrange
            string filename = "testfile.json";
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);
            await vault.SaveData(testData, filename);

            // Act
            RandomData? loadedData = await vault.LoadData(filename);

            // Assert
            Assert.AreEqual(testData, loadedData);

            // Clean up
            File.Delete(filename);
        }

        [Test]
        public async Task LoadData_InvalidFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string filename = "nonexistent.json";
            JsonVaultLocal<string> vault = new JsonVaultLocal<string>(Path.GetTempPath(), Secret);

            // Act and Assert
            Assert.ThrowsAsync<FileNotFoundException>(() => vault.LoadData(filename));
        }

        [Test]
        public void LoadData_InvalidSignature_ThrowsInvalidSignatureException()
        {
            var TestData = RandomData.GenerateRandomData();
            // Arrange
            string filename = "testfile.json";
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);
            string jsonWithInvalidSignature = AddInvalidSignatureToData(TestData.ToJson());
            File.WriteAllText(filename, jsonWithInvalidSignature);

            // Act and Assert
            Assert.ThrowsAsync<InvalidSignatureException>(() => vault.LoadData(filename));

            // Clean up
            File.Delete(filename);
        }

        private string AddInvalidSignatureToData(string data)
        {
            // Generate an invalid signature
            byte[] jsonBytes = Encoding.UTF8.GetBytes(data);
            byte[] secretBytes = Encoding.UTF8.GetBytes(Secret);

            using (HMACSHA256 hmac = new HMACSHA256(secretBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(jsonBytes);
                // Modify the hash by adding an extra byte
                hashBytes[0]++;
                string signature = Convert.ToBase64String(hashBytes);

                // Add the modified signature to the data
                var jsonDocument = JsonDocument.Parse(data);
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream))
                    {
                        writer.WriteStartObject();
                        foreach (var property in jsonDocument.RootElement.EnumerateObject())
                        {
                            property.WriteTo(writer);
                        }
                        writer.WriteString("signature", signature);
                        writer.WriteEndObject();
                    }
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }
    }
}