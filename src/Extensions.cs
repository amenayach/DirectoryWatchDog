using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectoryWatchDog
{
    public static class Extensions
    {
        public static bool IsEmpty(this string text) => string.IsNullOrWhiteSpace(text);

        public static bool NotEmpty(this string text) => !string.IsNullOrWhiteSpace(text);

        public static bool NotEmpty<T>(this IEnumerable<T> list) => list?.Any() ?? false;

        public static bool IsEmpty<T>(this IEnumerable<T> list) => (!list?.Any()) ?? true;

        public static string SafeTrim(this string text) => text.NotEmpty() ? text.Trim() : string.Empty;

        public static void Print(this string message, ConsoleColor color = ConsoleColor.Green)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }
    }
}
