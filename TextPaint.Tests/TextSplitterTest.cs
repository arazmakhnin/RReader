using System;
using System.Collections.Generic;
using System.Linq;
using Fb2;
using Fb2.Specification;
using NUnit.Framework;
using Shouldly;
using SkiaSharp;

namespace TextPaint.Tests
{
    public class TextSplitterTest
    {
        private SKPaint _paint;

        [SetUp]
        public void Setup()
        {
            _paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas"))) { TextSize = 20 };
        }

        [Test]
        public void Test1_SimpleWithOneLine()
        {
            // Arrange
            var splitter = CreateSplitter("a");

            // Act
            var result = splitter.GetPage(_paint, 100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[] { "a" });
        }

        [Test]
        public void Test2_BreakingSeveralLines()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("aaa aaa aaa");

            // Act
            var result = splitter.GetPage(_paint, maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                "aaa",
                "aaa"
            });
        }

        [Test]
        public void Test3_BreakingSeveralPages()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("aaa bbb ccc");

            // Act
            var result1 = splitter.GetPage(_paint, maxWidth, maxHeight).ToStringArray();
            splitter.NextPage();
            var result2 = splitter.GetPage(_paint, maxWidth, maxHeight).ToStringArray();

            // Assert
            result1.ShouldBe(new[]
            {
                "aaa",
                "bbb"
            });

            result2.ShouldBe(new[]
            {
                "ccc"
            });
        }

        [Test]
        public void Test4_ShouldIgnoreEmptySpace()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("<p>aaa</p>   \r\n    <p>bbb</p>");

            // Act
            var result = splitter.GetPage(_paint, maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                "bbb"
            });
        }

        [Test]
        public void Test5_ShouldProcessEmptyLineTag()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("<p>aaa</p> <empty-line /> <p>bbb</p>");

            // Act
            var result = splitter.GetPage(_paint, maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.EmptyLine,
                "bbb"
            });
        }

        private TextSplitter CreateSplitter(string text)
        {
            var readingInfo = new ReadingInfo(0, 0);
            var book = Fb2Parser.Load($"<FictionBook><body>{text}</body></FictionBook>");
            return new TextSplitter(book, readingInfo);
        }
    }

    public static class SplitExtension
    {
        internal const string EmptyLine = "empty-line";

        public static string[] ToStringArray(this IEnumerable<DrawingItem> items)
        {
            return items.Select(i => i switch
            {
                DrawingText text => text.Text,
                EmptyLine _ => EmptyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
            }).ToArray();
        }
    }
}