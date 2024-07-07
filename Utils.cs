using System.Text.RegularExpressions;
public static class PublicIdExtractor
{
    public static string Extract(string Url, string folderPath)
    {
        string[] splitUrl = Url.Split('/');
        if (splitUrl.Length > 0)
        {
            string[] splitPublicId = string.Join("/", splitUrl[7..]).Split('.');
            if (splitPublicId.Length > 0) return splitPublicId[0];
        }
        return "";
    }
}
public static class Validator
{
    public static bool IsValidEmail(string email)
    {
        string emailPattern= @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        Regex emailRegex = new(emailPattern);
        return emailRegex.IsMatch(email);
    }
}

public static class CustomRandom 
{
    public static List<int> YieldUniqueNumbers(int numberOfItem, int limit)
    {
        List<int> result = new();
        Random random = new();
        while (result.Count < numberOfItem) {
            int number = random.Next(0, limit);
            if (!result.Contains(number)) 
            {
                result.Add(number);
            }
        } 
        return result;
    }
}