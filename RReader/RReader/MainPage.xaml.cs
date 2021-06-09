using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RReader
{
    public partial class MainPage : ContentPage
    {
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

            var text = File.ReadAllText("/sdcard/Download/1.fb2");
            var doc = XDocument.Parse(text);
            ParseFb2(doc.Root.Elements());
        }

        private void ParseFb2(IEnumerable<XElement> nodes)
        {
            foreach (var node in nodes)
            {
                switch (node.Name.LocalName)
                {
                    case "FictionBook":
                    case "body":
                        ParseFb2(node.Elements());
                        break;

                    
                }
            }
        }

        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Red.ToSKColor(),
                StrokeWidth = 25
            };

            canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = SKColors.Blue;
            canvas.DrawCircle(e.Info.Width / 2, e.Info.Height / 2, 100, paint);

            Painter.Paint(canvas);
        }
    }
}
