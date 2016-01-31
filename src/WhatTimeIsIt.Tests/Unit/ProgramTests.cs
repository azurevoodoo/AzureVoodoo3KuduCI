using System;
using System.Globalization;
using Xunit;

namespace WhatTimeIsIt.Tests.Unit
{
    public sealed class ProgramTests
    {
        public sealed class TheGetDateTimeStringMethod
        {
            [Fact]
            public void Should_Return_Correct_Formatted_String()
            {
                // Given
                var now = new DateTime(2017, 12, 15, 0, 0, 0, DateTimeKind.Utc);
                const string expect = "Friday, 15 December 2017 00:00:00";

                // When
                var result = Program.GetDateTimeString(now);

                // Then
                Assert.Equal(result, expect);
            }
        }
    }
}