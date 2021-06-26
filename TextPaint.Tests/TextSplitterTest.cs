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
        private TextParameters _textParameters;

        [SetUp]
        public void Setup()
        {
            _paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas"))) { TextSize = 20 };
        }

        [TearDown]
        public void TearDown()
        {
            _paint?.Dispose();
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
            var splitter = CreateSplitter("aaa bbb ccc");

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb",
                SplitExtension.LineBreak,
                "ccc"
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
            var maxHeight = _paint.TextSize;
            var splitter = CreateSplitter("aaa bbb");

            // Act
            var result1 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage1 = splitter.NextPage();
            var result2 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage2 = splitter.NextPage();

            // Assert
            result1.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak
            });

            result2.ShouldBe(new[]
            {
                "bbb"
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeFalse();
        }

        [Test]
        public void Test3_6_ShouldGetNextPageForMultipleParagraphs()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("<p>aaa</p><p>bbb</p><p>ccc</p>");

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
                "ccc",
                SplitExtension.LineBreak
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeFalse();
        }

        [Test]
        public void Test3_7_ShouldGetNextPageForMultipleTags()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 3;
            var splitter = CreateSplitter("<p>aaa <emphasis>bbb</emphasis> ccc ddd eee</p>");

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
                "bbb-italic",
                SplitExtension.LineBreak,
                "ccc",
                SplitExtension.LineBreak
            });

            result2.ShouldBe(new[]
            {
                "ddd",
                SplitExtension.LineBreak,
                "eee",
                SplitExtension.LineBreak
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeFalse();
        }

        [Test]
        public void Test3_8_ShouldGetNextPageForMultipleTags()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa bbb ccc");
            var maxHeight = _paint.TextSize;
            var splitter = CreateSplitter("<p>aaa <emphasis>bbb</emphasis> ccc ddd eee</p>");

            // Act
            var result1 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage1 = splitter.NextPage();
            var result2 = splitter.GetPage(maxWidth, maxHeight).ToStringArray();
            var canNextPage2 = splitter.NextPage();

            // Assert
            result1.ShouldBe(new[]
            {
                "aaa ",
                "bbb-italic",
                " ccc",
                SplitExtension.LineBreak
            });

            result2.ShouldBe(new[]
            {
                "ddd eee",
                SplitExtension.LineBreak
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeFalse();
        }

        [Test]
        public void Test3_9_ShouldGetNextPageForMultipleTags()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var maxHeight = _paint.TextSize * 2;
            var splitter = CreateSplitter("<p>aaa bbb ccc</p><p>zzz xxx</p>");

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
                "ccc",
                SplitExtension.LineBreak,
                "zzz",
                SplitExtension.LineBreak
            });

            canNextPage1.ShouldBeTrue();
            canNextPage2.ShouldBeTrue();
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
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa-bold"
            });
        }

        [Test]
        [TestCase(0)]
        [TestCase(20)]
        public void Test6_ShouldProcessStrongTagExtended(int indent)
        {
            // Arrange
            var splitter = CreateSplitter("<p>aaa <strong>bbb</strong> ccc</p>");
            _textParameters.ParagraphFirstLineIndent = indent;
            splitter.ChangeParameters(_textParameters);

            // Act
            var result = splitter.GetPage(150, 100).SkipWhile(i => i is EmptySpace).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa ",
                "bbb-bold",
                " ccc",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test6_5_ShouldProcessMultipleTagsWithLineBreakOnSpace()
        {
            // Arrange
            var splitter = CreateSplitter("<p>asd <strong>aaa</strong> zxc</p>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "asd ",
                "aaa-bold",
                SplitExtension.LineBreak,
                "zxc",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test7_ShouldProcessEmphasisTag()
        {
            // Arrange
            var splitter = CreateSplitter("<emphasis>aaa</emphasis>");

            // Act
            var result = splitter.GetPage(100, 100).ToStringArray();

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
            var result = splitter.GetPage(150, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "asd ",
                "aaa-italic",
                " zxc",
                SplitExtension.LineBreak
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

        [Test]
        public void Test12_FirstLineOfParagraphShouldBeIndented()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa bbb");
            var splitter = CreateSplitter("<p>aaa bbb ccc</p>");
            var textParameters = new TextParameters
            {
                RegularTextPaint = _paint
            };
            splitter.ChangeParameters(textParameters);

            // Act
            var result = splitter.GetPage(maxWidth, 100).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                SplitExtension.EmptySpace + textParameters.ParagraphFirstLineIndent,
                "aaa ",
                SplitExtension.LineBreak,
                "bbb ccc",
                SplitExtension.LineBreak
            });
        }

        [Test]
        public void Test13_ShouldProcessTitleTag()
        {
            // Arrange
            var splitter = CreateSplitter("<title>aaa</title>");

            // Act
            var result = splitter.GetPage(100, 100);
            var strResult = result.ToStringArray();

            // Assert
            strResult.ShouldBe(new[]
            {
                "aaa"
            });

            var title = result.First() as DrawingText;
            title.ShouldNotBeNull();
            title.ShouldSatisfyAllConditions(
                () => title.Paint.TextSize.ShouldBe(24),
                () => title.Paint.TextAlign.ShouldBe(SKTextAlign.Center));
        }

        private TextSplitter CreateSplitter(string text)
        {
            var readingInfo = new ReadingInfo(0, 0);
            var book = Fb2Parser.Load($"<FictionBook><body>{text}</body></FictionBook>");
            var splitter = new TextSplitter(book, readingInfo);
            _textParameters = new TextParameters
            {
                RegularTextPaint = _paint,
                ParagraphFirstLineIndent = 0
            };
            splitter.ChangeParameters(_textParameters);
            return splitter;
        }
    }

    public static class SplitExtension
    {
        internal const string LineBreak = "line-break";
        internal const string EmptyLine = "empty-line";
        internal const string EmptySpace= "empty-space:";

        public static string[] ToStringArray(this IEnumerable<DrawingItem> items)
        {
            var f = new SKFont();

            var query = items.Select(i => i switch
            {
                DrawingText text => Stringify(text),
                LineBreak _ => LineBreak,
                EmptyLine _ => EmptyLine,
                EmptySpace emptySpace => EmptySpace + emptySpace.Size,
                _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
            });

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