using SkiaSharp;

namespace TextPaint
{
    public class FormCanvas : ICanvas
    {
        private readonly SKCanvas _canvas;

        public FormCanvas(SKCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Clear(SKColor color)
        {
            _canvas.Clear(color);
        }

        public void DrawText(string text, SKPoint point, SKPaint paint)
        {
            _canvas.DrawText(text, point, paint);
        }
    }

    public interface ICanvas
    {
        void Clear(SKColor color);
        void DrawText(string text, SKPoint point, SKPaint paint);
    }
}
