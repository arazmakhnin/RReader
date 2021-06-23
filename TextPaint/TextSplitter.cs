using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public class TextSplitter : ICurrentPage
    {
        private const int EmptyLineSize = 10;

        private SKPaint _regularTextPaint = new SKPaint(new SKFont
        {
            Typeface = SKTypeface.Default,
            Size = 20
        });

        private readonly BaseItem[] _book;
        private ReadingInfo _currentPage;
        private ReadingInfo _nextPage;

#if DEBUG
        public LoadInfo LoadInfo { get; private set; }
#endif

        public TextSplitter(FictionBook book, ReadingInfo startFrom)
        {
            _currentPage = startFrom;

            var body = book.Items.OfType<Body>().First();
            _book = Flatten(body).ToArray();
        }

        public void ChangeParameters(TextParameters textParameters)
        {
            _regularTextPaint = textParameters.RegularTextPaint ?? _regularTextPaint;
        }

        public IReadOnlyCollection<DrawingItem> GetPage(float maxWidth, float maxHeight)
        {
            var w = Stopwatch.StartNew();
            var ignoredTags = new List<string>();

            var height = 0f;

            var result = new List<DrawingItem>();

            var isStrong = false;

            var currentItemIndex = _currentPage.ItemIndex;
            var currentLineIndex = 0;
            var lookingForStart = true;
            var stopProcessing = false;
            while (currentItemIndex < _book.Length)
            {
                var item = _book[currentItemIndex];

                switch (item)
                {
                    case Text text:
                    {
                        currentLineIndex = 0;
                        foreach (var line in ProcessText(text, isStrong, maxWidth))
                        {
                            if (height + line.Paint.TextSize > maxHeight)
                            {
                                stopProcessing = true;
                                break;
                            }

                            if (lookingForStart)
                            {
                                if (currentLineIndex == _currentPage.LineIndex)
                                {
                                    lookingForStart = false;
                                }
                            }

                            if (!lookingForStart)
                            {
                                height += line.Paint.TextSize;
                                result.Add(line);
                                result.Add(new LineBreak());
                            }

                            currentLineIndex++;
                        }

                        break;
                    }

                    case Strong _:
                        isStrong = !isStrong;
                        break;

                    case Fb2.Specification.EmptyLine _:
                        result.Add(new EmptyLine(EmptyLineSize));
                        break;

                    default:
                        if (!ignoredTags.Contains(item.GetType().Name))
                        {
                            ignoredTags.Add(item.GetType().Name);
                        }
                        break;
                }

                if (stopProcessing)
                {
                    break;
                }

                currentItemIndex++;
            }

            _nextPage = new ReadingInfo(currentItemIndex, currentLineIndex);

            w.Stop();

            LoadInfo = new LoadInfo(ignoredTags, w.Elapsed);

            return result;
        }

        public bool NextPage()
        {
            if (_nextPage.ItemIndex >= _book.Length)
            {
                return false;
            }

            _currentPage = _nextPage;
            return true;
        }

        private static IEnumerable<BaseItem> Flatten(BaseItem item)
        {
            var result = new[] { item }.Concat(item.Items.SelectMany(Flatten));
            if (item is Strong)
            {
                result = result.Concat(new[] { item });
            }

            return result;
        }
        
        private IEnumerable<DrawingText> ProcessText(Text text, bool isStrong, float maxWidth)
        {
            var start = 0;

            var paint = _regularTextPaint;
            if (isStrong)
            {
                paint = new SKPaint(new SKFont(SKTypeface.FromFamilyName(_regularTextPaint.Typeface.FamilyName, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), _regularTextPaint.TextSize));
            }

            while (true)
            {
                var span = text.Value.AsSpan(start);
                var charCount = (int)_regularTextPaint.BreakText(span, maxWidth);
                if (span.Length == charCount)
                {
                    yield return new DrawingText(span.ToString(), paint);
                    break;
                }

                var spaceIndex = FindSpace(span, charCount);
                if (spaceIndex == -1)
                {
                    yield return new DrawingText(span.ToString(), paint);
                    break;
                }

                yield return new DrawingText(text.Value.Substring(start, spaceIndex), paint);

                start += spaceIndex + 1;
            }
        }

        private static int FindSpace(ReadOnlySpan<char> span, int from)
        {
            for (var i = from; i >= 0; i--)
            {
                if (span[i] == ' ')
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public class TextParameters
    {
        public SKPaint RegularTextPaint { get; }

        public TextParameters(SKPaint regularTextPaint)
        {
            RegularTextPaint = regularTextPaint;
        }
    }

    public interface ICurrentPage
    {
        IReadOnlyCollection<DrawingItem> GetPage(float maxWidth, float maxHeight);
    }

    public class DrawingItem
    {
    }

    public class DrawingText : DrawingItem
    {
        public string Text { get; }
        public SKPaint Paint { get; }

        public DrawingText(string text, SKPaint paint)
        {
            Text = text;
            Paint = paint;
        }
    }

    public class LineBreak : DrawingItem
    {
    }

    public class EmptyLine : DrawingItem
    {
        public int Size { get; }

        public EmptyLine(int size)
        {
            Size = size;
        }
    }
}
