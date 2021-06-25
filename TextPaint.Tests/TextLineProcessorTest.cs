using System.Linq;
using Fb2;
using Fb2.Specification;
using NUnit.Framework;
using Shouldly;
using SkiaSharp;

namespace TextPaint.Tests
{
    [TestFixture]
    public class TextLineProcessorTest
    {
        private TextLineProcessor _processor;
        private SKPaint _paint;
        private TextParameters _textParameters;

        [SetUp]
        public void Setup()
        {
            _paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas"))) { TextSize = 20 };
            _textParameters = new TextParameters
            {
                RegularTextPaint = _paint,
                ParagraphFirstLineIndent = 0
            };
            _processor = new TextLineProcessor(_textParameters);

            _processor.StartNewLine(true);
        }

        [TearDown]
        public void TearDown()
        {
            _paint?.Dispose();
        }

        [Test]
        public void WithSimpleLineOfText_ShouldProcessIt()
        {
            // Arrange
            var text = new Text("aaa");

            // Act
            var result = _processor.ProcessText(text, false, false, 100).ToStringArray();

            // Assert
            result.ShouldBe(new []
            {
                "aaa"
            });
        }
        
        [Test]
        public void WithLongText_ShouldBreakItToMultipleLinesWithSpaces()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa ");
            var text = new Text("aaa bbb ccc");

            // Act
            var result = _processor.ProcessText(text, false, false, maxWidth).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa ",
                SplitExtension.LineBreak,
                "bbb ",
                SplitExtension.LineBreak,
                "ccc"
            });
        }

        [Test]
        public void WithLongText_ShouldBreakItToMultipleLines()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var text = new Text("aaa bbb ccc");

            // Act
            var result = _processor.ProcessText(text, false, false, maxWidth).ToStringArray();

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
        public void WithLineStartedWithSpace_ShouldTrimThisSpace()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa bbb");
            var texts = new[]
            {
                "aaa",
                " bbb",
                " ccc"
            };
            
            // Act
            var result = ProcessText(texts, false, false, maxWidth);

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                " bbb",
                SplitExtension.LineBreak,
                "ccc"
            });
        }

        [Test]
        public void WithLongWord_ShouldBreakThisWord()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa");
            var text = new Text("aaabbb");
            
            // Act
            var result = _processor.ProcessText(text, false, false, maxWidth).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                SplitExtension.LineBreak,
                "bbb"
            });
        }
        
        [Test]
        public void WithLongTextInMultipleTags_ShouldBreakIt()
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa bbb ccc");
            var texts = new[]
            {
                "aaa bbb",
                " ccc ddd"
            };

            // Act
            var result = ProcessText(texts, false, false, maxWidth);

            // Assert
            result.ShouldBe(new[]
            {
                "aaa bbb",
                " ccc",
                SplitExtension.LineBreak,
                "ddd"
            });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WithLongWord_ShouldNotBreakThisWordIfThereAreMultipleWordsInTheLine(bool inOneTextItem)
        {
            // Arrange
            var maxWidth = _paint.MeasureText("aaa aaa");
            var text = inOneTextItem
                ? new[] { "zzz aaabbb" }
                : new[]
                {
                    "zzz ",
                    "aaabbb"
                };

            // Act
            var result = ProcessText(text, false, false, maxWidth);

            // Assert
            result.ShouldBe(new[]
            {
                "zzz ",
                SplitExtension.LineBreak,
                "aaabbb"
            });
        }

        [Test]
        public void WithTwoLinesInParagraph_ShouldIndentFirstOne()
        {
            // Arrange
            _textParameters.ParagraphFirstLineIndent = 20;
            _processor.StartNewLine(true);

            var maxWidth = _paint.MeasureText("aaa bbb");
            var text = new Text("aaa bbb ccc");

            // Act
            var result = _processor.ProcessText(text, false, false, maxWidth).ToStringArray();

            // Assert
            result.ShouldBe(new[]
            {
                SplitExtension.EmptySpace + _textParameters.ParagraphFirstLineIndent,
                "aaa ",
                SplitExtension.LineBreak,
                "bbb ccc"
            });
        }

        private string[] ProcessText(string[] texts, bool isStrong, bool isEmphasis, float maxWidth)
        {
            return texts
                .Aggregate(Enumerable.Empty<DrawingItem>(), 
                    (current, text) => current.Concat(_processor.ProcessText(new Text(text), isStrong, isEmphasis, maxWidth)))
                .ToStringArray();
        }
    }
}
