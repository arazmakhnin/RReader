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
        private readonly BaseItem[] _book;
        private ReadingInfo _currentPage;
        private ReadingInfo _nextPage;
        private readonly TextParameters _textParameters = new();

#if DEBUG
        public LoadInfo LoadInfo { get; private set; }
#endif

        public TextSplitter(FictionBook book, ReadingInfo startFrom)
        {
            _currentPage = startFrom;

            _book = book.Items.SkipWhile(i => i is not Body).ToArray();
        }

        public void ChangeParameters(TextParameters newParameters)
        {
            _textParameters.Apply(newParameters);
        }

        public IReadOnlyCollection<DrawingItem> GetPage(float maxWidth, float maxHeight)
        {
            var w = Stopwatch.StartNew();
            var ignoredTags = new List<string>();

            var result = new ItemsAggregation(_currentPage.LineIndex, maxHeight);
            var lineProcessor = new TextLineProcessor(_textParameters);
            var style = new TextStyle();

            var currentItemIndex = _currentPage.ItemIndex;
            for (var i = currentItemIndex; i < _book.Length; i++)
            {
                var item = _book[i];

                switch (item)
                {
                    case Text text:
                        foreach (var textPart in lineProcessor.ProcessText(text, style, maxWidth))
                        {
                            result.TryAdd(textPart);
                            if (result.EndOfPage)
                            {
                                break;
                            }
                        }

                        break;

                    case Body { TagType: TagType.Close }:
                        currentItemIndex = i + 1; // On the end of the book
                        result.StartNewItem();
                        break;

                    case Title title:
                        currentItemIndex = i;
                        result.StartNewItem();
                        lineProcessor.StartNewLine(true);
                        style.IsTitle = !style.IsTitle;

                        if (title.TagType == TagType.Close)
                        {
                            result.TryAdd(new EmptyLine(_textParameters.EmptyLineSize));
                        }
                        break;

                    case Section { TagType: TagType.Open }:
                        currentItemIndex = i + 1;
                        result.StartNewItem();
                        lineProcessor.StartNewLine(true);

                        if (!result.Empty)
                        {
                            result.StopCurrentPage();
                        }
                        break;

                    case Paragraph { TagType: TagType.Open }:
                        currentItemIndex = i;
                        result.StartNewItem();
                        lineProcessor.StartNewLine(true);
                        break;

                    case Paragraph { TagType: TagType.Close }:
                        result.TryAdd(new LineBreak(_textParameters.RegularTextPaint));
                        break;

                    case Strong:
                        style.IsStrong = !style.IsStrong;
                        break;

                    case Emphasis:
                        style.IsEmphasis = !style.IsEmphasis;
                        break;

                    case Fb2.Specification.EmptyLine:
                        result.TryAdd(new EmptyLine(_textParameters.EmptyLineSize));
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
    }

    public class TextParameters : IDisposable
    {
        public SKPaint RegularTextPaint { get; set; }
        public SKPaint TitlePaint { get; set; }

        public int ParagraphFirstLineIndent { get; set; } = 20;
        public int EmptyLineSize { get; set; } = 10;

        public TextParameters() : this(SKTypeface.Default)
        {
        }

        public TextParameters(string fontFamilyName) : this(SKTypeface.FromFamilyName(fontFamilyName))
        {
        }

        private TextParameters(SKTypeface typeface)
        {
            RegularTextPaint = new SKPaint(new SKFont
            {
                Typeface = typeface,
                Size = 20
            });

            TitlePaint = new SKPaint(new SKFont(typeface, RegularTextPaint.TextSize + 4))
            {
                TextAlign = SKTextAlign.Center
            };
        }

        public void Apply(TextParameters newParameters)
        {
            RegularTextPaint = newParameters.RegularTextPaint;
            TitlePaint = newParameters.TitlePaint;
            ParagraphFirstLineIndent = newParameters.ParagraphFirstLineIndent;
            EmptyLineSize = newParameters.EmptyLineSize;
        }

        public void Dispose()
        {
            RegularTextPaint?.Dispose();
            TitlePaint?.Dispose();
        }
    }

    public class TextStyle
    {
        public bool IsStrong { get; set; }
        public bool IsEmphasis { get; set; }
        public bool IsTitle { get; set; }

        public bool Any()
        {
            return IsStrong || IsEmphasis || IsTitle;
        }
    }

    public interface ICurrentPage
    {
        IReadOnlyCollection<DrawingItem> GetPage(float maxWidth, float maxHeight);
    }
}
