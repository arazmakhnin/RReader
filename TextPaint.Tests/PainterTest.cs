using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;
using SkiaSharp;

namespace TextPaint.Tests
{
    [TestFixture]
    public class PainterTest
    {
        private Mock<ICurrentPage> _page;
        private Mock<ICanvas> _canvas;
        private List<DrawingItem> _pageContent;
        private List<PaintInfo> _paintInfo;
        private SKPaint _paint;

        [SetUp]
        public void Setup()
        {
            _page = new Mock<ICurrentPage>();
            _page.Setup(m => m.GetPage(It.IsAny<float>(), It.IsAny<float>()))
                .Returns<float, float>((w, h) => _pageContent);

            _paintInfo = new List<PaintInfo>();
            _canvas = new Mock<ICanvas>();
            _canvas.Setup(m => m.DrawText(It.IsAny<string>(), It.IsAny<SKPoint>(), It.IsAny<SKPaint>()))
                .Callback<string, SKPoint, SKPaint>((text, point, paint) => _paintInfo.Add(new PaintInfo(text, point, paint)));

            _paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas"))) { TextSize = 20 };
        }

        [TearDown]
        public void TearDown()
        {
            _paint?.Dispose();
        }

        [Test]
        public void WithEmptyPage_ShouldDrawNothing()
        {
            // Arrange
            _pageContent = new List<DrawingItem>();
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(_page.Object, _canvas.Object, info);

            // Assert
            _paintInfo.ShouldBeEmpty();
        }

        [Test]
        public void WithOnlyOneWord_ShouldDrawItInTheTopLeftCorner()
        {
            // Arrange
            _pageContent = new List<DrawingItem>
            {
                new DrawingText("aaa", _paint)
            };
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(_page.Object, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(1);
            Check(_paintInfo[0], "aaa", new SKPoint(0, _paint.TextSize), _paint);
        }

        [Test]
        public void WithTwoItemsInOneLine_ShouldDrawOneLine()
        {
            // Arrange
            _pageContent = new List<DrawingItem>
            {
                new DrawingText("aaa", _paint),
                new DrawingText("bbb", _paint)
            };
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(_page.Object, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(_paintInfo[0], "aaa", new SKPoint(0, _paint.TextSize), _paint);
            Check(_paintInfo[1], "bbb", new SKPoint(_paint.MeasureText(_paintInfo[0].Text), _paint.TextSize), _paint);
        }

        [Test]
        public void WithTwoParagraphs_ShouldDrawTwoLines()
        {
            // Arrange
            _pageContent = new List<DrawingItem>
            {
                new DrawingText("aaa", _paint),
                new LineBreak(_paint),
                new DrawingText("bbb", _paint)
            };
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(_page.Object, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(_paintInfo[0], "aaa", new SKPoint(0, _paint.TextSize), _paint);
            Check(_paintInfo[1], "bbb", new SKPoint(0, _paint.TextSize * 2), _paint);
        }

        [Test]
        public void WithDifferentFontSizes_ShouldDrawTwoLines()
        {
            // Arrange
            using var bigFont = new SKPaint(new SKFont(SKTypeface.FromFamilyName("Consolas"))) { TextSize = 30 };
            _pageContent = new List<DrawingItem>
            {
                new DrawingText("aaa", _paint),
                new LineBreak(_paint),
                new DrawingText("bbb", bigFont)
            };
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(_page.Object, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(_paintInfo[0], "aaa", new SKPoint(0, _paint.TextSize), _paint);
            Check(_paintInfo[1], "bbb", new SKPoint(0, _paint.TextSize + bigFont.TextSize), bigFont);
        }

        private void Check(PaintInfo paintInfo, string text, SKPoint point, SKPaint paint)
        {
            paintInfo.ShouldSatisfyAllConditions(
                () => paintInfo.Text.ShouldBe(text),
                () => paintInfo.Point.ShouldBe(point),
                () => paintInfo.Paint.ShouldBe(paint));
        }
    }

    internal class PaintInfo
    {
        public string Text { get; }
        public SKPoint Point { get; }
        public SKPaint Paint { get; }

        public PaintInfo(string text, SKPoint point, SKPaint paint)
        {
            Text = text;
            Point = point;
            Paint = paint;
        }
    }
}
