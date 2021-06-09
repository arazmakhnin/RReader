using System;
using System.Collections.Generic;
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
        private FictionBook _book;

        public MainWindow()
        {
            InitializeComponent();
            _book = Fb2Parser.LoadFile("c:\\projects\\1.fb2");
        }

        private void SKElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            Painter.Paint(_book, e.Surface.Canvas);
        }
    }
}
