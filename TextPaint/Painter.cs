using System;
using System.Collections.Generic;
using SkiaSharp;

namespace TextPaint
{
    public static class Painter
    {
        public static void Paint(ICurrentPage currentPage, SKCanvas canvas, SKImageInfo info)
        {
            var paint = new SKPaint { TextSize = 20, Color = SKColors.White };
            var point = new SKPoint(0, 0);

            canvas.DrawRect(0, 0, info.Width, info.Height, new SKPaint()
            {
                Color = SKColors.Black
            });

            foreach (var drawingItem in currentPage.GetPage(paint, info.Width, info.Height))
            {
                switch (drawingItem)
                {
                    case DrawingText text:
                        point.Y += paint.TextSize;
                        canvas.DrawText(text.Text, point, paint);
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
