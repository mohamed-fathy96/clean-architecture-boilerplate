using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PhoneNumbers;
using Polly;
using Polly.Retry;
using TimeZoneConverter;

namespace CleanArchitecture.Domain.Shared;

public static class Helpers
{
    public static byte[] HashPassword(this string password, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
        return deriveBytes.GetBytes(64);
    }

    public static bool VerifyPassword(this string password, byte[] salt, byte[] storedHash)
    {
        byte[] computedHash = HashPassword(password, salt);
        return StructuralComparisons.StructuralEqualityComparer.Equals(computedHash, storedHash);
    }

    public static int ToInt(this string input)
    {
        if (input is null)
            return 0;

        return int.TryParse(input, out var result) ? result : 0;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        // If T is string, use IsNullOrEmpty
        if (typeof(T) == typeof(char))
        {
            return string.IsNullOrEmpty(collection as string);
        }

        // Otherwise, check if collection has any elements
        return collection == null || !collection.Any();
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
    {
        // If T is string, use IsNullOrEmpty
        if (typeof(T) == typeof(char))
        {
            return !string.IsNullOrEmpty(collection as string);
        }

        // Otherwise, check if collection has any elements
        return collection != null && collection.Any();
    }

    public static string GenerateUniqueKey(int length, string seed = "", bool numericOnly = false)
    {
        var characters = !numericOnly ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" : "0123456789";

        if (string.IsNullOrWhiteSpace(seed))
        {
            // Use a new instance of RNG to create the seed to make sure it's not affected by the previous seed value
            using var rng = RandomNumberGenerator.Create();
            var seedBytes = new byte[16];
            rng.GetBytes(seedBytes);
            seed = Convert.ToBase64String(seedBytes);
        }

        using var bufferRng = RandomNumberGenerator.Create();
        var buffer = new byte[144];
        bufferRng.GetBytes(buffer);

        var combinedBytes = Encoding.UTF8.GetBytes(seed).Concat(buffer).ToArray();

        // Convert the combined bytes to a string of alphanumeric characters only
        var code = new StringBuilder(length);
        foreach (var b in combinedBytes)
        {
            var index = b % characters.Length;
            code.Append(characters[index]);
            if (code.Length == length)
            {
                break;
            }
        }

        return code.ToString();
    }

    public static AsyncRetryPolicy CreateRetryPolicy<T>(int retryAttempts = 3, int waitTime = 3) where T : Exception
    {
        return Policy
            .Handle<T>()
            .OrInner<T>()
            .WaitAndRetryAsync(retryAttempts, _ => TimeSpan.FromSeconds(waitTime));
    }

    public static string ToDescription(this Enum val)
    {
        var attributes = (DescriptionAttribute[])val
            .GetType().GetField(val.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes?.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static string ToJson<T>(this T value, bool camelCase = true) where T : class
    {
        if (value == null)
            return "";

        if (camelCase)
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        return JsonConvert.SerializeObject(value);
    }

    public static string ToCamelCase(this string str)
    {
        var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
        var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
            m => m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
        var tailWords = words.Skip(1)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            .ToArray();
        return $"{leadWord}{string.Join(string.Empty, tailWords)}";
    }

    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var phoneNumberWithCode = $"+{phoneNumber}";
            var parsedPhoneNumber = phoneNumberUtil.Parse(phoneNumberWithCode, null);
            return phoneNumberUtil.IsValidNumber(parsedPhoneNumber);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumberForCountryCode(this string phoneNumber, string countryCode)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var phoneNumberWithCode = $"+{countryCode}{phoneNumber}";
            var parsedPhoneNumber = phoneNumberUtil.Parse(phoneNumberWithCode, null);
            return phoneNumberUtil.IsValidNumber(parsedPhoneNumber);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public static T StringToEnum<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
    
    public static DateTime ConvertDateToLocalTime(this DateTimeOffset date)
    {
        const string timezoneId = "Arab Standard Time";

        var arabStandardTimeZone = TZConvert.GetTimeZoneInfo(timezoneId);

        var offset = TimeZoneInfo.ConvertTime(date, arabStandardTimeZone);

        return offset.DateTime;
    }

    /// <summary>
    /// Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that exists on the enum value</returns>
    /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
    public static T GetEnumAttribute<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0 ? (T)attributes[0] : null;
    }
    
    public static bool BeAValidPhoneNumber(this string phoneNumber)
    {
        return phoneNumber.IsValidPhoneNumber();
    }

    public static DateTime EndDate(this DateTime date)
    {
        return date.Date.Add(new TimeSpan(23, 59, 59));
    }

    public static string RemoveCountryCode(this string phoneNumber)
    {
        var phoneUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var numberProto = phoneUtil.Parse(phoneNumber, null);

            var nationalNumber = numberProto.NationalNumber.ToString();

            return nationalNumber;
        }
        catch (NumberParseException)
        {
            return phoneNumber;
        }
    }

    public static string NormalizePhoneNumber(this string phoneNumber)
    {
        if (phoneNumber.StartsWith("0"))
            phoneNumber = phoneNumber[1..];

        return phoneNumber.RemoveCountryCode();
    }

    public static string TransformPhoneToInternationalFormat(string phone, string countryCode)
    {
        return !countryCode.StartsWith('+') ? $"+{countryCode}{phone}" : $"{countryCode}{phone}";
    }
}
