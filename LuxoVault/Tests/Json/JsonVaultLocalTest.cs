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
        string filename = "testfile";

        [Test]
        public async Task SaveData_ValidData_FileCreated()
        {
            var TestData = RandomData.GenerateRandomData();
            // Arrange
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);

            // Act
            await vault.SaveData(TestData, filename);

            var filepath = vault.Path + filename + ".json";

            // Assert
            Assert.True(File.Exists(filename));

            foreach (string st in File.ReadLines(filepath)) Console.WriteLine(st);

            // Clean up
            File.Delete(filepath);
        }

        [Test]
        public async Task LoadData_ValidFile_ReturnsData()
        {
            RandomData? testData = RandomData.GenerateRandomData();
            // Arrange
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);
            await vault.SaveData(testData, filename);

            // Act
            RandomData? loadedData = await vault.LoadData(filename);

            // Assert
            Assert.AreEqual(testData, loadedData);

            var filepath = vault.Path + filename + ".json";
            
            // Clean up
            File.Delete(filepath);
        }

        [Test]
        public async Task LoadData_InvalidFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakeFileName = "nonexistent";
            JsonVaultLocal<string> vault = new JsonVaultLocal<string>(Path.GetTempPath(), Secret);

            // Act and Assert
            Assert.ThrowsAsync<FileNotFoundException>(() => vault.LoadData(fakeFileName));
        }

        [Test]
        public void LoadData_InvalidSignature_ThrowsInvalidSignatureException()
        {
            var TestData = RandomData.GenerateRandomData();
            // Arrange
            JsonVaultLocal<RandomData> vault = new JsonVaultLocal<RandomData>(Path.GetTempPath(), Secret);
            string jsonWithInvalidSignature = AddInvalidSignatureToData(TestData.ToJson());
            var filepath = vault.Path + filename + ".json";
            File.WriteAllText(filepath, jsonWithInvalidSignature);

            // Act and Assert
            Assert.ThrowsAsync<InvalidSignatureException>(() => vault.LoadData(filename));

            // Clean up
            File.Delete(filepath);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(Path.GetTempPath()+filename+".json");
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