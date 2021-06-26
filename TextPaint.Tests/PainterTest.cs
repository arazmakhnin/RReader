using System;
using System.Collections.Generic;
using Fb2;
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
        private TextParameters _textParameters;

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

            _textParameters = new TextParameters("Consolas");
            _paint = _textParameters.RegularTextPaint;
        }

        [TearDown]
        public void TearDown()
        {
            _textParameters?.Dispose();
            _paint?.Dispose();
        }

        [Test]
        public void WithEmptyPage_ShouldDrawNothing()
        {
            // Arrange
            var splitter = CreateSplitter("");
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.ShouldBeEmpty();
        }

        [Test]
        [TestCase(0)]
        [TestCase(20)]
        public void WithOnlyOneWord_ShouldDrawItInTheTopLeftCorner(int indent)
        {
            // Arrange
            var splitter = CreateSplitter("<p>aaa</p>");
            _textParameters.ParagraphFirstLineIndent = indent;
            splitter.ChangeParameters(_textParameters);
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(1);
            Check(0, "aaa", new SKPoint(indent, _paint.TextSize), _paint);
        }
        
        [Test]
        public void WithTwoItemsInOneLine_ShouldDrawOneLine()
        {
            // Arrange
            var splitter = CreateSplitter("<strong>aaa</strong><emphasis>bbb</emphasis>");
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(0, "aaa", new SKPoint(0, _paint.TextSize), p => p.Typeface.IsBold.ShouldBeTrue());
            Check(1, "bbb", new SKPoint(_paint.MeasureText(_paintInfo[0].Text), _paint.TextSize), p => p.Typeface.IsItalic.ShouldBeTrue());
        }

        [Test]
        public void WithTwoParagraphs_ShouldDrawTwoLines()
        {
            // Arrange
            var splitter = CreateSplitter("<p>aaa</p><p>bbb</p>");
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(0, "aaa", new SKPoint(_textParameters.ParagraphFirstLineIndent, _paint.TextSize), _paint);
            Check(1, "bbb", new SKPoint(_textParameters.ParagraphFirstLineIndent, _paint.TextSize * 2), _paint);
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
            Check(0, "aaa", new SKPoint(0, _paint.TextSize), _paint);
            Check(1, "bbb", new SKPoint(0, _paint.TextSize + bigFont.TextSize), bigFont);
        }

        [Test]
        [TestCase("<title>aaa</title>")]
        [TestCase("<title><p>aaa</p></title>")]
        public void WithTitleTag_ShouldDrawTitle(string text)
        {
            // Arrange
            var splitter = CreateSplitter(text);
            var info = new SKImageInfo(100, 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(1);
            Check(0, "aaa", new SKPoint(info.Width / 2f, _textParameters.TitlePaint.TextSize), _textParameters.TitlePaint);
        }

        [Test]
        public void WithLongTitleTag_ShouldBreakIt()
        {
            // Arrange
            var maxWidth = _textParameters.TitlePaint.MeasureText("aaa");
            var splitter = CreateSplitter("<title><p>aaa bbb</p></title>");
            var info = new SKImageInfo((int)(maxWidth + 1), 100);

            // Act
            Painter.Paint(splitter, _canvas.Object, info);

            // Assert
            _paintInfo.Count.ShouldBe(2);
            Check(0, "aaa", new SKPoint(info.Width / 2f, _textParameters.TitlePaint.TextSize), _textParameters.TitlePaint);
            Check(1, "bbb", new SKPoint(info.Width / 2f, _textParameters.TitlePaint.TextSize * 2), _textParameters.TitlePaint);
        }

        private float Measure(int index)
        {
            return _paintInfo[index].Paint.MeasureText(_paintInfo[0].Text);
        }

        private void Check(int index, string text, SKPoint point, SKPaint paint)
        {
            Check(index, text, point, p => p.ShouldBe(paint));
        }

        private void Check(int index, string text, SKPoint point, Action<SKPaint> checkPaint)
        {
            _paintInfo[index].ShouldSatisfyAllConditions(
                () => _paintInfo[index].Text.ShouldBe(text),
                () => _paintInfo[index].Point.ShouldBe(point),
                () => checkPaint(_paintInfo[index].Paint));
        }

        private TextSplitter CreateSplitter(string text)
        {
            var readingInfo = new ReadingInfo(0, 0);
            var book = Fb2Parser.Load($"<FictionBook><body>{text}</body></FictionBook>");
            var splitter = new TextSplitter(book, readingInfo);
            splitter.ChangeParameters(_textParameters);
            return splitter;
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
