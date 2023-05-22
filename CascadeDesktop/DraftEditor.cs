using AutoDialog;
using Cascade.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;

            propertyGrid1.Visible = false;
            propertyGrid1.Dock = DockStyle.None;
            propertyGrid1.HelpVisible = false;
            pictureBox1.Controls.Add(propertyGrid1);
            propertyGrid1.Width = 150;
            propertyGrid1.Height = 150;
            propertyGrid1.Left = 0;
            propertyGrid1.Top = pictureBox1.Height - propertyGrid1.Height;
            propertyGrid1.SelectedObjectsChanged += PropertyGrid1_SelectedObjectsChanged;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteItem();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (selectedItem is Arc2d arc)
            {
                arc.UpdateMiddle();
            }
        }

        private void PropertyGrid1_SelectedObjectsChanged(object sender, EventArgs e)
        {
            propertyGrid1.Top = pictureBox1.Height - propertyGrid1.Height;
        }

        private void DraftEditor_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        BlueprintItem selectedItem = null;
        PointF? lastClickPosition = null;
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var bpos = ctx.BackTransform(pos);
            if (e.Button == MouseButtons.Left)
            {
                propertyGrid1.SelectedObject = null;
                BlueprintItem prevSelected = selectedItem;

                foreach (var item in blueprint.Items)
                {
                    if (item == prevSelected && lastClickPosition != null && (lastClickPosition.Value.ToVector2d() - bpos.ToVector2d()).Length < 1)
                        continue;

                    if (item is Line2D l)
                    {
                        var dist = (new Vector2d(l.Start.X, l.Start.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist < 5)
                        {
                            propertyGrid1.SelectedObject = l.Start;
                            selectedItem = l;
                        }
                        var dist2 = (new Vector2d(l.End.X, l.End.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist2 < 5)
                        {
                            propertyGrid1.SelectedObject = l.End;
                            selectedItem = l;
                        }
                    }
                    if (item is Arc2d arc)
                    {

                        var dist = (new Vector2d(arc.Center.X, arc.Center.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist < 5)
                        {
                            propertyGrid1.SelectedObject = arc.Center;
                            selectedItem = arc;
                        }
                        if (Math.Abs(dist - arc.Radius) < 5)
                        {
                            propertyGrid1.SelectedObject = arc;
                            selectedItem = arc;
                        }
                        var dist1 = (new Vector2d(arc.Start.X, arc.Start.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist1 < 5)
                        {
                            propertyGrid1.SelectedObject = arc.Start;
                            selectedItem = arc;
                        }
                        var dist2 = (new Vector2d(arc.End.X, arc.End.Y) - new Vector2d(bpos.X, bpos.Y)).Length;
                        if (dist2 < 5)
                        {
                            propertyGrid1.SelectedObject = arc.End;
                            selectedItem = arc;
                        }

                    }
                }

                propertyGrid1.Visible = propertyGrid1.SelectedObject != null;
            }
            lastClickPosition = bpos;
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
                foreach (var item2 in item.Items)
                {
                    DrawItem(item2, Pens.Blue);
                }
            }

            foreach (var item in blueprint.Items)
            {
                DrawItem(item, Pens.Black);
            }
        }

        public void DrawItem(BlueprintItem item, Pen pen)
        {
            if (selectedItem == item)
            {
                pen = Pens.Blue;
            }
            if (item is Line2D l)
            {
                var t1 = ctx.Transform(l.Start.X, l.Start.Y);
                var t2 = ctx.Transform(l.End.X, l.End.Y);

                var size = 4 * ctx.zoom;
                AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
                Pen pen1 = new Pen(pen.Color);

                pen1.CustomEndCap = bigArrow;
                ctx.gr.DrawLine(pen1, t1, t2);

                int gap = 5;
                ctx.gr.DrawRectangle(Pens.Blue, t1.X - gap, t1.Y - gap, gap * 2, gap * 2);
                ctx.gr.DrawRectangle(Pens.Blue, t2.X - gap, t2.Y - gap, gap * 2, gap * 2);

            }
            if (item is BlueprintPolyline pl)
            {
                int gap = 5;
                var pp = pl.Points;
                var t = pp.Select(z => ctx.Transform(z.X, z.Y)).ToArray();

                ctx.gr.DrawPolygon(pen, t);
                for (int i = 0; i < pl.Points.Count; i++)
                {
                    var t1 = ctx.Transform(pl.Points[i].X, pl.Points[i].Y);
                    ctx.gr.DrawRectangle(Pens.Blue, t1.X - gap, t1.Y - gap, gap * 2, gap * 2);
                }
            }
            if (item is Arc2d arc)
            {
                var t1 = ctx.Transform(arc.Center.X, arc.Center.Y);

                var size = 4 * ctx.zoom;
                AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
                Pen pen1 = new Pen(pen.Color);

                if (arc.CCW)
                {
                    pen1.CustomStartCap = bigArrow;
                }
                else
                {
                    pen1.CustomEndCap = bigArrow;
                }
                ctx.gr.DrawArc(pen1, (float)(t1.X - arc.Radius * ctx.zoom),
                    (float)(t1.Y - arc.Radius * ctx.zoom),
                    (float)arc.Radius * 2 * ctx.zoom, (float)arc.Radius * 2 * ctx.zoom, (float)arc.AngleStart, (float)arc.AngleSweep);
                int gap = 5;
                ctx.gr.DrawRectangle(Pens.Blue, t1.X - gap, t1.Y - gap, gap * 2, gap * 2);
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
            var arc = new Arc2d()
            {
                Center = new Vertex2D(0, 0),
                AngleStart = 0,
                AngleSweep = 180,
                Radius = 50
            };
            arc.UpdateMiddle();
            blueprint.Items.Add(arc);
        }

        private void rectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("w", "Width", 50);
            d.AddNumericField("h", "Height", 50);
            d.AddBoolField("r", "Reversed");

            d.ShowDialog();

            var w = d.GetNumericField("w");
            var h = d.GetNumericField("h");
            var r = d.GetBoolField("r");

            List<BlueprintItem> items = new List<BlueprintItem>();
            items.Add(new Line2D() { Start = new Vertex2D(0, 0), End = new Vertex2D(w, 0) });
            items.Add(new Line2D() { Start = new Vertex2D(w, 0), End = new Vertex2D(w, h) });
            items.Add(new Line2D() { Start = new Vertex2D(w, h), End = new Vertex2D(0, h) });
            items.Add(new Line2D() { Start = new Vertex2D(0, h), End = new Vertex2D(0, 0) });
            foreach (var item in items.OfType<Line2D>())
            {
                item.Start.X -= w / 2;
                item.Start.Y -= h / 2;
                item.End.X -= w / 2;
                item.End.Y -= h / 2;
            }
            if (r)
            {
                foreach (var item in items)
                {
                    item.Reverse();
                }
            }
            blueprint.Items.AddRange(items);
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


        BlueprintContour[] ConnectContour(BlueprintItem[] items)
        {
            List<BlueprintContour> rets = new List<BlueprintContour>();
            List<BlueprintItem> remains = new List<BlueprintItem>();

            BlueprintContour ret = new BlueprintContour();
            rets.Add(ret);
            ret.Items.Add(items[0]);
            remains.AddRange(items.Skip(1));
            float eps = 1e-5f;
            while (remains.Any())
            {
                var p2 = ret.Items.Last().End;
                var p1 = ret.Items.First().Start;
                BlueprintItem todel = null;
                foreach (var item in remains)
                {

                    var dist1 = (item.Start.ToVector2d() - p2.ToVector2d()).Length;
                    if (dist1 < eps)
                    {
                        ret.Items.Add(item);
                        todel = item;
                        break;
                    }
                    var dist2 = (item.End.ToVector2d() - p2.ToVector2d()).Length;
                    if (dist2 < eps)
                    {
                        item.Reverse();
                        ret.Items.Add(item);

                        todel = item;
                        break;
                    }
                }

                if (todel == null)
                {
                    //new contour
                    ret = new BlueprintContour();
                    rets.Add(ret);
                    todel = remains[0];
                    ret.Items.Add(remains[0]);
                }

                remains.Remove(todel);
            }

            return rets.ToArray();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (var item in blueprint.Items.OfType<Arc2d>())
            {
                item.UpdateMiddle();
            }
            var contours = ConnectContour(blueprint.Items.ToArray());

            blueprint.Items.Clear();
            blueprint.Contours.AddRange(contours);

            //BlueprintContour contour = new BlueprintContour();
            //bool first = true;
            //foreach (var b in blueprint.Items)
            //{
            //    if (b is Line2D l)
            //    {
            //        if (first)
            //        {
            //            contour.Points.Add(new Vertex(l.Start.X, l.Start.Y, 0));
            //            first = false;
            //        }
            //        contour.Points.Add(new Vertex(l.End.X, l.End.Y, 0));

            //    }
            //}
            ////contour.Points.Add(new Vertex(contour.Points[0].X, contour.Points[0].Y, 0));
            //blueprint.Items.Clear();
            //blueprint.Contours.Add(contour);
            //if (blueprint.Contours.Count > 1)
            //{
            //    int sign = Math.Sign(StaticHelpers.signed_area(blueprint.Contours[0].Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray()));
            //    if (Math.Sign(StaticHelpers.signed_area(contour.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray())) == sign)
            //    {
            //        contour.Points.Reverse();
            //    }
            //}
        }

        public void DeleteItem()
        {
            if (selectedItem == null)
                return;

            blueprint.Items.Remove(selectedItem);
            selectedItem = null;
            propertyGrid1.SelectedObject = null;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DeleteItem();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var d = DialogHelpers.StartDialog();
            d.AddNumericField("x", "X", 0);
            d.AddNumericField("y", "Y", 0);

            d.ShowDialog();

            var x = d.GetNumericField("x");
            var y = d.GetNumericField("y");

            foreach (var b in blueprint.Items)
            {
                b.Start.X += x;
                b.End.X += x;

                b.Start.Y += y;
                b.End.Y += y;
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            blueprint.Items.Clear();
        }
    }
}
