using System;
using static System.FormattableString;

namespace WhatTimeIsIt
{
    public static class Program
    {
        static void Main()
        {
            Console.WriteLine(GetDateTimeString(DateTime.Now));
        }

        public static string GetDateTimeString(DateTime dateTime)
        {
            return Invariant($"{dateTime:F}");
        }
    }
}
