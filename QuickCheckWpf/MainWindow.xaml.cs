using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Fb2;
using Fb2.Specification;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using TextPaint;

namespace QuickCheckWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextSplitter _splitter;

        public MainWindow()
        {
            InitializeComponent();
            const string fileName = "c:\\projects\\1.fb2";
            var book = Fb2Parser.LoadFile(fileName);

            var readingInfo = new ReadingInfo(0, 0);
            _splitter = new TextSplitter(book, readingInfo);

            DebugInfo(fileName, book);
        }

        [Conditional("DEBUG")]
        private void DebugInfo(string fileName, FictionBook book)
        {
            using var bitmap = new SKBitmap();
            using var canvas = new SKCanvas(bitmap);

            Painter.Paint(_splitter, canvas, new SKImageInfo(int.MaxValue, int.MaxValue), out var drawInfo);

            var builder = new StringBuilder()
                .AppendLine("== Parsing ==")
                .AppendLine(book.LoadInfo.ToString())
                .AppendLine()
                .AppendLine("== Splitting ==");

            var fullBookSplit = !_splitter.NextPage();
            if (!fullBookSplit)
            {
                builder.AppendLine(" !! Full book wasn't split !!")
                    .AppendLine();
            }

            builder.AppendLine(_splitter.LoadInfo.ToString())
                .AppendLine()
                .AppendLine("== Drawing ==")
                .AppendLine(drawInfo.ToString());

            File.WriteAllText($"{fileName}.info", builder.ToString());
        }

        private void SKElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            Painter.Paint(_splitter, e.Surface.Canvas, e.Info);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _splitter.NextPage();
            CanvasView.InvalidateVisual();
        }
    }
}
