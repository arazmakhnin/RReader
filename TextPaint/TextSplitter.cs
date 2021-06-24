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

            var result = new ItemsAggregation(_currentPage.LineIndex, maxHeight);

            var isStrong = false;
            var isEmphasis = false;

            var currentItemIndex = _currentPage.ItemIndex;
            while (currentItemIndex < _book.Length)
            {
                var item = _book[currentItemIndex];

                switch (item)
                {
                    case Text text:
                        foreach (var textPart in ProcessText(text, isStrong, isEmphasis, maxWidth))
                        {
                            result.Add(textPart);
                            if (result.EndOfPage)
                            {
                                break;
                            }
                        }

                        break;

                    case Paragraph _:
                        result.Add(new LineBreak(_regularTextPaint));
                        break;

                    case Strong _:
                        isStrong = !isStrong;
                        break;

                    case Emphasis _:
                        isEmphasis = !isEmphasis;
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

                if (result.EndOfPage)
                {
                    break;
                }

                currentItemIndex++;
            }

            _nextPage = new ReadingInfo(currentItemIndex, result.CurrentLineIndex);

            w.Stop();

            LoadInfo = new LoadInfo(ignoredTags, w.Elapsed);

            return result.Items;
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
            IEnumerable<BaseItem> result = new BaseItem[0];
            if (item is not Paragraph)
            {
                result = result.Concat(new[] { item });
            }

            result = result.Concat(item.Items.SelectMany(Flatten));
            if (item is Strong || item is Emphasis || item is Paragraph)
            {
                result = result.Concat(new[] { item });
            }

            return result;
        }
        
        private IEnumerable<DrawingItem> ProcessText(Text text, bool isStrong, bool isEmphasis, float maxWidth)
        {
            var start = 0;

            var paint = _regularTextPaint;
            if (isStrong || isEmphasis)
            {
                var typeface = SKTypeface.FromFamilyName(
                    _regularTextPaint.Typeface.FamilyName, 
                    isStrong ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, 
                    SKFontStyleWidth.Normal, 
                    isEmphasis ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                paint = new SKPaint(new SKFont(typeface, _regularTextPaint.TextSize));
            }

            var width = 0f;

            while (true)
            {
                var span = text.Value.AsSpan(start);
                var charCount = (int)_regularTextPaint.BreakText(span, maxWidth - width);
                if (span.Length == charCount)
                {
                    yield return new DrawingText(span.ToString(), paint);
                    break;
                }

                var spaceIndex = FindSpace(span, charCount);
                if (spaceIndex == -1)
                {
                    yield return new LineBreak(paint);
                    width = 0;
                    continue;
                }

                var part = text.Value.Substring(start, spaceIndex);
                yield return new DrawingText(part, paint);
                width += paint.MeasureText(part);

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
}
