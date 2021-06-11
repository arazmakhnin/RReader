using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Fb2;
using Fb2.Specification;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using TextPaint;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RReader
{
    public partial class MainPage : ContentPage
    {
        private FictionBook _book;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    ((Button)sender).Text = "Nothing because " + status;
                    return;
                }
            }

            _book = Fb2Parser.LoadFile("/sdcard/Download/1.fb2");
            CanvasView.InvalidateSurface();
        }

        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_book == null)
            {
                return;
            }

            Painter.Paint(_book, e.Surface.Canvas, e.Info);
        }
    }
}
