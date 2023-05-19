using System.Text.Json;
namespace Tests.Json.Utils;

public class RandomData
{
    public string RandomString { get; set; }
    public int RandomNumber { get; set; }
    public DateTime RandomDate { get; set; }

    public static RandomData GenerateRandomData()
    {
        return new RandomData();
    }

    public RandomData()
    {
        RandomString = GenerateRandomString(10);
        RandomNumber = GenerateRandomNumber(18, 60);
        RandomDate = GenerateRandomDate();
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] randomString = new char[length];
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            randomString[i] = chars[random.Next(chars.Length)];
        }

        return new string(randomString);
    }

    private static int GenerateRandomNumber(int minValue, int maxValue)
    {
        Random random = new Random();
        return random.Next(minValue, maxValue + 1);
    }

    private static DateTime GenerateRandomDate()
    {
        Random random = new Random();
        int range = (DateTime.Today - DateTime.MinValue).Days;
        return DateTime.MinValue.AddDays(random.Next(range));
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
    
    public override bool Equals(object obj)
    {
        if (GetType() != obj.GetType())
        {
            return false;
        }

        RandomData otherData = (RandomData)obj;
        return RandomString == otherData.RandomString &&
               RandomNumber == otherData.RandomNumber &&
               RandomDate == otherData.RandomDate;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RandomString, RandomNumber, RandomDate);
    }
}