using System;

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
            return $"{dateTime:D}";
        }
    }
}
