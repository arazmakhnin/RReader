using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fb2;
using Fb2.Specification;
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
            var book = Fb2Parser.LoadFile("c:\\projects\\1.fb2");
            File.WriteAllText("c:\\projects\\1.fb2.loadinfo", book.LoadInfo.ToString());

            var readingInfo = new ReadingInfo(0, 0);
            _splitter = new TextSplitter(book, readingInfo);
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
