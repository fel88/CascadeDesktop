using Cascade.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public partial class DraftEditor : Form
    {
        PropertyGrid propertyGrid1;
        public DraftEditor()
        {
            InitializeComponent();
            pictureBox1.Paint += PictureBox1_Paint;
            ctx.Init(pictureBox1);
            FormClosing += DraftEditor_FormClosing;
            pictureBox1.MouseUp += PictureBox1_MouseUp;

            propertyGrid1 = new PropertyGrid();

            propertyGrid1.Visible = false;
            propertyGrid1.Dock = DockStyle.None;
            propertyGrid1.HelpVisible = false;
            pictureBox1.Controls.Add(propertyGrid1);
            propertyGrid1.Width = 150;
            propertyGrid1.Height = 100;
            propertyGrid1.Left = 0;
            propertyGrid1.Top = pictureBox1.Height - 100;
        }

        private void DraftEditor_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var bpos = ctx.BackTransform(pos);
            if (e.Button == MouseButtons.Left)
            {
                propertyGrid1.SelectedObject = null;
                foreach (var item in blueprint.Items)
                {
                    if (item is Line2D l)
                    {
                        var dist = (new Vector2d(l.Start.X, l.Start.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist < 5)
                        {
                            propertyGrid1.SelectedObject = l.Start;
                        }
                        var dist2 = (new Vector2d(l.End.X, l.End.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist2 < 5)
                        {
                            propertyGrid1.SelectedObject = l.End;
                        }
                    }
                    if (item is Arc2D arc)
                    {

                        var dist = (new Vector2d(arc.Center.X, arc.Center.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist < 5)
                        {
                            propertyGrid1.SelectedObject = arc.Center;
                        }
                        if (Math.Abs(dist - arc.Radius) < 5)
                        {
                            propertyGrid1.SelectedObject = arc;
                        }

                    }
                }

                propertyGrid1.Visible = propertyGrid1.SelectedObject != null;
            }
        }

        Blueprint blueprint = new Blueprint();
        public Blueprint Blueprint => blueprint;

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var bpos = ctx.BackTransform(pos);
            var gr = e.Graphics;
            ctx.gr = gr;
            ctx.UpdateDrag();
            gr.Clear(Color.White);
            gr.DrawString($"{bpos.X} {bpos.Y}", SystemFonts.DefaultFont, Brushes.Blue, 5, 5);
            ctx.DrawLine(Pens.Red, new PointF(0, 0), new PointF(100, 0));
            ctx.DrawLine(Pens.Blue, new PointF(0, 0), new PointF(0, 100));

            foreach (var item in blueprint.Contours)
            {
                var pp = item.Points;
                var t = pp.Select(z => ctx.Transform(z.X, z.Y)).ToArray();

                ctx.gr.DrawPolygon(Pens.Blue, t);

            }
            foreach (var item in blueprint.Items)
            {
                if (item is Line2D l)
                {
                    var t1 = ctx.Transform(l.Start.X, l.Start.Y);
                    var t2 = ctx.Transform(l.End.X, l.End.Y);
                    ctx.gr.DrawLine(Pens.Black, t1, t2);
                    int gap = 5;
                    ctx.gr.DrawRectangle(Pens.Blue, t1.X - gap, t1.Y - gap, gap * 2, gap * 2);
                    ctx.gr.DrawRectangle(Pens.Blue, t2.X - gap, t2.Y - gap, gap * 2, gap * 2);
                }
                if (item is Arc2D arc)
                {
                    var t1 = ctx.Transform(arc.Center.X, arc.Center.Y);

                    ctx.gr.DrawArc(Pens.Black, (float)(t1.X - arc.Radius * ctx.zoom),
                        (float)(t1.Y - arc.Radius * ctx.zoom),
                        (float)arc.Radius * 2 * ctx.zoom, (float)arc.Radius * 2 * ctx.zoom, (float)arc.AngleStart, (float)arc.AngleSweep);
                    int gap = 5;
                    ctx.gr.DrawRectangle(Pens.Blue, t1.X - gap, t1.Y - gap, gap * 2, gap * 2);
                }
            }
        }

        DrawingContext ctx = new DrawingContext();

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            blueprint.Items.Add(new Line2D() { Start = new Vertex2D(0, 0), End = new Vertex2D(100, 100) });
        }

        private void arcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            blueprint.Items.Add(new Arc2D() { Center = new Vertex2D(0, 0), AngleStart = 0, AngleSweep = 180, Radius = 50 });
        }

        private void rectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("w", "Width", 50);
            d.AddNumericField("h", "Height", 50);


            d.ShowDialog();

            var w = d.GetNumericField("w");
            var h = d.GetNumericField("h");

            blueprint.Items.Add(new Line2D() { Start = new Vertex2D(0, 0), End = new Vertex2D(w, 0) });
            blueprint.Items.Add(new Line2D() { Start = new Vertex2D(w, 0), End = new Vertex2D(w, h) });
            blueprint.Items.Add(new Line2D() { Start = new Vertex2D(w, h), End = new Vertex2D(0, h) });
            blueprint.Items.Add(new Line2D() { Start = new Vertex2D(0, h), End = new Vertex2D(0, 0) });
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            Close();
        }

        private void hexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 50);


            d.ShowDialog();

            var r = d.GetNumericField("r");
            List<Vector2d> pnts = new List<Vector2d>();
            for (double i = 0; i < Math.PI * 2; i += (Math.PI * 2 / 6f))
            {
                var xx = r * Math.Cos(i);
                var yy = r * Math.Sin(i);
                pnts.Add(new Vector2d(xx, yy));
            }
            for (int i = 1; i <= pnts.Count; i++)
            {
                blueprint.Items.Add(new Line2D()
                {
                    Start = new Vertex2D(pnts[i - 1].X, pnts[i - 1].Y),
                    End = new Vertex2D(pnts[i % pnts.Count].X, pnts[i % pnts.Count].Y)
                });
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            blueprint.Items.Clear();
            blueprint.Contours.Clear();
        }

        private void pseudoCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("r", "Radius", 50);
            d.AddNumericField("n", "N", 6);


            d.ShowDialog();

            var r = d.GetNumericField("r");
            var n = d.GetNumericField("n");
            List<Vector2d> pnts = new List<Vector2d>();
            for (double i = 0; i < Math.PI * 2; i += (Math.PI * 2 / n))
            {
                var xx = r * Math.Cos(i);
                var yy = r * Math.Sin(i);
                pnts.Add(new Vector2d(xx, yy));
            }
            for (int i = 1; i <= pnts.Count; i++)
            {
                blueprint.Items.Add(new Line2D()
                {
                    Start = new Vertex2D(pnts[i - 1].X, pnts[i - 1].Y),
                    End = new Vertex2D(pnts[i % pnts.Count].X, pnts[i % pnts.Count].Y)
                });
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            BlueprintContour contour = new BlueprintContour();
            bool first = true;
            foreach (var b in blueprint.Items)
            {
                if (b is Line2D l)
                {
                    if (first)
                    {
                        contour.Points.Add(new Vertex(l.Start.X, l.Start.Y, 0));
                        first = false;
                    }
                    contour.Points.Add(new Vertex(l.End.X, l.End.Y, 0));

                }
            }
            //contour.Points.Add(new Vertex(contour.Points[0].X, contour.Points[0].Y, 0));
            blueprint.Items.Clear();
            blueprint.Contours.Add(contour);
            if (blueprint.Contours.Count > 1)
            {
                int sign = Math.Sign(StaticHelpers.signed_area(blueprint.Contours[0].Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray()));
                if (Math.Sign(StaticHelpers.signed_area(contour.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray())) == sign)
                {
                    contour.Points.Reverse();
                }
            }
        }
    }
}
