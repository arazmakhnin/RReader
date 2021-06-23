using System.Collections.Generic;
using SkiaSharp;

namespace TextPaint
{
    public static class Painter
    {
        public static void Paint(TextSplitter splitter, SKCanvas canvas, SKImageInfo info)
        {
            var paint = new SKPaint { TextSize = 20, Color = SKColors.White };
            var point = new SKPoint(0, 0);

            canvas.DrawRect(0, 0, info.Width, info.Height, new SKPaint()
            {
                Color = SKColors.Black
            });

            foreach (var drawingItem in splitter.GetPage(paint, info.Width, info.Height))
            {
                if (drawingItem is DrawingText text)
                {
                    point.Y += paint.TextSize;
                    canvas.DrawText(text.Text, point, paint);
                }
            }
        }
    }
}
