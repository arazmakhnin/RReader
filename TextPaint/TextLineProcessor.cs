using System;
using System.Collections.Generic;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public class TextLineProcessor
    {
        private readonly TextParameters _textParameters;
        private float _width;
        private bool _isFirstItemInLine;
        private bool _isParagraphStart;

        public TextLineProcessor(TextParameters textParameters)
        {
            _textParameters = textParameters;
        }

        public IEnumerable<DrawingItem> ProcessText(Text text, bool isStrong, bool isEmphasis, float maxWidth)
        {
            if (_isParagraphStart && _textParameters.ParagraphFirstLineIndent != 0)
            {
                yield return new EmptySpace(_textParameters.ParagraphFirstLineIndent);
                _width = _textParameters.ParagraphFirstLineIndent;
            }

            var start = 0;

            var paint = _textParameters.RegularTextPaint;
            if (isStrong || isEmphasis)
            {
                var typeface = SKTypeface.FromFamilyName(
                    _textParameters.RegularTextPaint.Typeface.FamilyName,
                    isStrong ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    isEmphasis ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                paint = new SKPaint(new SKFont(typeface, _textParameters.RegularTextPaint.TextSize));
            }

            while (start < text.Value.Length)
            {
                if (_isFirstItemInLine && text.Value[start] == ' ')
                {
                    start++;
                    continue;
                }

                var span = text.Value.AsSpan(start);
                var charCount = (int)paint.BreakText(span, maxWidth - _width);
                if (charCount == 0)
                {
                    yield return new LineBreak(paint);
                    StartNewLine(false);
                    continue;
                }

                if (span.Length == charCount)
                {
                    var str = span.ToString();
                    yield return new DrawingText(str, paint);
                    _isFirstItemInLine = false;
                    _width += paint.MeasureText(str);
                    break;
                }

                var charsToSpace = span[charCount] == ' ' ? charCount : FindSpace(span, charCount - 1) + 1;
                if (charsToSpace is 0 or 1)
                {
                    // No space found. If this is a first word on the line,
                    // then looks like it's too long and should be broken
                    if (_isFirstItemInLine)
                    {
                        charsToSpace = charCount;
                    }
                    else
                    {
                        yield return new LineBreak(paint);
                        StartNewLine(false);
                        continue;
                    }
                }

                var part = text.Value.Substring(start, charsToSpace);
                yield return new DrawingText(part, paint);
                yield return new LineBreak(paint);
                
                StartNewLine(false);

                start += charsToSpace;
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

        public void StartNewLine(bool paragraphStart)
        {
            _isParagraphStart = paragraphStart;
            _width = 0;
            _isFirstItemInLine = true;
        }
    }
}
