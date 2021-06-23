using System;
using System.Collections.Generic;
using SkiaSharp;

namespace TextPaint
{
    public static class Painter
    {
        public static void Paint(ICurrentPage currentPage, SKCanvas canvas, SKImageInfo info)
        {
            var point = new SKPoint(0, 0);

            canvas.DrawRect(0, 0, info.Width, info.Height, new SKPaint
            {
                Color = SKColors.Black
            });

            foreach (var drawingItem in currentPage.GetPage(info.Width, info.Height))
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
                        throw new ArgumentOutOfRangeException(nameof(drawingItem),
                            $"Unknown type: {drawingItem.GetType().Name}");
                }
            }
        }
    }
}
