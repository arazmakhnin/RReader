using System;
using System.Linq;
using Fb2.Specification;
using SkiaSharp;

namespace TextPaint
{
    public class Painter
    {
        public static void Paint(FictionBook book, SKCanvas canvas)
        {
            var text = FindFirstText(book);
            if (text == null)
            {
                throw new InvalidOperationException("Text not found");
            }

            var point = new SKPoint(100, 100);
            var paint = new SKPaint();
            canvas.DrawText(text.Value, point, paint);
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
