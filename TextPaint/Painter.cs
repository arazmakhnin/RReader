using System;
using System.Drawing;
using System.Linq;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public class Painter
    {
        public static void Paint(FictionBook book, SKCanvas canvas, SKImageInfo info)
        {
            var text = FindFirstText(book)?.Value;
            if (text == null)
            {
                throw new InvalidOperationException("Text not found");
            }

            var paint = new SKPaint { TextSize = 20 };
            var point = new SKPoint(0, 0);

            foreach (var drawingText in TextSplitter.Split(text, paint, info.Width))
            {
                point.Y += paint.TextSize;
                canvas.DrawText(drawingText.Text, point, paint);
            }
        }

        private static Text FindFirstText(BaseItem item)
        {
            if (item is Text text)
            {
                return text;
            }

            foreach (var node in item.Items)
            {
                var z = FindFirstText(node);
                if (z != null)
                {
                    return z;
                }
            }

            return null;
        }
    }
}
