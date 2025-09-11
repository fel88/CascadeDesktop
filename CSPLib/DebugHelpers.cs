using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenTK;
using System.Linq;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Threading;
using OpenTK.Mathematics;

namespace CSPLib
{
    public static class DebugHelpers
    {

        public static Action<string> Error;
        public static Action<Exception> Exception;
        public static Action<string> Warning;
        public static Action<bool, float> Progress;
        public static void ToBitmap(Contour[] cntrs, Vector2d[][] triangls, float mult = 1, bool withTriang = false)
        {
            if (!Debugger.IsAttached) return;


            var maxx = cntrs.SelectMany(z => z.Elements).Max(z => Math.Max(z.Start.X, z.End.X));
            var minx = cntrs.SelectMany(z => z.Elements).Min(z => Math.Min(z.Start.X, z.End.X));
            var maxy = cntrs.SelectMany(z => z.Elements).Max(z => Math.Max(z.Start.Y, z.End.Y));
            var miny = cntrs.SelectMany(z => z.Elements).Min(z => Math.Min(z.Start.Y, z.End.Y));
            var dx = (float)(maxx - minx);
            var dy = (float)(maxy - miny);
            var mdx = Math.Max(dx, dy);
            Bitmap bmp = new Bitmap((int)(mdx * mult), (int)(mdx * mult));
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);

            foreach (var item in triangls)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon(item.Select(z => new PointF((float)((z.X - minx) / mdx * (bmp.Width - 1)),
                    (float)((z.Y - miny) / mdx * (bmp.Height - 1)))).ToArray());
                gr.FillPath(Brushes.LightBlue, gp);
                if (withTriang)
                {
                    gr.DrawPath(Pens.Black, gp);

                }
            }

            foreach (var cntr in cntrs)
            {
                foreach (var cc in cntr.Elements)
                {
                    var x1 = (float)(cc.Start.X - minx);
                    x1 = (x1 / mdx) * (bmp.Width - 1);
                    var y1 = (float)(cc.Start.Y - miny);
                    y1 = (y1 / mdx) * (bmp.Height - 1);
                    var x2 = (float)(cc.End.X - minx);
                    x2 = (x2 / mdx) * (bmp.Width - 1);
                    var y2 = (float)(cc.End.Y - miny);
                    y2 = (y2 / mdx) * (bmp.Height - 1);

                    gr.DrawLine(Pens.Black, x1, y1, x2, y2);
                }
            }

            ExecuteSTA(() => Clipboard.SetImage(bmp));
        }

        public static void ExecuteSTA(Action act)
        {
            if (!Debugger.IsAttached) return;
            Thread thread = new Thread(() => { act(); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public static bool DebugBitmapExportAllowed = false;

        public static void ToBitmap(Contour[] cntrs, float mult = 1)
        {
            if (!DebugBitmapExportAllowed) return;
            if (!Debugger.IsAttached) return;


            var maxx = cntrs.SelectMany(z => z.Elements).Max(z => Math.Max(z.Start.X, z.End.X));
            var minx = cntrs.SelectMany(z => z.Elements).Min(z => Math.Min(z.Start.X, z.End.X));
            var maxy = cntrs.SelectMany(z => z.Elements).Max(z => Math.Max(z.Start.Y, z.End.Y));
            var miny = cntrs.SelectMany(z => z.Elements).Min(z => Math.Min(z.Start.Y, z.End.Y));
            var dx = (float)(maxx - minx);
            var dy = (float)(maxy - miny);
            var mdx = Math.Max(dx, dy);
            Bitmap bmp = new Bitmap((int)(mdx * mult), (int)(mdx * mult));
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);

            foreach (var cntr in cntrs)
            {
                foreach (var cc in cntr.Elements)
                {
                    var x1 = (float)(cc.Start.X - minx);
                    x1 = (x1 / mdx) * (bmp.Width - 1);
                    var y1 = (float)(cc.Start.Y - miny);
                    y1 = (y1 / mdx) * (bmp.Height - 1);
                    var x2 = (float)(cc.End.X - minx);
                    x2 = (x2 / mdx) * (bmp.Width - 1);
                    var y2 = (float)(cc.End.Y - miny);
                    y2 = (y2 / mdx) * (bmp.Height - 1);

                    gr.DrawLine(Pens.Black, x1, y1, x2, y2);
                }
            }

            ExecuteSTA(() => Clipboard.SetImage(bmp));
        }
    }
}
