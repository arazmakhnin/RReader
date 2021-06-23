using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public static class Painter
    {
        public static void Paint(ICurrentPage currentPage, SKCanvas canvas, SKImageInfo info)
        {
            Paint(currentPage, canvas, info, out _);
        }

        public static void Paint(ICurrentPage currentPage, SKCanvas canvas, SKImageInfo info, out LoadInfo loadInfo)
        {
            var point = new SKPoint(0, 0);
            
            canvas.DrawRect(0, 0, info.Width, info.Height, new SKPaint
            {
                Color = SKColors.Black
            });

            var page = currentPage.GetPage(info.Width, info.Height);

            var ignoredTags = new List<string>();
            var w = Stopwatch.StartNew();
            foreach (var drawingItem in page)
            {
                switch (drawingItem)
                {
                    case DrawingText text:
                        text.Paint.Color = SKColors.White;
                        point.Y += text.Paint.TextSize;
                        canvas.DrawText(text.Text, point, text.Paint);
                        break;

                    case EmptyLine emptyLine:
                        point.Y += emptyLine.Size;
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
