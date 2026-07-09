using System;
using System.Linq;
using System.Text;
using UnityEngine;

public static class RoomCodeGenerator
{
    private const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly System.Random Random = new System.Random();

    public const int CodeLength = 6;

    public static string Generate()
    {
        var builder = new StringBuilder(CodeLength);

        for (int i = 0; i < CodeLength; i++)
        {
            builder.Append(AllowedCharacters[Random.Next(AllowedCharacters.Length)]);
        }

        return builder.ToString();
    }

    public static string Normalize(string rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
        {
            return string.Empty;
        }

        return new string(rawCode
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray());
    }

    public static bool IsValid(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != CodeLength)
        {
            return false;
        }

        return code.All(char.IsLetterOrDigit);
    }
}
