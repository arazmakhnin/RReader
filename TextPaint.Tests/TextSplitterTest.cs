using System.Linq;
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
            _paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas")));
        }

        [Test]
        public void Test1()
        {
            // Arrange, Act
            var result = TextSplitter.Split("a", _paint, 100).Select(i => i.Text).ToArray();

            // Assert
            result.ShouldBe(new[] { "a" });
        }

        [Test]
        public void Test2()
        {
            // Arrange, Act
            var maxWidth = _paint.MeasureText("aaa");
            var result = TextSplitter.Split("aaa aaa aaa", _paint, maxWidth).Select(i => i.Text).ToArray();

            // Assert
            result.ShouldBe(new[]
            {
                "aaa",
                "aaa",
                "aaa"
            });
        }

    }
}