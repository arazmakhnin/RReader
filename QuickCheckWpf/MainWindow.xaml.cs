using System.IO;
using System.Windows;
using Fb2;
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

#if DEBUG
            do
            {
                _splitter.GetPage(float.MaxValue, float.MaxValue);
            } while (_splitter.NextPage());

            File.WriteAllText($"{fileName}.info", "Parsing: \r\n" + book.LoadInfo + "\r\n\r\n=======\r\nSplitting:\r\n" + _splitter.LoadInfo);
#endif
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
