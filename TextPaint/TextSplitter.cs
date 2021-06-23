using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public class TextSplitter
    {
        private readonly BaseItem[] _book;
        private ReadingInfo _currentPage;
        private ReadingInfo _nextPage;

        public TextSplitter(FictionBook book, ReadingInfo startFrom)
        {
            _currentPage = startFrom;

            var body = book.Items.OfType<Body>().First();
            _book = Flatten(body).ToArray();
        }

        public IReadOnlyCollection<DrawingItem> GetPage(SKPaint paint, float maxWidth, float maxHeight)
        {
            var height = 0f;

            var result = new List<DrawingItem>();

            var currentItemIndex = _currentPage.ItemIndex;
            var currentLineIndex = 0;
            var lookingForStart = true;
            while (currentItemIndex < _book.Length)
            {
                var item = _book[currentItemIndex];

                if (item is Text text)
                {
                    currentLineIndex = 0;
                    foreach (var line in ProcessText(text, paint, maxWidth))
                    {
                        if (height + paint.TextSize > maxHeight)
                        {
                            _nextPage = new ReadingInfo(currentItemIndex, currentLineIndex);
                            return result;
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
                            height += paint.TextSize;
                            result.Add(line);
                        }

                        currentLineIndex++;
                    }
                }

                currentItemIndex++;
            }

            _nextPage = new ReadingInfo(currentItemIndex, currentLineIndex);
            return result;
        }

        public void NextPage()
        {
            _currentPage = _nextPage;
        }

        private static IEnumerable<BaseItem> Flatten(BaseItem item)
        {
            return new[] { item }.Concat(item.Items.SelectMany(Flatten));
        }
        
        private static IEnumerable<DrawingText> ProcessText(Text text, SKPaint paint, float maxWidth)
        {
            var start = 0;

            while (true)
            {
                var span = text.Value.AsSpan(start);
                var charCount = (int)paint.BreakText(span, maxWidth);
                if (span.Length == charCount)
                {
                    yield return new DrawingText(span.ToString());
                    break;
                }

                var spaceIndex = FindSpace(span, charCount);
                if (spaceIndex == -1)
                {
                    yield return new DrawingText(span.ToString());
                    break;
                }

                yield return new DrawingText(text.Value.Substring(start, spaceIndex));

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

    public class DrawingItem
    {
    }

    public class DrawingText : DrawingItem
    {
        public string Text { get; }

        public DrawingText(string text)
        {
            Text = text;
        }
    }

    public class PageBreak : DrawingItem
    {
    }
}
