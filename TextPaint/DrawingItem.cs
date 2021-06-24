using SkiaSharp;

namespace TextPaint
{
    public abstract class DrawingItem
    {
        public abstract float GetHeight { get; }
    }

    public class DrawingText : DrawingItem
    {
        public string Text { get; }
        public SKPaint Paint { get; }

        public override float GetHeight => Paint.TextSize;

        public DrawingText(string text, SKPaint paint)
        {
            Text = text;
            Paint = paint;
        }
    }

    public class LineBreak : DrawingItem
    {
        public SKPaint Paint { get; }

        public override float GetHeight => Paint.TextSize;

        public LineBreak(SKPaint paint)
        {
            Paint = paint;
        }
    }

    public class EmptyLine : DrawingItem
    {
        public int Size { get; }

        public override float GetHeight => Size;

        public EmptyLine(int size)
        {
            Size = size;
        }
    }
}
