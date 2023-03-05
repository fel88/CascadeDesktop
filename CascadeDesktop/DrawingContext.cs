using OpenTK;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public class DrawingContext : IDrawingContext
    {
        public Graphics gr;
        public float scale = 1;


        public float startx, starty;
        public float origsx, origsy;
        public Vector2d startReal;
        public bool isDrag = false;
        private bool isMiddleDrag = false;
        public bool MiddleDrag { get { return isMiddleDrag; } }
        public float sx, sy;
        public float zoom = 1;

        public Bitmap Bmp;

        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        public PointF GetCursor()
        {
            var p = PictureBox.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }
        public void Init(PictureBox pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }
        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;

            pb.SizeChangedAction = Pb_SizeChanged;
            
        }
        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {            
            var pos = PictureBox.Control.PointToClient(Cursor.Position);
            if (!PictureBox.Control.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        public bool SnapEnable = false;
        public Vector2d? SnapPoint;

        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
            if (e.Button == MouseButtons.Middle)
            {
                isMiddleDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
                if (SnapEnable && SnapPoint != null)
                {
                    startReal = new Vector2d(SnapPoint.Value.X, SnapPoint.Value.Y);
                    var trsp = Transform(SnapPoint.Value);
                    startx = trsp.X;
                    starty = trsp.Y;
                }
            }

            var tt = BackTransform(e.X, e.Y);
            MouseDown?.Invoke(tt.X, tt.Y, e.Button);
        }

        public void ResetView()
        {
            zoom = 1;
            sx = 0;
            sy = 0;
        }

        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            Bmp = new Bitmap(PictureBox.Control.Width, PictureBox.Control.Height);
            gr = Graphics.FromImage(Bmp);
        }
        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            isMiddleDrag = false;

            var tt = BackTransform(e.X, e.Y);
            MouseUp?.Invoke(tt.X, tt.Y, e.Button);
        }

        public event Action<float, float, MouseButtons> MouseUp;
        public event Action<float, float, MouseButtons> MouseDown;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(float x, float y)
        {
            return new PointF((x + sx) * zoom, -(y + sy) * zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF((float)((x + sx) * zoom), (float)(-(y + sy) * zoom));
        }
        public virtual PointF Transform(SvgPoint p1)
        {
            return new PointF((float)((p1.X + sx) * zoom), (float)(-(p1.Y + sy) * zoom));
        }
        public virtual PointF Transform(Vector2d p1)
        {
            return new PointF((float)((p1.X + sx) * zoom), (float)(-(p1.Y + sy) * zoom));
        }

        public virtual PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
        public virtual PointF BackTransform(float x, float y)
        {
            var posx = (x / zoom - sx);
            var posy = (-y / zoom - sy);
            return new PointF(posx, posy);
        }
        public virtual PointF BackTransform(double x, double y)
        {
            var posx = (x / zoom - sx);
            var posy = (-y / zoom - sy);
            return new PointF((float)posx, (float)posy);
        }
        public EventWrapperPictureBox PictureBox;

        internal void FillEllipse(Brush black, float v1, float v2, float v3, float v4)
        {
            var pp = Transform(new PointF(v1, v2));
            gr.FillEllipse(black, pp.X, pp.Y, v3 * scale, v4 * scale);
        }

        internal void DrawLine(Pen black, PointF point, PointF point2)
        {
            var pp = Transform(point);
            var pp2 = Transform(point2);
            gr.DrawLine(black, pp, pp2);
        }

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = PictureBox.Control.Width;
            var h = PictureBox.Control.Height;

            var dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            var ky = h / dy;

            var oz = zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            zoom = kx;
            if (sz1.Width > w || sz1.Height > h) zoom = ky;

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            sx = ((w / 2f) / zoom - x);
            sy = -((h / 2f) / zoom + y);

            var test = Transform(new PointF(x, y));

        }
    }
}
