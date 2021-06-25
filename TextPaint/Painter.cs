using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public static class Painter
    {
        public static void Paint(ICurrentPage currentPage, ICanvas canvas, SKImageInfo info)
        {
            Paint(currentPage, canvas, info, out _);
        }

        public static void Paint(ICurrentPage currentPage, ICanvas canvas, SKImageInfo info, out LoadInfo loadInfo)
        {
            var point = new SKPoint(0, 0);
            
            canvas.Clear(SKColors.Black);

            var page = currentPage.GetPage(info.Width, info.Height);

            var ignoredTags = new List<string>();
            var w = Stopwatch.StartNew();
            var isFirstInLine = true;
            foreach (var drawingItem in page)
            {
                switch (drawingItem)
                {
                    case DrawingText text:
                        if (isFirstInLine)
                        {
                            point.Y += text.Paint.TextSize;
                            isFirstInLine = false;
                        }

                        text.Paint.Color = SKColors.White;
                        canvas.DrawText(text.Text, point, text.Paint);
                        point.X += text.Paint.MeasureText(text.Text);
                        break;

                    case EmptyLine emptyLine:
                        point.Y += emptyLine.Size;
                        break;

                    case LineBreak lineBreak:
                        point.X = 0;
                        isFirstInLine = true;
                        break;

                    case EmptySpace emptySpace:
                        point.X += emptySpace.Size;
                        break;

                    default:
                        if (!ignoredTags.Contains(drawingItem.GetType().Name))
                        {
                            ignoredTags.Add(drawingItem.GetType().Name);
                        }

                        break;
                }
            }

            w.Stop();
            loadInfo = new LoadInfo(ignoredTags, w.Elapsed);
        }
    }
}
