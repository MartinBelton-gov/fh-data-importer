using System.Text.RegularExpressions;

namespace PluginBase;

public static class Helper
{
    public static DateTime? GetDateFromString(string strDate)
    {
        if (string.IsNullOrEmpty(strDate))
            return null;

        if (DateTime.TryParse(strDate, out DateTime date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return null;
    }

    public static long  GetLongFromString(string value, string adminAreaCode = default!)
    {
        if (Guid.TryParse(value, out Guid guid))
        {
            if (!string.IsNullOrEmpty(adminAreaCode))
            {
                string id = $"{adminAreaCode.Replace("E", "")}{Math.Abs(guid.GetHashCode())}";
                if (long.TryParse(id, out long longValue))
                {
                    return longValue;
                }
            }

            return Math.Abs(guid.GetHashCode());
        }
        else
        {
            string result = Regex.Replace(value, "[A-Za-z ]", "");
            if (!string.IsNullOrEmpty(adminAreaCode))
            {
                result = $"{adminAreaCode.Replace("E", "")}{result}";
            }
                
            if (long.TryParse(result, out long longValue)) 
            { 
                return longValue; 
            }
        }

        return 0;
    }
}

