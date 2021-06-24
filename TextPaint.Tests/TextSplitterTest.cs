using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fb2;
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
        public void Test0_WithEmptyText()
        {
            // Arrange
            var splitter = CreateSplitter(string.Empty);

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBeEmpty();
        }

        [Test]
        public void Test1_SimpleWithOneLine()
        {
            // Arrange
            var splitter = CreateSplitter("a");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "a"
            });
        }

        [Test]
        public void Test2_BreakingSeveralLines()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("aaa aaa aaa");

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "aaa",
                SplitExtension.LineBreak,
                "aaa"
            });
        }

        [Test]
        public void Test3_ShouldBreakPage()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("aaa bbb ccc");

            // Act
            var result = splitter.GetPage(maxWidth, maxHeight).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test3_5_ShouldGetNextPageAfterBreak()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("aaa bbb ccc");

            // Act
            var result1 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage1 = splitter.NextPage();
            var result2 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage2 = splitter.NextPage();

            // Assert
            result1.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb",
                SplitExtension.LineBreak
            });

            result2.ShouldBe(new[]
            {
                "ccc"
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeFalse();
        }

        [Test]
        public void Test4_ShouldIgnoreEmptySpace()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("<p>aaa</p>   \r\n    <p>bbb</p>");

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test5_ShouldProcessEmptyLineTag()
        {
            // Arrange
            var splitter = CreateSplitter("<p>aaa</p> <empty-line /> <p>bbb</p>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                SplitExtension.EmptyLine,
                "bbb",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test5_4_ShouldProcessParagraphTag()
        {
            // Arrange
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("<p>aaa</p><p>bbb</p>");

            // Act
            var result = splitter.GetPage(100, maxHeight).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test5_5_ShouldCountEmptyLineWhenCalculatingPageHeight()
        {
            // Arrange
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("<p>aaa</p> <empty-line /> <p>bbb</p>");

            // Act
            var result = splitter.GetPage(100, maxHeight).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                SplitExtension.EmptyLine
            });
        }

        [Test]
        public void Test5_ShouldProcessStrongTag()
        {
            // Arrange
            var splitter = CreateSplitter("<strong>aaa</strong>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray(true);

            // Assert
            result.ShouldBe(new[]
            {
                "aaa-bold"
            });
        }

        [Test]
        public void Test6_ShouldProcessStrongTagExtended()
        {
            // Arrange
            var splitter = CreateSplitter("<p>asd <strong>aaa</strong> zxc</p>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray(true);

            // Assert
            result.ShouldBe(new[]
            {
                "asd ",
                "aaa-bold",
                " zxc"
            });
        }

        [Test]
        public void Test7_ShouldProcessEmphasisTag()
        {
            // Arrange
            var splitter = CreateSplitter("<emphasis>aaa</emphasis>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray(true);

            // Assert
            result.ShouldBe(new[]
            {
                "aaa-italic"
            });
        }

        [Test]
        public void Test8_ShouldProcessEmphasisTagExtended()
        {
            // Arrange
            var splitter = CreateSplitter("<p>asd <emphasis>aaa</emphasis> zxc</p>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray(true);

            // Assert
            result.ShouldBe(new[]
            {
                "asd ",
                "aaa-italic",
                " zxc"
            });
        }

        [Test]
        public void Test9_ShouldProcessBothStrongAndEmphasisTags()
        {
            // Arrange
            var splitter = CreateSplitter("<p>1 <strong>2 <emphasis>3</emphasis> 4</strong> 5</p>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "1 ",
                "2 -bold",
                "3-bold-italic",
                " 4-bold",
                " 5",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test10_ShouldCountWidthForDifferentTags()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var splitter = CreateSplitter("<strong>aaa</strong><emphasis>bbb</emphasis>");

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa-bold",
                SplitExtension.LineBreak,
                "bbb-italic"
            });
        }

        [Test]
        public void Test11_ShouldBreakLineInsideTags()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa bbb ccc");
            var splitter = CreateSplitter("<strong>aaa bbb</strong><emphasis> ccc ddd</emphasis>");

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa bbb-bold",
                " ccc-italic",
                SplitExtension.LineBreak,
                "ddd-italic"
            });
        }

        private TextSplitter CreateSplitter(string text)
        {
            var readingInfo = new ReadingInfo(0, 0);
            var book = Fb2Parser.Load($"<FictionBook><body>{text}</body></FictionBook>");
            var splitter = new TextSplitter(book, readingInfo);
            splitter.ChangeParameters(new TextParameters(_paint));
            return splitter;
        }
    }

    public static class SplitExtension
    {
        internal const string LineBreak = "line-break";
        internal const string EmptyLine = "empty-line";

        public static string[] ToStringArray(this IEnumerable<DrawingItem> items, bool ignoreLineBreaks = false)
        {
            var f = new SKFont();

            var query = items.Select(i => i switch
            {
                DrawingText text => Stringify(text),
                LineBreak _ => LineBreak,
                EmptyLine _ => EmptyLine,
                _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
            });

            if (ignoreLineBreaks)
            {
                query = query
                    .Where(i => i != LineBreak);
            }

            return query.ToArray();
        }

        private static string Stringify(DrawingText text)
        {
            var builder = new StringBuilder(text.Text);

            if (text.Paint.Typeface.IsBold)
            {
                builder.Append("-bold");
            }

            if (text.Paint.Typeface.IsItalic)
            {
                builder.Append("-italic");
            }

            return builder.ToString();
        }
    }
}