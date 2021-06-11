using System;
using System.Collections.Generic;
using SkiaSharp;

namespace TextPaint
{
    public static class TextSplitter
    {
        public static IEnumerable<DrawingText> Split(string text, SKPaint paint, float maxWidth)
        {
            var start = 0;

            while (true)
            {
                var span = text.AsSpan(start);
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

                yield return new DrawingText(text.Substring(start, spaceIndex));

                start += spaceIndex + 1;
            }
        }

        private static int FindSpace(ReadOnlySpan<char> span, int from)
        {
            for (int i = from; i >= 0; i--)
            {
                if (span[i] == ' ')
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public class DrawingText
    {
        public string Text { get; }

        public DrawingText(string text)
        {
            Text = text;
        }
    }
}
