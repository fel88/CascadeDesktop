using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using SkiaSharp;
using System.Diagnostics;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml;
using System.Numerics;
using System.Drawing.Drawing2D;
using ClipperLib;
using Vector2 = OpenTK.Vector2;
using System.Threading;
using TriangleNet.Geometry;
using System.Reflection;
using TriangleNet.Meshing;
using DxfPad;

namespace CSPLib
{
    public partial class DraftEditorControl : UserControl, IDraftEditor
    {
        public DraftEditorControl()
        {
            InitializeComponent();
            ctx = Activator.CreateInstance(DrawerType) as IDrawingContext;
            ctx.DragButton = MouseButtons.Right;
            //new SkiaGLDrawingContext() { DragButton = MouseButtons.Right };
            RenderControl = ctx.GenerateRenderControl();
            Controls.Add(RenderControl);
            RenderControl.Dock = DockStyle.Fill;
            ctx.Init(RenderControl);
            ctx.PaintAction = () => { Render(); };

            ctx.InitGraphics();
            ctx.MouseDown += Ctx_MouseDown;
            RenderControl.MouseUp += PictureBox1_MouseUp;
            RenderControl.MouseDown += PictureBox1_MouseDown;
            ctx.Tag = this;
            InitPaints();
        }
        Control RenderControl;

        SKPaint PointPaint;
        public void InitPaints()
        {
            PointPaint = new SKPaint();
            var clr = Pens.Black.Color;
            PointPaint.Color = new SKColor(clr.R, clr.G, clr.B);
            PointPaint.IsAntialias = true;
            PointPaint.StrokeWidth = Pens.Black.Width;
            PointPaint.Style = SKPaintStyle.Stroke;
        }

        void Render()
        {
            var sw = Stopwatch.StartNew();

            ctx.Clear(Color.White); //same thing but also erases anything else on the canvas first

            //   ctx.gr.Clear(Color.White);
            ctx.UpdateDrag();
            subSnapType = SubSnapTypeEnum.None;
            updateNearest();
            ctx.SetPen(Pens.Blue);
            ctx.DrawLineTransformed(new PointF(0, 0), new PointF(0, 100));
            ctx.SetPen(Pens.Red);
            ctx.DrawLineTransformed(new PointF(0, 0), new PointF(100, 0));

            if (_draft != null)
            {
                var dpnts = _draft.DraftPoints.ToArray();
                if (ShowHelpers)
                {
                    foreach (var item in _draft.Helpers.Where(z => z.Z < 0))
                    {
                        if (!item.Visible) continue;

                        item.Draw(ctx);
                    }
                }
                ctx.SetPen(Pens.Black);
                for (int i = 0; i < dpnts.Length; i++)
                {
                    var item = dpnts[i];
                    float gp = 5;
                    var tr = ctx.Transform(item.X, item.Y);

                    if (nearest == item || selected.Contains(item))
                    {
                        ctx.FillRectangle(Brushes.Blue, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                    }
                    ctx.DrawRectangle(tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                }

                var dlns = _draft.DraftLines.ToArray();
                for (int i = 0; i < dlns.Length; i++)
                {
                    var el = dlns[i];

                    Vector2d item0 = dlns[i].V0.Location;
                    Vector2d item = dlns[i].V1.Location;
                    var tr = ctx.Transform(item0.X, item0.Y);
                    var tr11 = ctx.Transform(item.X, item.Y);
                    Pen p = new Pen(selected.Contains(el) ? Color.Blue : Color.Black);

                    if (el.Dummy)
                        p.DashPattern = new float[] { 10, 10 };

                    //ctx.gr.DrawLine(p, tr, tr11);
                    ctx.SetPen(p);
                    ctx.DrawLine(tr, tr11);
                }

                var elps = _draft.DraftEllipses.Where(z => !z.SpecificAngles).ToArray();
                for (int i = 0; i < elps.Length; i++)
                {
                    var el = elps[i];
                    Vector2d item0 = elps[i].Center.Location;
                    var rad = (float)el.Radius * ctx.zoom;
                    var tr = ctx.Transform(item0.X, item0.Y);

                    Pen p = new Pen(selected.Contains(el) ? Color.Blue : Color.Black);

                    if (el.Dummy)
                        p.DashPattern = new float[] { 10, 10 };
                    if (nearest == el.Center || selected.Contains(el.Center))
                    {
                        p.Width = 2;
                        p.Color = Color.Blue;
                        ctx.DrawCircle(p, tr.X, tr.Y, rad);
                    }
                    else
                        ctx.DrawCircle(p, tr.X, tr.Y, rad);

                    float gp = 5;
                    tr = ctx.Transform(el.Center.X, el.Center.Y);

                    if (nearest == el.Center || selected.Contains(el.Center))
                    {
                        ctx.FillRectangle(Brushes.Blue, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                    }
                    ctx.SetPen(p);
                    ctx.DrawRectangle(tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                }

                var hexes = _draft.DraftEllipses.Where(z => z.SpecificAngles).ToArray();
                for (int i = 0; i < hexes.Length; i++)
                {
                    var el = hexes[i];
                    Vector2d item0 = hexes[i].Center.Location;
                    var rad = (float)el.Radius * ctx.zoom;
                    var tr = ctx.Transform(item0.X, item0.Y);

                    Pen p = new Pen(selected.Contains(el) ? Color.Blue : Color.Black);

                    if (el.Dummy)
                        p.DashPattern = new float[] { 10, 10 };
                    if (nearest == el.Center || selected.Contains(el.Center))
                    {
                        p.Width = 2;
                        p.Color = Color.Blue;
                    }

                    ctx.DrawCircle(p, tr.X, tr.Y, rad, el.Angles, 0);

                    float gp = 5;
                    tr = ctx.Transform(el.Center.X, el.Center.Y);

                    if (nearest == el.Center || selected.Contains(el.Center))
                    {
                        ctx.FillRectangle(Brushes.Blue, tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                    }
                    ctx.SetPen(p);
                    ctx.DrawRectangle(tr.X - gp, tr.Y - gp, gp * 2, gp * 2);
                }

                if (ShowHelpers)
                {
                    foreach (var item in _draft.Helpers.Where(z => z.Z >= 0))
                    {
                        if (!item.Visible) continue;

                        item.Draw(ctx);
                    }
                }
            }
            if (ctx.MiddleDrag)//measure tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                ctx.SetPen(pen);
                Pen pen2 = new Pen(Color.White, 2);

                var gcur = ctx.GetCursor();
                var curp = ctx.Transform(gcur);
                double maxSnapDist = 10 / ctx.zoom;

                //check perpendicular of lines?
                foreach (var item in Draft.DraftLines)
                {
                    //get projpoint                     
                    var proj = GeometryUtils.GetProjPoint(gcur.ToVector2d(), item.V0.Location, item.Dir);
                    var sx = ctx.BackTransform(ctx.startx, ctx.starty);
                    var proj2 = GeometryUtils.GetProjPoint(new Vector2d(sx.X, sx.Y), item.V0.Location, item.Dir);
                    if (!item.ContainsPoint(proj))
                        continue;

                    var len = (proj - gcur.ToVector2d()).Length;
                    var len2 = (proj2 - gcur.ToVector2d()).Length;
                    if (len < maxSnapDist)
                    {
                        //sub nearest = projpoint
                        curp = ctx.Transform(proj);
                        gcur = proj.ToPointF();
                        subSnapType = SubSnapTypeEnum.PointOnLine;
                    }
                    if (len2 < maxSnapDist)
                    {
                        //sub nearest = projpoint
                        curp = ctx.Transform(proj2);
                        gcur = proj2.ToPointF();
                        subSnapType = SubSnapTypeEnum.Perpendicular;
                    }
                }

                if (nearest is DraftPoint dp)
                {
                    curp = ctx.Transform(dp.Location);
                    gcur = dp.Location.ToPointF();
                }
                if (nearest is DraftLine dl)
                {
                    var len = (dl.Center - gcur.ToVector2d()).Length;
                    if (len < maxSnapDist)
                    {
                        curp = ctx.Transform(dl.Center);
                        gcur = dl.Center.ToPointF();
                        subSnapType = SubSnapTypeEnum.CenterLine;
                    }
                }
                if (nearest is DraftEllipse de)
                {
                    var diff = (de.Center.Location - new Vector2d(gcur.X, gcur.Y)).Normalized();
                    var onEl = de.Center.Location - diff * (double)de.Radius;
                    //get point on ellipse
                    curp = ctx.Transform(onEl);
                    gcur = onEl.ToPointF();
                }
                var t = ctx.Transform(new PointF(ctx.startx, ctx.starty));

                //snap starto
                if (startMiddleDragNearest is DraftPoint sdp)
                {

                }
                ctx.SetPen(pen2);

                ctx.DrawLine(ctx.startx, ctx.starty, curp.X, curp.Y);
                ctx.SetPen(pen);

                ctx.DrawLine(ctx.startx, ctx.starty, curp.X, curp.Y);

                //ctx.gr.DrawLine(pen, ctx.startx, ctx.starty, curp.X, curp.Y);
                var pp = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(gcur.X, gcur.Y);
                var dist = (v2 - v1).Length;
                var hintText = dist.ToString("N2");
                if (subSnapType == SubSnapTypeEnum.PointOnLine)
                {
                    hintText = "[point on line] : " + hintText;
                }
                if (subSnapType == SubSnapTypeEnum.CenterLine)
                {
                    hintText = "[line center] : " + hintText;
                }
                if (subSnapType == SubSnapTypeEnum.Perpendicular)
                {
                    hintText = "[perpendicular] : " + hintText;
                }
                var mss = ctx.MeasureString(hintText, SystemFonts.DefaultFont);

                ctx.FillRectangle(Brushes.White, curp.X + 10, curp.Y, mss.Width, mss.Height);
                ctx.DrawString(hintText, SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);


            }
            if (ctx.isLeftDrag)//rect tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                var gcur = ctx.GetCursor();

                var curp = ctx.Transform(gcur);

                var t = ctx.Transform(new PointF(ctx.startx, ctx.starty));
                var rxm = Math.Min(ctx.startx, curp.X);
                var rym = Math.Min(ctx.starty, curp.Y);
                var rdx = Math.Abs(ctx.startx - curp.X);
                var rdy = Math.Abs(ctx.starty - curp.Y);
                ctx.SetPen(pen);
                ctx.DrawRectangle(rxm, rym, rdx, rdy);
                var pp = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(gcur.X, gcur.Y);
                var dist = (v2 - v1).Length;
                ctx.DrawString(dist.ToString("N2"), SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);


            }
            editor.CurrentTool.Draw();

            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            LastRenderTime = ms;

            ctx.DrawString("current tool: " + editor.CurrentTool.GetType().Name, SystemFonts.DefaultFont, Brushes.Black, 5, 5);

        }

        internal void AddImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;
            //Draft.Helpers.Add(new ImageDraftHelper(Draft, bmp));
        }

        internal void ArrayUI()
        {
            //var points = selected.OfType<DraftPoint>().ToArray();
            //if (points.Length == 0) return;

            //ArrayDialog ad = new ArrayDialog();
            //if (ad.ShowDialog() != DialogResult.OK) return;

            //Backup();
            //var maxx = points.Max(z => z.X);
            //var maxy = points.Max(z => z.Y);
            //var minx = points.Min(z => z.X);
            //var miny = points.Min(z => z.Y);
            //var width = maxx - minx;
            //var height = maxy - miny;

            //var lines = _draft.DraftLines.Where(z => points.Contains(z.V0) && points.Contains(z.V1)).ToArray();
            //var circles = _draft.DraftEllipses.Where(z => points.Contains(z.Center)).ToArray();

            //for (int i = 0; i < ad.QtyX; i++)
            //{
            //    for (int j = 0; j < ad.QtyY; j++)
            //    {
            //        if (i == 0 && j == 0) continue;
            //        var shx = i * (ad.OffsetX + width);
            //        var shy = j * (ad.OffsetY + height);
            //        List<DraftPoint> added = new List<DraftPoint>();
            //        foreach (var item in points)
            //        {
            //            var dp = new DraftPoint(_draft, item.X + shx, item.Y + shy);
            //            _draft.AddElement(dp);
            //            added.Add(dp);
            //        }
            //        foreach (var item in lines)
            //        {
            //            var v0 = added.First(z => (z.Location - (item.V0.Location + new Vector2d(shx, shy))).Length < float.Epsilon);
            //            var v1 = added.First(z => (z.Location - (item.V1.Location + new Vector2d(shx, shy))).Length < float.Epsilon);
            //            _draft.AddElement(new DraftLine(v0, v1, _draft));
            //        }
            //        foreach (var item in circles)
            //        {
            //            var v0 = added.First(z => (z.Location - (item.Center.Location + new Vector2d(shx, shy))).Length < float.Epsilon);
            //            _draft.AddElement(new DraftEllipse(v0, item.Radius, _draft));
            //        }
            //    }
            //}
        }

        public long LastRenderTime;

        public event Action UndosChanged;
        private void Ctx_MouseDown(float arg1, float arg2, MouseButtons e)
        {
            //var pos = ctx.PictureBox.Control.PointToClient(Cursor.Position);

            if (e == MouseButtons.Left)
            {
                if ((Control.ModifierKeys & Keys.Shift) != 0)
                {

                }
                else
                if ((Control.ModifierKeys & Keys.Control) != 0)
                {

                }
                else
                {
                    foreach (var item in selected)
                    {
                        if (item is IDrawable dd)
                        {
                            dd.Selected = false;
                        }
                    }
                    selected = new[] { nearest };
                }
                //Form1.Form.SetStatus($"selected: {selected.Count()} objects");
                foreach (var item in selected)
                {
                    if (item is IDrawable dd)
                    {
                        dd.Selected = true;
                    }
                }
            }

            if (e == MouseButtons.Middle)
            {
                ctx.isMiddleDrag = true;
                if (nearest is DraftPoint pp)
                {
                    startMiddleDragNearest = nearest;
                    var tr = ctx.Transform(pp.Location);
                    ctx.startx = (float)tr.X;
                    ctx.starty = (float)tr.Y;
                }
            }
            if (e == MouseButtons.Left && editor.CurrentTool is SelectionTool)
            {
                ctx.isLeftDrag = true;
                if (nearest is DraftPoint pp)
                {
                    var tr = ctx.Transform(pp.Location);
                    ctx.startx = (float)tr.X;
                    ctx.starty = (float)tr.Y;
                }
            }
        }


        List<DraftElement> queue = new List<DraftElement>();
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (editor.CurrentTool is SelectionTool && e.Button == MouseButtons.Left)
            {
                //if (nearest is LinearConstraintHelper lh)
                {
                    //   editor.ObjectSelect(lh.constraint);
                }
                //  else
                {
                    editor.ObjectSelect(nearest);
                }
            }
            if (editor.CurrentTool is PerpendicularConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftLine)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftLine);
                }
                if (queue.Count > 1)
                {
                    var cc = new PerpendicularConstraint(queue[0] as DraftLine, queue[1] as DraftLine, _draft);
                    if (!_draft.Constraints.OfType<PerpendicularConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new PerpendicularConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();
                }
            }
            if (editor.CurrentTool is ParallelConstraintTool && e.Button == MouseButtons.Left)
            {
                if (nearest is DraftLine)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftLine);
                }
                if (queue.Count > 1)
                {
                    var cc = new ParallelConstraint(queue[0] as DraftLine, queue[1] as DraftLine, _draft);

                    if (!_draft.Constraints.OfType<ParallelConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new ParallelConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist", ParentForm.Text);
                    }
                    queue.Clear();
                    editor.ResetTool();

                }
            }

            editor.CurrentTool.MouseDown(e);
        }


        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //isPressed = false;
            var cp = Cursor.Position;
            var pos = ctx.PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                var dx = Math.Abs(ctx.startx - pos.X);
                var dy = Math.Abs(ctx.starty - pos.Y);
                float eps = 1;
                if ((dx + dy) < eps)
                {
                    //contextMenuStrip1.Show(cp);
                }
            }
            if (e.Button == MouseButtons.Left)
            {

                var gcur = ctx.GetCursor();
                var t = ctx.BackTransform(new PointF(ctx.startx, ctx.starty));
                var rxm = Math.Min(t.X, gcur.X);
                var rym = Math.Min(t.Y, gcur.Y);
                var rdx = Math.Abs(t.X - gcur.X);
                var rdy = Math.Abs(t.Y - gcur.Y);
                var rect = new RectangleF(rxm, rym, rdx, rdy);
                if (rect.Width > 1 && rect.Height > 1)
                {
                    var tt = _draft.DraftPoints.Where(z => rect.Contains((float)z.Location.X, (float)z.Location.Y)).ToArray();
                    tt = tt.Union(_draft.DraftEllipses.Select(z => z.Center).Where(z => rect.Contains((float)z.Location.X, (float)z.Location.Y))).ToArray();
                    if ((Control.ModifierKeys & Keys.Shift) != 0)
                    {
                        selected = selected.Except(tt).ToArray();
                    }
                    else
                    if ((Control.ModifierKeys & Keys.Control) != 0)
                    {
                        selected = selected.Union(tt).ToArray();
                    }
                    else
                        selected = tt;
                    //Form1.Form.SetStatus($"selected: {tt.Count()} points");
                }
                else
                {
                    if ((Control.ModifierKeys & Keys.Control) != 0)
                    {
                        if (selected.Length == 1)
                        {
                            if (selected[0] is DraftLine dl)
                            {
                                List<DraftLine> contour = new List<DraftLine>();
                                contour.Add(dl);

                                //contour select
                                double eps = 1e-8;
                                var remains = Draft.DraftLines.Except(new[] { dl }).ToList();
                                while (true)
                                {
                                    DraftLine add = null;
                                    foreach (var line in remains)
                                    {
                                        var v1 = new[] { line.V0, line.V1 };
                                        if ((contour[0].V0.Location - v1[0].Location).Length < eps
                                            || (contour[0].V0.Location - v1[1].Location).Length < eps
                                             || (contour[0].V1.Location - v1[0].Location).Length < eps
                                              || (contour[0].V1.Location - v1[1].Location).Length < eps
                                            )
                                        {
                                            add = line;
                                            contour.Insert(0, line);
                                            break;
                                        }
                                    }

                                    if (add == null) break;
                                    remains.Remove(add);
                                }
                                //check closed
                                //select all
                                selected = contour.SelectMany(z => new[] { z.V0, z.V1 }).Distinct().OfType<object>().Union(contour.ToArray()).ToArray();
                            }
                        }
                    }
                }
            }
        }

        public List<string> Undos = new List<string>();
        public void Undo()
        {
            if (Undos.Count == 0) return;
            var el = XElement.Parse(Undos.Last());
            _draft.Restore(el);
            Undos.RemoveAt(Undos.Count - 1);
            SetDraft(_draft);
            UndosChanged?.Invoke();

        }

        public void Backup()
        {
            StringWriter sw = new StringWriter();
            _draft.Store(sw);
            Undos.Add(sw.ToString());
            UndosChanged?.Invoke();
        }

        public void FitAll()
        {
            if (_draft == null || _draft.Elements.Count() == 0) return;

            var t = _draft.DraftPoints.Select(z => z.Location).ToArray();
            var t2 = _draft.DraftEllipses.SelectMany(z => new[] {
                new Vector2d(z.Center.X - (double)z.Radius, z.Center.Y-(double)z.Radius) ,
                new Vector2d(z.Center.X + (double)z.Radius, z.Center.Y+(double)z.Radius) ,

            }).ToArray();
            t = t.Union(t2).ToArray();

            ctx.FitToPoints(t.Select(z => z.ToPointF()).ToArray(), 5);
        }
        public static Type DrawerType = typeof(SkiaGLDrawingContext);
        IDrawingContext ctx;
        public object nearest { get; private set; }
        public object startMiddleDragNearest;
        object[] selected = new object[] { };
        void updateNearest()
        {
            var pos = ctx.GetCursor();
            var _pos = new Vector2d(pos.X, pos.Y);
            double minl = double.MaxValue;
            object minp = null;
            double maxDist = 10 / ctx.zoom;
            foreach (var item in _draft.DraftPoints)
            {
                var d = (item.Location - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            foreach (var item in _draft.DraftEllipses)
            {
                var d = (item.Center.Location - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item.Center;
                }

                d = Math.Abs((item.Center.Location - _pos).Length - (double)item.Radius);
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }

            }
            foreach (var item in _draft.DraftLines)
            {
                var loc = (item.V0.Location + item.V1.Location) / 2;
                var d = (loc - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            foreach (var item in _draft.ConstraintHelpers)
            {
                var d = (item.SnapPoint - _pos).Length;
                if (d < minl)
                {
                    minl = d;
                    minp = item;
                }
            }
            if (minl < maxDist)
            {
                nearest = minp;
            }
            else
                nearest = null;
        }

        public enum SubSnapTypeEnum
        {
            None, Point, PointOnLine, CenterLine, Perpendicular, PointOnCircle
        }

        SubSnapTypeEnum subSnapType;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Visible) return;
            RenderControl.Refresh();
            return;

        }

        public void ResetTool()
        {
            editor.ResetTool();
        }
        IEditor editor;

        Draft _draft;
        public Draft Draft => _draft;

        public IDrawingContext DrawingContext => ctx;

        public bool CanUndo => Undos.Any();

        public bool ShowHelpers { get; set; } = true;

        public void SetDraft(Draft draft)
        {
            _draft = draft;
            _draft.BeforeConstraintChanged = (c) =>
            {
                Backup();
            };

            //restore helpers
            foreach (var citem in draft.Constraints)
            {
                if (draft.ConstraintHelpers.Any(z => z.Constraint == citem)) continue;
                if (citem is LinearConstraint lc)
                {
                    draft.AddHelper(new LinearConstraintHelper(_draft, lc));
                }
                if (citem is VerticalConstraint vc)
                {
                    _draft.AddHelper(new VerticalConstraintHelper(vc));
                }
                if (citem is HorizontalConstraint hc)
                {
                    _draft.AddHelper(new HorizontalConstraintHelper(hc));
                }
                if (citem is EqualsConstraint ec)
                {
                    _draft.AddHelper(new EqualsConstraintHelper(draft, ec));
                }
            }
        }
        public void Init(IEditor editor)
        {
            this.editor = editor;
            editor.ToolChanged += Editor_ToolChanged;
        }

        private void Editor_ToolChanged(ITool obj)
        {
            //lastDraftPoint = null;
        }

        internal void Finish()
        {
            _draft.EndEdit();
        }

        internal void Clear()
        {
            Backup();
            _draft.Clear();
        }

        public void CloseLine()
        {
            if (_draft.DraftPoints.Any())
                _draft.Elements.Add(new DraftLine(_draft.DraftPoints.First(), _draft.DraftPoints.Last(), _draft));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                deleteToolStripMenuItem_Click(null, null);

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected == null || selected.Length == 0) return;
            Backup();
            foreach (var item in selected)
            {
                if (item is DraftElement de)
                    _draft.RemoveElement(de);

                if (item is IDrawable dd)
                {
                    _draft.RemoveChild(dd);
                }
            }
        }

        private void detectCOMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var sx = points.Sum(z => z.X) / points.Length;
            var sy = points.Sum(z => z.Y) / points.Length;

            _draft.AddElement(new DraftPoint(_draft, sx, sy));
        }

        private void approxByCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var sx = points.Sum(z => z.X) / points.Length;
            var sy = points.Sum(z => z.Y) / points.Length;
            var dp = new DraftPoint(_draft, sx, sy);
            var rad = (decimal)(points.Select(z => (z.Location - dp.Location).Length).Average());

            _draft.AddElement(new DraftEllipse(dp, rad, _draft));
            if (GUIHelpers.ShowQuestion("Delete source points?", ParentForm.Text) == DialogResult.Yes)
            {
                for (int p = 0; p < points.Length; p++)
                {
                    _draft.RemoveElement(points[p]);
                }
            }

        }

        internal void FlipHorizontal()
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var maxx = points.Max(z => z.X);
            var minx = points.Min(z => z.X);
            var mx = (maxx + minx) / 2;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].SetLocation(2 * mx - points[i].Location.X, points[i].Y);
            }
        }

        internal void FlipVertical()
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;
            var maxy = points.Max(z => z.Y);
            var miny = points.Min(z => z.Y);
            var my = (maxy + miny) / 2;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].SetLocation(points[i].X, 2 * my - points[i].Location.Y);
            }
        }

        public void TranslateUI()
        {
            /*var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;

            var ret = GUIHelpers.EditorStart(Vector3d.Zero, "Translate", typeof(Vector2dPropEditor));
            var r = (Vector2d)ret;

            Backup();
            foreach (var item in points)
            {
                item.SetLocation(item.Location + r);
            }*/
        }
        private void translateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateUI();
        }

        public void Swap<T>(List<T> ar, int i, int j)
        {
            var temp = ar[i];
            ar[i] = ar[j];
            ar[j] = temp;
        }
        public DraftPoint[] ExtractStripContour(DraftLine[] lines)
        {
            List<DraftPoint> ret = new List<DraftPoint>();
            ret = lines.SelectMany(z => new[] { z.V0, z.V1 }).ToList();
            //reverse pairs inplace
            for (int i = 0; i < ret.Count - 2; i += 2)
            {
                var cur1 = ret[i];
                var cur2 = ret[i + 1];
                var next1 = ret[i + 2];
                var next2 = ret[i + 3];
                //find connection point
                var dist0 = (cur2.Location - next1.Location).Length;
                var dist1 = (cur1.Location - next2.Location).Length;
                var dist2 = (cur2.Location - next2.Location).Length;
                var dist3 = (cur1.Location - next1.Location).Length;
                var minIndex = new[] { dist0, dist1, dist2, dist3 }.Select((z, ii) => new Tuple<double, int>(z, ii)).OrderBy(z => z.Item1).First().Item2;
                bool reverse1 = false;
                bool reverse2 = false;
                switch (minIndex)
                {
                    case 0:

                        break;
                    case 1:
                        //reverse both
                        reverse1 = true;
                        reverse2 = true;
                        break;
                    case 2:

                        reverse2 = true;
                        break;
                    case 3:
                        reverse1 = true;
                        break;
                }
                if (reverse1)
                {
                    Swap(ret, i, i + 1);
                }
                if (reverse2)
                {
                    Swap(ret, i + 2, i + 3);
                }
            }

            /*for (int i = 1; i < ret.Count - 2; i += 2)
            {
                var cur = ret[i];
                var forw1 = ret[i + 1];
                var forw2 = ret[i + 2];
                var dist0 = (cur.Location - forw1.Location).Length;
                var dist1 = (cur.Location - forw2.Location).Length;
                if (dist0 < dist1)
                {

                }
                else
                {
                    //swap
                    var temp = ret[i + 1];
                    ret[i + 1] = ret[i + 2];
                    ret[i + 2] = temp;
                }
            }*/
            return ret.Distinct().ToArray();
        }

        public void OffsetUI()
        {
            //OffsetDialog od = new OffsetDialog();
            //if (od.ShowDialog() != DialogResult.OK) return;

            //Backup();

            //NFP p = new NFP();
            //NFP ph2 = new NFP();
            ////restore contours
            //var lines = selected.OfType<DraftLine>().ToArray();

            ////single contour support only yet

            //var l = Draft.DraftLines.Where(z => selected.Contains(z.V0) && selected.Contains(z.V1)).OfType<DraftElement>().ToArray();
            //var l2 = Draft.DraftEllipses.Where(z => selected.Contains(z.Center)).ToArray();

            ////restore contours
            ///*    l = l.Union(Draft.DraftEllipses.Where(z => selected.Contains(z.Center)).ToArray()).ToArray();
            //    foreach (var item in l)
            //    {
            //        item.Dummy = true;
            //    }*/

            ////p.Points = ph2.Polygon.Points.Select(z => new Vector2d(z.X, z.Y)).ToArray();
            //var strip = ExtractStripContour(lines);
            //p.Points = strip.Select(z => z.Location).ToArray();
            //var jType = od.JoinType;
            //double offset = od.Offset;
            //double miterLimit = 4;
            //double curveTolerance = od.CurveTolerance;
            //var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
            ////if (offs.Count() > 1) throw new NotImplementedException();
            //NFP ph = new NFP();
            //foreach (var item in ph2.Childrens)
            //{
            //    var offs2 = ClipperHelper.offset(item, -offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
            //    var nfp1 = new NFP();
            //    if (offs2.Any())
            //    {
            //        //if (offs2.Count() > 1) throw new NotImplementedException();
            //        foreach (var zitem in offs2)
            //        {
            //            nfp1.Points = zitem.Points.Select(z => new Vector2d(z.X, z.Y)).ToArray();
            //            ph.Childrens.Add(nfp1);
            //        }
            //    }
            //}

            //if (offs.Any())
            //{
            //    ph.Points = offs.First().Points.Select(z => new Vector2d(z.X, z.Y)).ToArray();
            //}

            //foreach (var item in offs.Skip(1))
            //{
            //    var nfp2 = new NFP();

            //    nfp2.Points = item.Points.Select(z => new Vector2d(z.X, z.Y)).ToArray();
            //    ph.Childrens.Add(nfp2);

            //}

            //List<DraftPoint> newp = new List<DraftPoint>();
            //for (int i = 0; i < ph.Points.Length; i++)
            //{
            //    newp.Add(new DraftPoint(Draft, ph.Points[i].X, ph.Points[i].Y));

            //}
            //Draft.Elements.AddRange(newp);
            //for (int i = 1; i <= ph.Points.Length; i++)
            //{
            //    Draft.AddElement(new DraftLine(newp[i - 1], newp[i % ph.Points.Length], Draft));
            //}

            ///*ph.OffsetX = ph2.OffsetX;
            //ph.OffsetY = ph2.OffsetY;
            //ph.Rotation = ph2.Rotation;
            //dataModel.AddItem(ph);*/
        }

        private void offsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OffsetUI();
        }

        private void dummyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = Draft.DraftLines.Where(z => selected.Contains(z.V0) && selected.Contains(z.V1)).OfType<DraftElement>().ToArray();
            l = l.Union(Draft.DraftEllipses.Where(z => selected.Contains(z.Center)).ToArray()).ToArray();
            if (l.Any()) Backup();
            foreach (var item in l)
            {
                item.Dummy = true;
            }
        }

        private void undummyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = Draft.DraftLines.Where(z => selected.Contains(z.V0) && selected.Contains(z.V1)).OfType<DraftElement>().ToArray();
            l = l.Union(Draft.DraftEllipses.Where(z => selected.Contains(z.Center)).ToArray()).ToArray();
            if (l.Any()) Backup();
            foreach (var item in l)
            {
                item.Dummy = false;
            }
        }

        private void mergePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var points = selected.OfType<DraftPoint>().ToArray();
            if (points.Length == 0) return;


            var sx = points.Sum(z => z.X) / points.Length;
            var sy = points.Sum(z => z.Y) / points.Length;
            Backup();
            _draft.AddElement(new DraftPoint(_draft, sx, sy));

            var l = Draft.DraftLines.Where(z => selected.Contains(z.V0) && selected.Contains(z.V1)).OfType<DraftElement>().ToArray();


            for (int i = 0; i < points.Length; i++)
            {
                _draft.RemoveElement(points[i]);
            }
            //todo: add lines
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (!Visible) return;
            RenderControl.Refresh();
            return;
        }
    }

    public interface IDraftConstraintHelper : IDraftHelper
    {
        DraftConstraint Constraint { get; }
        Vector2d SnapPoint { get; set; }


    }
    public class DraftLine : DraftElement
    {
        public readonly DraftPoint V0;
        public readonly DraftPoint V1;

        public DraftLine(XElement el, Draft parent) : base(el, parent)
        {
            var v0Id = int.Parse(el.Attribute("v0").Value);
            var v1Id = int.Parse(el.Attribute("v1").Value);
            Dummy = bool.Parse(el.Attribute("dummy").Value);
            V0 = parent.DraftPoints.First(z => z.Id == v0Id);
            V1 = parent.DraftPoints.First(z => z.Id == v1Id);
        }

        public DraftLine(DraftPoint v0, DraftPoint v1, Draft parent) : base(parent)
        {
            this.V0 = v0;
            this.V1 = v1;
        }

        public Vector2d Center => (V0.Location + V1.Location) / 2;
        public Vector2d Dir => (V1.Location - V0.Location).Normalized();
        public Vector2d Normal => new Vector2d(-Dir.Y, Dir.X);
        public double Length => (V1.Location - V0.Location).Length;
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<line id=\"{Id}\" dummy=\"{Dummy}\" v0=\"{V0.Id}\" v1=\"{V1.Id}\" />");
        }

        public bool ContainsPoint(Vector2d proj)
        {
            return Math.Abs(((V0.Location - proj).Length + (V1.Location - proj).Length) - Length) < 1e-8f;
        }
    }
    public static class FactoryHelper
    {

        public static int NewId;
    }
    public class ConstraintSolverContext
    {
        public ConstraintSolverContext Parent;
        public List<ConstraintSolverContext> Childs = new List<ConstraintSolverContext>();
        public List<DraftPoint> FreezedPoints = new List<DraftPoint>();
        public List<TopologyDraftLineInfo> FreezedLinesDirs = new List<TopologyDraftLineInfo>();
    }

    public class DraftEllipse : DraftElement
    {
        public readonly DraftPoint Center;
        public double X { get => Center.Location.X; set => Center.SetLocation(new OpenTK.Vector2d(value, Center.Y)); }
        public double Y { get => Center.Location.Y; set => Center.SetLocation(new OpenTK.Vector2d(Center.X, value)); }
        decimal _radius { get; set; }
        public decimal Radius { get => _radius; set => _radius = value; }
        public decimal Diameter { get => 2 * _radius; set => _radius = value / 2; }
        public bool SpecificAngles { get; set; }
        public int Angles { get; set; }
        public DraftEllipse(DraftPoint center, decimal radius, Draft parent)
            : base(parent)
        {
            this.Center = center;
            this.Radius = radius;
        }
        public DraftEllipse(XElement elem, Draft parent)
          : base(elem, parent)
        {
            var c = Helpers.ParseVector2(elem.Attribute("center").Value);
            Center = new DraftPoint(parent, c.X, c.Y);
            Radius = Helpers.ParseDecimal(elem.Attribute("radius").Value);
            if (elem.Attribute("angles") != null)
                Angles = int.Parse(elem.Attribute("angles").Value);
            if (elem.Attribute("specificAngles") != null)
                SpecificAngles = bool.Parse(elem.Attribute("specificAngles").Value);
        }

        internal decimal CutLength()
        {
            return (2 * (decimal)Math.PI * Radius);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<ellipse id=\"{Id}\" angles=\"{Angles}\" specificAngles=\"{SpecificAngles}\" center=\"{Center.X}; {Center.Y}\" radius=\"{Radius}\">");
            writer.WriteLine("</ellipse>");
        }

        public Vector2d[] GetPoints()
        {
            var step = 360f / Angles;
            List<Vector2d> pp = new List<Vector2d>();
            for (int i = 0; i < Angles; i++)
            {
                var ang = step * i;
                var radd = ang * Math.PI / 180f;
                var xx = Center.X + (double)Radius * Math.Cos(radd);
                var yy = Center.Y + (double)Radius * Math.Sin(radd);
                pp.Add(new Vector2d(xx, yy));
            }
            return pp.ToArray();
        }
    }
    public class TopologyDraftLineInfo
    {
        public DraftLine Line;
        public Vector2d Dir;
    }
    public class Line3D
    {
        public Vector3d Start;
        public Vector3d End;
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public bool IsPointOnLine(Vector3d pnt, float epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - Start;
            if (d1.Length < tolerance) return true;
            if ((End - Start).Length < tolerance) throw new Exception("degenerated 3d line");
            var crs = Vector3d.Cross(d1.Normalized(), (End - Start).Normalized());
            return Math.Abs(crs.Length) < epsilon;
        }
        public bool IsPointInsideSegment(Vector3d pnt, float epsilon = 10e-6f)
        {
            if (!IsPointOnLine(pnt, epsilon)) return false;
            var v0 = (End - Start).Normalized();
            var v1 = pnt - Start;
            var crs = Vector3d.Dot(v0, v1) / (End - Start).Length;
            return !(crs < 0 || crs > 1);
        }
        public bool IsSameLine(Line3D l)
        {
            return IsPointOnLine(l.Start) && IsPointOnLine(l.End);
        }

        public void Shift(Vector3d vector3)
        {
            Start += vector3;
            End += vector3;
        }
    }
    public interface ITool
    {

        void Update();
        void MouseDown(MouseEventArgs e);
        void MouseUp(MouseEventArgs e);
        void Draw();
        void Select();
        void Deselect();
    }
    public interface IDrawable
    {
        int Id { get; set; }
        IDrawable Parent { get; set; }
        List<IDrawable> Childs { get; }
        string Name { get; set; }
        bool Visible { get; set; }
        bool Frozen { get; set; }
        void Draw();
        bool Selected { get; set; }
        TransformationChain Matrix { get; }

        IDrawable[] GetAll(Predicate<IDrawable> p);
        void RemoveChild(IDrawable dd);
        void Store(TextWriter writer);
        int Z { get; set; }
    }
    public class PlaneHelper : AbstractDrawable, IEditFieldsContainer, ICommandsContainer
    {
        public PlaneHelper()
        {

        }

        public PlaneHelper(XElement elem)
        {
            if (elem.Attribute("name") != null)
            {
                Name = elem.Attribute("name").Value;
            }
            if (elem.Attribute("size") != null)
            {
                DrawSize = int.Parse(elem.Attribute("size").Value);
            }
            var pos = elem.Attribute("pos").Value.Split(';').Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3d(pos[0], pos[1], pos[2]);
            var normal = elem.Attribute("normal").Value.Split(';').Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Normal = new Vector3d(normal[0], normal[1], normal[2]);
        }

        public Plane GetPlane()
        {
            return new Plane() { Normal = Normal, Position = Position };
        }

        [EditField]
        public Vector3d Position { get; set; }

        [EditField]
        public Vector3d Normal { get; set; }

        [EditField]
        public int DrawSize { get; set; } = 10;

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<plane name=\"{Name}\" size=\"{DrawSize}\" pos=\"{Position.X};{Position.Y};{Position.Z}\" normal=\"{Normal.X};{Normal.Y};{Normal.Z}\"/>");
        }

        public Vector3d[] GetBasis()
        {
            Vector3d[] shifts = new[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Vector3d axis1 = Vector3d.Zero;
            for (int i = 0; i < shifts.Length; i++)
            {
                var proj = ProjPoint(Position + shifts[i]);

                if (Vector3d.Distance(proj, Position) > 10e-6)
                {
                    axis1 = (proj - Position).Normalized();
                    break;
                }
            }
            var axis2 = Vector3d.Cross(Normal.Normalized(), axis1);

            return new[] { axis1, axis2 };
        }
        public Vector2d ProjectPointUV(Vector3d v)
        {
            var basis = GetBasis();
            return GetUVProjPoint(v, basis[0], basis[1]);
        }
        public Vector2d GetUVProjPoint(Vector3d point, Vector3d axis1, Vector3d axis2)
        {
            var p = GetProjPoint(point) - Position;
            var p1 = Vector3d.Dot(p, axis1);
            var p2 = Vector3d.Dot(p, axis2);
            return new Vector2d(p1, p2);
        }
        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Position;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }
        public Vector3d ProjPoint(Vector3d point)
        {
            var nrm = Normal.Normalized();
            var v = point - Position;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }

        public Line3D Intersect(PlaneHelper ps)
        {
            Line3D ret = new Line3D();

            var dir = Vector3d.Cross(ps.Normal, Normal);


            var k1 = ps.GetKoefs();
            var k2 = GetKoefs();
            var a1 = k1[0];
            var b1 = k1[1];
            var c1 = k1[2];
            var d1 = k1[3];

            var a2 = k2[0];
            var b2 = k2[1];
            var c2 = k2[2];
            var d2 = k2[3];



            var res1 = det2(new[] { a1, a2 }, new[] { b1, b2 }, new[] { -d1, -d2 });
            var res2 = det2(new[] { a1, a2 }, new[] { c1, c2 }, new[] { -d1, -d2 });
            var res3 = det2(new[] { b1, b2 }, new[] { c1, c2 }, new[] { -d1, -d2 });

            List<Vector3d> vvv = new List<Vector3d>();

            if (res1 != null)
            {
                Vector3d v1 = new Vector3d((float)res1[0], (float)res1[1], 0);
                vvv.Add(v1);

            }

            if (res2 != null)
            {
                Vector3d v1 = new Vector3d((float)res2[0], 0, (float)res2[1]);
                vvv.Add(v1);
            }
            if (res3 != null)
            {
                Vector3d v1 = new Vector3d(0, (float)res3[0], (float)res3[1]);
                vvv.Add(v1);
            }

            var pnt = vvv.OrderBy(z => z.Length).First();


            var r1 = IsOnPlane(pnt);
            var r2 = IsOnPlane(pnt);

            ret.Start = pnt;
            ret.End = pnt + dir * 100;
            return ret;
        }
        public bool IsOnPlane(Vector3d orig, Vector3d normal, Vector3d check, double tolerance = 10e-6)
        {
            return (Math.Abs(Vector3d.Dot(orig - check, normal)) < tolerance);
        }
        public bool IsOnPlane(Vector3d v)
        {
            return IsOnPlane(Position, Normal, v);
        }
        double[] det2(double[] a, double[] b, double[] c)
        {
            var d = a[0] * b[1] - a[1] * b[0];
            if (d == 0) return null;
            var d1 = c[0] * b[1] - c[1] * b[0];
            var d2 = a[0] * c[1] - a[1] * c[0];
            var x = d1 / d;
            var y = d2 / d;
            return new[] { x, y };
        }

        public bool Fill { get; set; }

        public static List<ICommand> Commands = new List<ICommand>();
        ICommand[] ICommandsContainer.Commands => Commands.ToArray();

        public double[] GetKoefs()
        {
            double[] ret = new double[4];
            ret[0] = Normal.X;
            ret[1] = Normal.Y;
            ret[2] = Normal.Z;
            ret[3] = -(ret[0] * Position.X + Position.Y * ret[1] + Position.Z * ret[2]);

            return ret;
        }

        public override void Draw()
        {
            if (!Visible) return;

        }

        public IName[] GetObjects()
        {
            List<IName> ret = new List<IName>();
            var fld = GetType().GetProperties();
            for (int i = 0; i < fld.Length; i++)
            {

                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    if (fld[i].PropertyType == typeof(Vector3d))
                    {
                        //ret.Add(new VectorEditor(fld[i]) { Object = this });
                    }
                    if (fld[i].PropertyType == typeof(int))
                    {
                        ret.Add(new IntFieldEditor(fld[i]) { Object = this });
                    }
                }
            }
            return ret.ToArray();
        }
    }

    public class LiteCadException : Exception
    {
        public LiteCadException(string str) : base(str) { }
    }

    public class ConstraintsException : Exception
    {
        public ConstraintsException(string msg) : base(msg) { }
    }
    public class XmlNameAttribute : Attribute
    {
        public string XmlName { get; set; }
    }

    public class ChangeCand
    {
        public DraftPoint Point;
        public Vector2d Position;
        public void Apply()
        {
            Point.SetLocation(Position);
        }

    }
    public interface IPropEditor
    {
        void Init(object o);
        object ReturnValue { get; }
    }
    public class VertexInfo
    {
        public Vector3d Position;
        public Vector3d Normal;
    }
    public class ParallelConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly ParallelConstraint constraint;
        public ParallelConstraintHelper(ParallelConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent => throw new System.NotImplementedException();

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Element1.Center;
            var dp1 = constraint.Element2.Center;
            var tr0 = ctx.Transform(dp0);
            var tr1 = ctx.Transform(dp1);
            var text = ctx.Transform((dp0 + dp1) / 2);

            ctx.DrawString("||", SystemFonts.DefaultFont, Brushes.Black, text);
            SnapPoint = (dp0 + dp1) / 2;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            Pen p = new Pen(Color.Red, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;


            //create bezier here
            ctx.DrawPolygon(p, new PointF[] { tr0, tr1 });
        }



        public override void Draw()
        {

        }
    }
    public abstract class AbstractDrawable : IDrawable
    {
        public static IMessageReporter MessageReporter;
        public AbstractDrawable()
        {
            Id = FactoryHelper.NewId++;
        }
        public bool Frozen { get; set; }

        public AbstractDrawable(XElement item)
        {
            if (item.Attribute("id") != null)
            {
                Id = int.Parse(item.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
        }
        public string Name { get; set; }

        public abstract void Draw();

        public virtual void RemoveChild(IDrawable dd)
        {
            Childs.Remove(dd);
        }

        public virtual void Store(TextWriter writer)
        {

        }

        public virtual IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            if (Childs.Count == 0)
                return new[] { this };
            return new[] { this }.Union(Childs.SelectMany(z => z.GetAll(p))).ToArray();
        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }
        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }
        public int Z { get; set; }
    }
    public class TransformationChain
    {
        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<transformationChain>");
            foreach (var item in Items)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</transformationChain>");
        }

        public void RestoreXml(XElement xElement)
        {
            Items.Clear();
            Type[] types = new[] {
                typeof(ScaleTransformChainItem),
                typeof(TranslateTransformChainItem),
                typeof(RotationTransformChainItem)
            };
            foreach (var item in xElement.Element("transformationChain").Elements())
            {
                var fr = types.First(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                var v = Activator.CreateInstance(fr) as TransformationChainItem;
                v.RestoreXml(item);
                Items.Add(v);
            }
        }

        public List<TransformationChainItem> Items = new List<TransformationChainItem>();
        public Matrix4d Calc()
        {
            var r = Matrix4d.Identity;
            foreach (var item in Items)
            {
                r *= item.Matrix();
            }
            return r;
        }

        public TransformationChain Clone()
        {
            TransformationChain ret = new TransformationChain();
            foreach (var item in Items)
            {
                ret.Items.Add(item.Clone());
            }
            return ret;
        }
    }
    public abstract class TransformationChainItem : IXmlStorable
    {
        public abstract Matrix4d Matrix();

        void IXmlStorable.RestoreXml(XElement elem)
        {
            RestoreXml(elem);
        }

        internal abstract void StoreXml(TextWriter writer);
        internal abstract void RestoreXml(XElement elem);

        void IXmlStorable.StoreXml(TextWriter writer)
        {
            StoreXml(writer);
        }

        internal abstract TransformationChainItem Clone();
    }
    public interface IXmlStorable
    {
        void StoreXml(TextWriter writer);
        void RestoreXml(XElement elem);

    }
    public class ClipperHelper
    {
        public static NFP clipperToSvg(IList<IntPoint> polygon, double clipperScale = 10000000)
        {
            List<Vector2d> ret = new List<Vector2d>();

            for (var i = 0; i < polygon.Count; i++)
            {
                ret.Add(new Vector2d(polygon[i].X / clipperScale, polygon[i].Y / clipperScale));
            }

            return new NFP() { Points = ret.ToArray() };
        }

        public static IntPoint[] ScaleUpPaths(NFP p, double scale = 10000000)
        {
            List<IntPoint> ret = new List<IntPoint>();

            for (int i = 0; i < p.Points.Count(); i++)
            {
                ret.Add(new ClipperLib.IntPoint(
                    (long)Math.Round((decimal)p.Points[i].X * (decimal)scale),
                    (long)Math.Round((decimal)p.Points[i].Y * (decimal)scale)
                ));

            }
            return ret.ToArray();
        }

        public static NFP[] offset(NFP polygon, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ScaleUpPaths(polygon, clipperScale).ToList();

            var co = new ClipperLib.ClipperOffset(miterLimit, curveTolerance * clipperScale);
            co.AddPath(p.ToList(), jType, ClipperLib.EndType.etClosedPolygon);

            var newpaths = new List<List<ClipperLib.IntPoint>>();
            co.Execute(ref newpaths, offset * clipperScale);

            var result = new List<NFP>();
            for (var i = 0; i < newpaths.Count; i++)
            {
                result.Add(clipperToSvg(newpaths[i]));
            }
            return result.ToArray();
        }
        public static IntPoint[][] nfpToClipperCoordinates(NFP nfp, double clipperScale = 10000000)
        {

            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

            // children first
            if (nfp.Childrens != null && nfp.Childrens.Count > 0)
            {
                for (var j = 0; j < nfp.Childrens.Count; j++)
                {
                    if (GeometryUtils.polygonArea(nfp.Childrens[j]) < 0)
                    {
                        nfp.Childrens[j].Reverse();
                    }
                    //var childNfp = SvgNest.toClipperCoordinates(nfp.children[j]);
                    var childNfp = ScaleUpPaths(nfp.Childrens[j], clipperScale);
                    clipperNfp.Add(childNfp);
                }
            }

            if (GeometryUtils.polygonArea(nfp) > 0)
            {
                nfp.Reverse();
            }


            //var outerNfp = SvgNest.toClipperCoordinates(nfp);

            // clipper js defines holes based on orientation

            var outerNfp = ScaleUpPaths(nfp, clipperScale);

            //var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);

            clipperNfp.Add(outerNfp);
            //var area = Math.abs(ClipperLib.Clipper.Area(cleaned));

            return clipperNfp.ToArray();
        }
        public static IntPoint[][] ToClipperCoordinates(NFP[] nfp, double clipperScale = 10000000)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                var clip = nfpToClipperCoordinates(nfp[i], clipperScale);
                clipperNfp.AddRange(clip);
            }

            return clipperNfp.ToArray();
        }
        public static NFP toNestCoordinates(IntPoint[] polygon, double scale)
        {
            var clone = new List<Vector2d>();

            for (var i = 0; i < polygon.Count(); i++)
            {
                clone.Add(new Vector2d(
                     polygon[i].X / scale,
                             polygon[i].Y / scale
                        ));
            }
            return new NFP() { Points = clone.ToArray() };
        }
        public static NFP[] intersection(NFP polygon, NFP polygon1, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ToClipperCoordinates(new[] { polygon }, clipperScale).ToList();
            var p1 = ToClipperCoordinates(new[] { polygon1 }, clipperScale).ToList();

            Clipper clipper = new Clipper();
            clipper.AddPaths(p.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
            clipper.AddPaths(p1.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

            List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
            if (clipper.Execute(ClipType.ctIntersection, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero) && finalNfp != null && finalNfp.Count > 0)
            {
                return finalNfp.Select(z => toNestCoordinates(z.ToArray(), clipperScale)).ToArray();
            }
            return null;
        }
        public static NFP MinkowskiSum(NFP pattern, NFP path, bool useChilds = false, bool takeOnlyBiggestArea = true)
        {
            var ac = ScaleUpPaths(pattern);

            List<List<IntPoint>> solution = null;
            if (useChilds)
            {
                var bc = nfpToClipperCoordinates(path);
                for (var i = 0; i < bc.Length; i++)
                {
                    for (int j = 0; j < bc[i].Length; j++)
                    {
                        bc[i][j].X *= -1;
                        bc[i][j].Y *= -1;
                    }
                }

                solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<List<IntPoint>>(bc.Select(z => z.ToList())), true);
            }
            else
            {
                var bc = ScaleUpPaths(path);
                for (var i = 0; i < bc.Length; i++)
                {
                    bc[i].X *= -1;
                    bc[i].Y *= -1;
                }
                solution = Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<IntPoint>(bc), true);
            }
            NFP clipperNfp = null;

            double? largestArea = null;
            int largestIndex = -1;

            for (int i = 0; i < solution.Count(); i++)
            {
                var n = toNestCoordinates(solution[i].ToArray(), 10000000);
                var sarea = Math.Abs(GeometryUtils.polygonArea(n));
                if (largestArea == null || largestArea < sarea)
                {
                    clipperNfp = n;
                    largestArea = sarea;
                    largestIndex = i;
                }
            }
            if (!takeOnlyBiggestArea)
            {
                for (int j = 0; j < solution.Count; j++)
                {
                    if (j == largestIndex) continue;
                    var n = toNestCoordinates(solution[j].ToArray(), 10000000);
                    if (clipperNfp.Childrens == null)
                        clipperNfp.Childrens = new List<NFP>();
                    clipperNfp.Childrens.Add(n);
                }
            }

            for (var i = 0; i < clipperNfp.Length; i++)
            {
                clipperNfp.Points[i].X *= -1;
                clipperNfp.Points[i].Y *= -1;
                clipperNfp.Points[i].X += pattern[0].X;
                clipperNfp.Points[i].Y += pattern[0].Y;
            }
            var minx = clipperNfp.Points.Min(z => z.X);
            var miny = clipperNfp.Points.Min(z => z.Y);
            var minx2 = path.Points.Min(z => z.X);
            var miny2 = path.Points.Min(z => z.Y);

            var shiftx = minx2 - minx;
            var shifty = miny2 - miny;
            if (clipperNfp.Childrens != null)
                foreach (var nFP in clipperNfp.Childrens)
                {
                    for (int j = 0; j < nFP.Length; j++)
                    {

                        nFP.Points[j].X *= -1;
                        nFP.Points[j].Y *= -1;
                        nFP.Points[j].X += pattern[0].X;
                        nFP.Points[j].Y += pattern[0].Y;
                    }
                }

            return clipperNfp;
        }

    }
    public class NFP
    {
        public Vector2d[] Points = new Vector2d[] { };
        public List<NFP> Childrens = new List<NFP>();
        public NFP Parent;
        public Vector2d this[int ind]
        {
            get
            {
                return Points[ind];
            }
        }
        public void Shift(Vector2d vector)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].X += vector.X;
                Points[i].Y += vector.Y;
            }
        }
        public double SignedArea()
        {
            return GeometryUtils.signed_area(Points);
        }

        public int Length
        {
            get
            {
                return Points.Length;
            }
        }

        public void Reverse()
        {
            Points = Points.Reverse().ToArray();
        }
    }
    public interface IMessageReporter
    {
        void Warning(string text);
        void Error(string text);
        void Info(string text);
    }
    public interface IDraftHelper : IDrawable
    {

        Draft DraftParent { get; }
        bool Enabled { get; set; }

        void Draw(IDrawingContext ctx);

    }
    [XmlName(XmlName = "equalsConstraint")]
    public class EqualsConstraint : DraftConstraint, IXmlStorable
    {
        public DraftLine SourceLine;
        public DraftLine TargetLine;
        public EqualsConstraint(DraftLine target, DraftLine source, Draft parent) : base(parent)
        {
            SourceLine = source;
            TargetLine = target;
        }

        public EqualsConstraint(XElement el, Draft parent) : base(parent)
        {
            TargetLine = parent.Elements.OfType<DraftLine>().First(z => z.Id == int.Parse(el.Attribute("targetId").Value));
            SourceLine = parent.Elements.OfType<DraftLine>().First(z => z.Id == int.Parse(el.Attribute("sourceId").Value));
        }

        public override bool IsSatisfied(float eps = 1E-06F)
        {
            return Math.Abs(TargetLine.Length - SourceLine.Length) < eps;
        }

        ChangeCand[] GetCands()
        {
            List<ChangeCand> ret = new List<ChangeCand>();
            var dir = TargetLine.Dir;
            ret.Add(new ChangeCand() { Point = TargetLine.V0, Position = TargetLine.V1.Location + SourceLine.Length * (-dir) });
            ret.Add(new ChangeCand() { Point = TargetLine.V1, Position = TargetLine.V0.Location + SourceLine.Length * dir });
            return ret.Where(z => !z.Point.Frozen).ToArray();
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var cc = GetCands();
            var ar = cc.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            var fr = ar.First();
            fr.Apply();
        }

        public bool IsSame(EqualsConstraint cc)
        {
            return cc.TargetLine == TargetLine && cc.SourceLine == SourceLine;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return TargetLine == de || TargetLine.V0 == de || TargetLine.V1 == de || SourceLine == de || SourceLine.V0 == de || SourceLine.V1 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<equalsConstraint targetId=\"{TargetLine.Id}\" sourceId=\"{SourceLine.Id}\"/>");
        }

        public void StoreXml(TextWriter writer)
        {
            Store(writer);
        }

        public void RestoreXml(XElement elem)
        {
            //   var targetId = int.Parse(elem.Attribute("targetId").Value);
            // Line = Line.Parent.Elements.OfType<DraftLine>().First(z => z.Id == targetId);
        }
    }
    public class CSPConstrEqualTwoVars : CSPConstrEqualExpression
    {
        public CSPConstrEqualTwoVars(CSPVar var1, CSPVar var2)
        {
            Var1 = var1;
            Var2 = var2;
            Expression = $"{var1.Name}={var2.Name}";
            Vars = new[] { var1, var2 };
        }
        public CSPVar Var1;
        public CSPVar Var2;
    }
    [XmlName(XmlName = "pointPositionConstraint")]
    public class PointPositionConstraint : DraftConstraint
    {
        public readonly DraftPoint Point;

        Vector2d _location;
        public Vector2d Location
        {
            get => _location; set
            {
                BeforeChanged?.Invoke();
                _location = value;
                Parent.RecalcConstraints();
            }
        }

        public double X
        {
            get => _location.X; set
            {
                _location.X = value;
                Parent.RecalcConstraints();
            }
        }
        public double Y
        {
            get => _location.Y; set
            {
                _location.Y = value;
                Parent.RecalcConstraints();
            }
        }
        public PointPositionConstraint(XElement el, Draft parent) : base(parent)
        {
            if (el.Attribute("id") != null)
                Id = int.Parse(el.Attribute("id").Value);

            Point = parent.Elements.OfType<DraftPoint>().First(z => z.Id == int.Parse(el.Attribute("pointId").Value));
            X = Helpers.ParseDouble(el.Attribute("x").Value);
            Y = Helpers.ParseDouble(el.Attribute("y").Value);
        }

        public PointPositionConstraint(DraftPoint draftPoint1, Draft parent) : base(parent)
        {
            this.Point = draftPoint1;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            return (Point.Location - Location).Length < eps;
        }

        internal void Update()
        {
            //Point.SetLocation(Location);
            var top = Point.Parent.Constraints.OfType<TopologyConstraint>().FirstOrDefault();
            var dir = Location - Point.Location;
            if (top != null)
            {
                //whole draft translate
                var d = Point.Parent;
                foreach (var item in d.DraftPoints)
                {
                    item.SetLocation(item.Location + dir);
                }
            }
            else
                Point.SetLocation(Location);
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            Update();
        }

        public bool IsSame(PointPositionConstraint cc)
        {
            return cc.Point == Point;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Point == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<pointPositionConstraint id=\"{Id}\" pointId=\"{Point.Id}\" x=\"{X}\" y=\"{Y}\"/>");
        }
    }
    [XmlName(XmlName = "linearConstraintHelper")]
    public class LinearConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly LinearConstraint constraint;
        public LinearConstraintHelper(Draft parent, LinearConstraint c)
        {
            DraftParent = parent;
            constraint = c;
        }
        public LinearConstraintHelper(XElement el, Draft draft)
        {
            DraftParent = draft;
            var cid = int.Parse(el.Attribute("constrId").Value);
            constraint = draft.Constraints.OfType<LinearConstraint>().First(z => z.Id == cid);
            Shift = int.Parse(el.Attribute("shift").Value);
        }
        public decimal Length { get => constraint.Length; set => constraint.Length = value; }
        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;
        public int Shift { get; set; } = 10;
        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent { get; private set; }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<linearConstraintHelper constrId=\"{constraint.Id}\" shift=\"{Shift}\" enabled=\"{Enabled}\" snapPoint=\"{SnapPoint.X};{SnapPoint.Y}\"/>");
        }
        public static SKPath RoundedRect(SKRect bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            SKRect arc = new SKRect(bounds.Location.X, bounds.Location.Y, bounds.Location.X + size.Width, bounds.Location.Y + size.Height);
            SKPath path = new SKPath();


            if (radius == 0)
            {
                path.AddRect(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.Left = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Top = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.Left = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.Close();
            return path;
        }

        public void Draw(IDrawingContext ctx)
        {
            var editor = ctx.Tag as IDraftEditor;
            var elems = new[] { constraint.Element1, constraint.Element2 };
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            var hovered = editor.nearest == this;
            Pen p = new Pen(hovered ? Color.Red : Color.Blue, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;
            if (constraint.Element1 is DraftPoint dp0 && constraint.Element2 is DraftPoint dp1)
            {
                //get perpencdicular
                var diff = (dp1.Location - dp0.Location).Normalized();
                var perp = new Vector2d(-diff.Y, diff.X);
                var tr0 = ctx.Transform(dp0.Location + perp * Shift);
                var tr1 = ctx.Transform(dp1.Location + perp * Shift);
                var tr2 = ctx.Transform(dp0.Location);
                var tr3 = ctx.Transform(dp1.Location);
                var shiftX = 0;
                var text = (dp0.Location + perp * Shift + dp1.Location + perp * Shift) / 2 + perp;
                var trt = ctx.Transform(text);
                trt = new PointF(trt.X + shiftX, trt.Y);
                var ms = ctx.MeasureString(constraint.Length.ToString(), SystemFonts.DefaultFont);

                var fontBrush = Brushes.Black;
                if (hovered)
                    fontBrush = Brushes.Red;

                if (!constraint.IsSatisfied())
                {
                    var rect = new SKRect(trt.X, trt.Y, trt.X + ms.Width, trt.Y + ms.Height);
                    rect.Inflate(1.3f, 1.3f);
                    var rr = new SKRoundRect(rect, 5);
                    ctx.FillRoundRectangle(Brushes.Red, rr);
                    ctx.DrawRoundRectangle(Pens.Black, rr);
                    fontBrush = Brushes.White;
                }

                ctx.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, fontBrush, trt);

                SnapPoint = text;

                //ctx.DrawLine(p, tr0, tr1);
                ctx.DrawArrowedLine(p, tr0, tr1, 5);
                ctx.SetPen(hovered ? Pens.Red : Pens.Blue);
                ctx.DrawLine(tr0, tr2);
                ctx.DrawLine(tr1, tr3);
                if (hovered)
                {
                    ctx.FillCircle(Brushes.Red, tr2.X, tr2.Y, 5);
                    ctx.FillCircle(Brushes.Red, tr3.X, tr3.Y, 5);
                }
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);

                var diff = (dp.Location - pp).Normalized();
                var perp = new Vector2d(-diff.Y, diff.X);
                var tr0 = ctx.Transform(dp.Location + perp * Shift);
                var tr1 = ctx.Transform(pp + perp * Shift);
                var text = (dp.Location + perp * Shift + pp + perp * Shift) / 2 + perp;
                var trt = ctx.Transform(text);
                ctx.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, Brushes.Black, trt);
                SnapPoint = text;
                //get proj of point to line
                //var diff = (pp - dp.Location).Length;
                ctx.SetPen(p);
                ctx.DrawLine(tr0, tr1);
                var tr2 = ctx.Transform(dp.Location);
                var tr3 = ctx.Transform(pp);
                ctx.SetPen(Pens.Red);
                ctx.DrawLine(tr0, tr2);
                ctx.DrawLine(tr1, tr3);
            }
        }

        public override void Draw()
        {

        }
    }
    public interface IEditFieldsContainer
    {
        IName[] GetObjects();
    }
    public class EditFieldAttribute : Attribute
    {

    }
    public interface ICommand
    {
        string Name { get; }
        Action<IDrawable, object> Process { get; }
    }
    public interface ICommandsContainer
    {
        ICommand[] Commands { get; }
    }
    public interface IName
    {
        string Name { get; set; }
    }
    public class VerticalConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly VerticalConstraint constraint;
        public VerticalConstraintHelper(VerticalConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent { get; private set; }

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Line.Center;
            var perp = new Vector2d(-constraint.Line.Dir.Y, constraint.Line.Dir.X);

            SnapPoint = (dp0);
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            //create bezier here
            ctx.FillCircle(Brushes.Green, tr0.X, tr0.Y, gap);
            ctx.SetPen(new Pen(Brushes.Orange, 3));
            ctx.DrawLine(tr0.X, tr0.Y + 5, tr0.X, tr0.Y - 5);
        }

        public override void Draw()
        {

        }
    }
    public class IntersectInfo
    {
        public double Distance;
        public TriangleInfo Target;
        public IMeshNodesContainer Model;
        public Vector3d Point { get; set; }
        public object Parent;
    }
    public interface IMeshNodesContainer
    {
        MeshNode[] Nodes { get; }
    }
    public class MeshNode
    {
        public bool Visible { get; set; } = true;
        //public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        public bool Contains(TriangleInfo tr)
        {
            return Triangles.Any(z => z.IsSame(tr));
        }

        public virtual void SwitchNormal()
        {
            //if (!(Parent.Surface is BRepPlane pl)) return;

            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }

        public MeshNode RestoreXml(XElement mesh)
        {
            MeshNode ret = new MeshNode();
            foreach (var tr in mesh.Elements())
            {
                TriangleInfo tt = new TriangleInfo();
                tt.RestoreXml(tr);
                ret.Triangles.Add(tt);
            }
            return ret;
        }

        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<mesh>");
            foreach (var item in Triangles)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</mesh>");
        }

        public bool Contains(TriangleInfo target, Matrix4d mtr1)
        {
            return Triangles.Any(z => z.Multiply(mtr1).IsSame(target));
        }
    }
    public class PerpendicularConstraintTool : AbstractDraftTool
    {
        public PerpendicularConstraintTool(IDraftEditor ee) : base(ee)
        {
        }


        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        public override void MouseDown(MouseEventArgs e)
        {

        }

        public override void MouseUp(MouseEventArgs e)
        {

        }

        public override void Select()
        {


        }

        public override void Update()
        {

        }
    }
    public class PerpendicularConstraint : DraftConstraint
    {
        public DraftLine Element1;
        public DraftLine Element2;
        public DraftPoint CommonPoint;
        public PerpendicularConstraint(DraftLine draftPoint1, DraftLine draftPoint2, Draft parent) : base(parent)
        {
            var ar1 = new[] { draftPoint2.V0, draftPoint2.V1 };
            var ar2 = new[] { draftPoint1.V0, draftPoint1.V1 };
            if (ar1.Intersect(ar2).Count() != 1) throw new ArgumentException();
            CommonPoint = ar1.Intersect(ar2).First();
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;

            return Math.Abs(Vector2d.Dot(dp0.Dir, dp1.Dir)) <= eps;
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            /*var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            if (dp0.Frozen && dp1.Frozen)
            {
                throw new ConstraintsException("double frozen");
            }
            var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            dp0 = ar[0];
            dp1 = ar[1];
            if (dp1.Frozen || (dp1.V0 == CommonPoint && dp1.V1.Frozen) || (dp1.V1 == CommonPoint && dp1.V0.Frozen))
            {
                var temp = dp1;
                dp1 = dp0;
                dp0 = temp;
            }

            //generate all valid candidates first. then random select
            //not frozen points to move
            var mp = new[] { dp1.V0, dp1.V1, dp0.V1, dp0.V0 }.Distinct().Where(z => !z.Frozen).ToArray();

            if (!CommonPoint.Frozen)
            {
                //intersect
            }
            else
            if (dp1.V0 == CommonPoint)
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                var cand1 = CommonPoint.Location + projectV * dp1.Length;
                var cand2 = CommonPoint.Location - projectV * dp1.Length;
                if ((cand1 - dp1.V1.Location).Length < (cand2 - dp1.V1.Location).Length)
                {
                    dp1.V1.SetLocation(cand1);
                }
                else
                {
                    dp1.V1.SetLocation(cand2);
                }
            }
            else
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                //dp1.V0.SetLocation(CommonPoint.Location + projectV * dp1.Length);
                var cand1 = CommonPoint.Location + projectV * dp1.Length;
                var cand2 = CommonPoint.Location - projectV * dp1.Length;
                if ((cand1 - dp1.V0.Location).Length < (cand2 - dp1.V0.Location).Length)
                {
                    dp1.V0.SetLocation(cand1);
                }
                else
                {
                    dp1.V0.SetLocation(cand2);
                }
            }
            /* var diff = (dp1.Location - dp0.Location).Normalized();
             dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public bool IsSame(PerpendicularConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Element1 == de || Element2 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<perpendicularConstraint p0=\"{Element1.Id}\" p1=\"{Element2.Id}\"/>");
        }
    }
    public class PerpendicularConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly PerpendicularConstraint constraint;
        public PerpendicularConstraintHelper(PerpendicularConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent => throw new System.NotImplementedException();

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Element1.Center;
            var dp1 = constraint.Element2.Center;
            var tr0 = ctx.Transform(dp0);
            var tr1 = ctx.Transform(dp1);
            var text = ctx.Transform((dp0 + dp1) / 2);

            ctx.DrawString("P-|", SystemFonts.DefaultFont, Brushes.Black, text);
            SnapPoint = (dp0 + dp1) / 2;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            Pen p = new Pen(Color.Red, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;


            //create bezier here
            ctx.DrawPolygon(p, new PointF[] { tr0, tr1 });
        }

        public override void Draw()
        {

        }
    }
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
    public class IntFieldEditor : IName
    {
        public IntFieldEditor(PropertyInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public PropertyInfo Field;
        public int Value
        {
            get
            {
                return ((int)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
    public class CSPConstrEqualVarValue : CSPConstrEqualExpression
    {
        public CSPConstrEqualVarValue(CSPVar var, double val)
        {
            Var1 = var;
            Value = val;
            Expression = $"{Var1.Name}={val}";
            Vars = new[] { Var1 };
        }
        public CSPVar Var1;
        public double Value;
    }
    public class Segment
    {
        public override string ToString()
        {
            return "Start: " + Start + "; End: " + End;
        }
        public Vector2d Start;
        public Vector2d End;
        public double Length()
        {
            return (End - Start).Length;
        }
    }
    public class Plane
    {
        public Plane()
        {

        }

        public Vector3d Position { get; set; }


        public Vector3d Normal { get; set; }




        public Vector3d[] GetBasis()
        {
            Vector3d[] shifts = new[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Vector3d axis1 = Vector3d.Zero;
            for (int i = 0; i < shifts.Length; i++)
            {
                var proj = ProjPoint(Position + shifts[i]);

                if (Vector3d.Distance(proj, Position) > 10e-6)
                {
                    axis1 = (proj - Position).Normalized();
                    break;
                }
            }
            var axis2 = Vector3d.Cross(Normal.Normalized(), axis1);

            return new[] { axis1, axis2 };
        }
        public Vector2d ProjectPointUV(Vector3d v)
        {
            var basis = GetBasis();
            return GetUVProjPoint(v, basis[0], basis[1]);
        }
        public Vector2d GetUVProjPoint(Vector3d point, Vector3d axis1, Vector3d axis2)
        {
            var p = GetProjPoint(point) - Position;
            var p1 = Vector3d.Dot(p, axis1);
            var p2 = Vector3d.Dot(p, axis2);
            return new Vector2d(p1, p2);
        }
        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Position;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }
        public Vector3d ProjPoint(Vector3d point)
        {
            var nrm = Normal.Normalized();
            var v = point - Position;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }

        public Line3D Intersect(Plane ps)
        {
            Line3D ret = new Line3D();

            var dir = Vector3d.Cross(ps.Normal, Normal);


            var k1 = ps.GetKoefs();
            var k2 = GetKoefs();
            var a1 = k1[0];
            var b1 = k1[1];
            var c1 = k1[2];
            var d1 = k1[3];

            var a2 = k2[0];
            var b2 = k2[1];
            var c2 = k2[2];
            var d2 = k2[3];



            var res1 = det2(new[] { a1, a2 }, new[] { b1, b2 }, new[] { -d1, -d2 });
            var res2 = det2(new[] { a1, a2 }, new[] { c1, c2 }, new[] { -d1, -d2 });
            var res3 = det2(new[] { b1, b2 }, new[] { c1, c2 }, new[] { -d1, -d2 });

            List<Vector3d> vvv = new List<Vector3d>();

            if (res1 != null)
            {
                Vector3d v1 = new Vector3d((float)res1[0], (float)res1[1], 0);
                vvv.Add(v1);

            }

            if (res2 != null)
            {
                Vector3d v1 = new Vector3d((float)res2[0], 0, (float)res2[1]);
                vvv.Add(v1);
            }
            if (res3 != null)
            {
                Vector3d v1 = new Vector3d(0, (float)res3[0], (float)res3[1]);
                vvv.Add(v1);
            }

            var pnt = vvv.OrderBy(z => z.Length).First();


            var r1 = IsOnPlane(pnt);
            var r2 = IsOnPlane(pnt);

            ret.Start = pnt;
            ret.End = pnt + dir * 100;
            return ret;
        }
        public bool IsOnPlane(Vector3d orig, Vector3d normal, Vector3d check, double tolerance = 10e-6)
        {
            return (Math.Abs(Vector3d.Dot(orig - check, normal)) < tolerance);
        }
        public bool IsOnPlane(Vector3d v)
        {
            return IsOnPlane(Position, Normal, v);
        }
        double[] det2(double[] a, double[] b, double[] c)
        {
            var d = a[0] * b[1] - a[1] * b[0];
            if (d == 0) return null;
            var d1 = c[0] * b[1] - c[1] * b[0];
            var d2 = a[0] * c[1] - a[1] * c[0];
            var x = d1 / d;
            var y = d2 / d;
            return new[] { x, y };
        }




        public double[] GetKoefs()
        {
            double[] ret = new double[4];
            ret[0] = Normal.X;
            ret[1] = Normal.Y;
            ret[2] = Normal.Z;
            ret[3] = -(ret[0] * Position.X + Position.Y * ret[1] + Position.Z * ret[2]);

            return ret;
        }
    }
    [XmlName(XmlName = "scale")]
    public class ScaleTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.Scale(Vector);
        }

        internal override TransformationChainItem Clone()
        {
            return new ScaleTransformChainItem() { Vector = Vector };
        }

        internal override void RestoreXml(XElement elem)
        {
            Vector = Helpers.ParseVector(elem.Attribute("vec").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<scale vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");

        }
    }
    [XmlName(XmlName = "translate")]
    public class TranslateTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.CreateTranslation(Vector);
        }

        internal override TransformationChainItem Clone()
        {
            return new TranslateTransformChainItem() { Vector = Vector };
        }

        internal override void RestoreXml(XElement elem)
        {
            Vector = Helpers.ParseVector(elem.Attribute("vec").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<translate vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");
        }
    }
    [XmlName(XmlName = "rotate")]
    public class RotationTransformChainItem : TransformationChainItem
    {
        public Vector3d Axis = Vector3d.UnitZ;
        public double Angle { get; set; }
        public override Matrix4d Matrix()
        {
            return Matrix4d.Rotate(Axis, Angle * Math.PI / 180);
        }

        internal override TransformationChainItem Clone()
        {
            return new RotationTransformChainItem() { Axis = Axis, Angle = Angle };
        }

        internal override void RestoreXml(XElement elem)
        {
            Axis = Helpers.ParseVector(elem.Attribute("axis").Value);
            Angle = Helpers.ParseDouble(elem.Attribute("angle").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<rotate axis=\"{Axis.X};{Axis.Y};{Axis.Z}\" angle=\"{Angle}\"/>");

        }
    }
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points3D = new List<Vector3d>();

        public Draft()
        {
            Plane = new PlaneHelper() { Normal = Vector3d.UnitZ, Position = Vector3d.Zero };
            _inited = true;
        }

        public Draft(XElement el)
        {
            Restore(el);
            _inited = true;
        }
        public void Clear()
        {
            Elements.Clear();
            Helpers.Clear();
            Constraints.Clear();
        }
        public void Restore(XElement el)
        {
            Clear();
            Plane = new PlaneHelper(el.Element("plane"));
            Name = el.Attribute("name").Value;
            foreach (var item2 in el.Elements())
            {
                if (item2.Name == "point")
                {
                    DraftPoint dl = new DraftPoint(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "line")
                {
                    DraftLine dl = new DraftLine(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "ellipse")
                {
                    DraftEllipse dl = new DraftEllipse(item2, this);
                    AddElement(dl);
                }
            }

            var constr = el.Element("constraints");
            if (constr != null)
            {
                Type[] types = new[] {
                      typeof(LinearConstraint),
                      typeof(VerticalConstraint),
                     typeof(HorizontalConstraint),
                     typeof(EqualsConstraint),
                     typeof(PointPositionConstraint),
                     typeof(TopologyConstraint),
                                  };
                foreach (var item in constr.Elements())
                {
                    var fr = types.FirstOrDefault(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                    if (fr == null) continue;
                    var v = Activator.CreateInstance(fr, new object[] { item, this }) as DraftConstraint;
                    //if (v is IXmlStorable xx)
                    //{
                    //    xx.RestoreXml(item);
                    //}
                    AddConstraint(v);
                }
            }

            var helpers = el.Element("helpers");
            if (helpers != null)
            {
                var types = Assembly.GetEntryAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlNameAttribute), true) != null).ToArray();
                foreach (var item in helpers.Elements())
                {
                    var fr = types.FirstOrDefault(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                    if (fr == null) continue;
                    var v = Activator.CreateInstance(fr, new object[] { item, this }) as IDraftHelper;
                    var ch = ConstraintHelpers.ToArray();
                    if (v is IDraftConstraintHelper dch)
                    {
                        if (!ch.Any(z => z.Constraint.Id == dch.Constraint.Id))
                        {
                            AddHelper(v);
                        }
                        else
                        {
                            //todo: warning!
                            //
                            if (MessageReporter != null)
                            {
                                MessageReporter.Warning($"duplicate constraint helper detected. skipped: {dch.Constraint.Id}");
                            }
                        }
                    }
                    else
                        AddHelper(v);
                }
            }

            EndEdit();
        }

        public DraftElement[][] GetWires()
        {
            var remains = DraftLines.Where(z => !z.Dummy).ToList();
            List<List<DraftLine>> ret1 = new List<List<DraftLine>>();

            while (remains.Any())
            {
                List<DraftLine> added = new List<DraftLine>();
                foreach (var rem in remains)
                {
                    bool good = false;

                    foreach (var item in ret1)
                    {
                        var arr1 = item.SelectMany(z => new[] { z.V0, z.V1 }).ToArray();
                        if (arr1.Contains(rem.V0) || arr1.Contains(rem.V1))
                        {
                            item.Add(rem);
                            good = true;
                            break;
                        }
                    }
                    if (good)
                    {
                        added.Add(rem);
                    }
                }
                if (added.Count == 0)
                {
                    ret1.Add(new List<DraftLine>());
                    ret1.Last().Add(remains[0]);
                    added.Add(remains[0]);
                }
                foreach (var item in added)
                {
                    remains.Remove(item);
                }
            }

            List<List<DraftElement>> ret = new List<List<DraftElement>>();
            var remains1 = DraftEllipses.Where(z => !z.Dummy).ToList();

            ret.AddRange(ret1.Select(z => z.Select(u => u as DraftElement).ToList()));
            foreach (var item in remains1)
            {
                ret.Add(new List<DraftElement>()
                {
                    item
                });
            }

            return ret.Select(z => z.ToArray()).ToArray();
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<draft name=\"{Name}\">");
            Plane.Store(writer);
            foreach (var item in DraftPoints)
            {
                item.Store(writer);
            }
            foreach (var item in Elements.Except(DraftPoints))
            {
                item.Store(writer);
            }
            writer.WriteLine("<constraints>");
            foreach (var item in Constraints)
            {
                item.Store(writer);
            }
            writer.WriteLine("</constraints>");
            writer.WriteLine("<helpers>");
            foreach (var item in Helpers)
            {
                item.Store(writer);
            }
            writer.WriteLine("</helpers>");
            writer.WriteLine("</draft>");
        }

        bool _inited = false;

        bool expandGraphSolver(ConstraintSolverContext ctx)
        {
            var start = Stopwatch.StartNew();
            int cntr = 0;
            while (true)
            {
                var unsat = Constraints.Where(z => !z.IsSatisfied()).ToArray();
                if (unsat.Length == 0) return true;
                cntr++;
                if (start.Elapsed.TotalSeconds > 5 /*&& !Debugger.IsAttached*/)
                {
                    //throw new LiteCadException("time exceed");
                    return false;
                }
                var cnctd = unsat.Where(z => ctx.FreezedPoints.Any(uu => z.ContainsElement(uu))).ToArray();
                var top = Constraints.OfType<TopologyConstraint>().First();
                var vv = cnctd.Where(zz => zz is VerticalConstraint || zz is HorizontalConstraint).ToArray();
                List<DraftPoint> toFreeze = new List<DraftPoint>();
                foreach (var item in vv)
                {
                    item.RandomUpdate(ctx);
                    DraftLine line = null;
                    if (item is VerticalConstraint vv2)
                    {
                        line = vv2.Line;
                    }
                    if (item is HorizontalConstraint hh)
                    {
                        line = hh.Line;
                    }
                    toFreeze.Add(line.V0);
                    toFreeze.Add(line.V1);
                }
                var size = cnctd.OfType<LinearConstraint>().ToArray();
                foreach (var ss in size)
                {
                    ss.RandomUpdate(ctx);
                }
                ctx.FreezedPoints.AddRange(toFreeze);
            }
        }


        public CSPTask ExtractCSPTask()
        {
            CSPTask task = new CSPTask();
            int vcntr = 0;
            foreach (var item in DraftPoints)
            {
                task.Vars.Add(new CSPVar() { Name = "x" + vcntr });
                task.Vars.Add(new CSPVar() { Name = "y" + vcntr });
                vcntr++;
            }
            var ppc2 = Constraints.OfType<PointPositionConstraint>().ToArray();
            foreach (var item in ppc2)
            {
                var vind = DraftPoints.ToList().IndexOf(item.Point);
                task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "x" + vind), item.Location.X));
                task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "y" + vind), item.Location.Y));
            }
            var vert = Constraints.OfType<VerticalConstraint>().ToArray();
            foreach (var item in vert)
            {
                var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
                var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
                task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "x" + vind0), task.Vars.First(uu => uu.Name == "x" + vind1)));
            }
            var horiz = Constraints.OfType<HorizontalConstraint>().ToArray();
            foreach (var item in horiz)
            {
                var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
                var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
                task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "y" + vind0), task.Vars.First(uu => uu.Name == "y" + vind1)));
            }
            var linears = Constraints.OfType<LinearConstraint>().ToList();
            var eqls = Constraints.OfType<EqualsConstraint>().ToArray();

            if (Constraints.Any(z => z is TopologyConstraint))
            {
                var topo = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
                List<EqualsConstraint> notFoundEqs = new List<EqualsConstraint>();
                foreach (var item in eqls)
                {
                    if (linears.Any(z => z.IsLineConstraint(item.SourceLine)))
                    {
                        var frr = linears.First(z => z.IsLineConstraint(item.SourceLine));
                        linears.Add(new LinearConstraint(item.TargetLine.V0, item.TargetLine.V1, frr.Length, this));
                    }
                    else
                    {
                        notFoundEqs.Add(item);
                    }
                }

                foreach (var item in notFoundEqs)
                {
                    //add something like this: x3-x2=x1-x0
                    var vind0 = DraftPoints.ToList().IndexOf(item.TargetLine.V0);
                    var vind1 = DraftPoints.ToList().IndexOf(item.TargetLine.V1);
                    var tind0 = DraftPoints.ToList().IndexOf(item.SourceLine.V0);
                    var tind1 = DraftPoints.ToList().IndexOf(item.SourceLine.V1);

                    var t1 = topo.Lines.First(z => z.Line == item.TargetLine);
                    var t2 = topo.Lines.First(z => z.Line == item.SourceLine);

                    if (vert.Any(z => z.Line == item.TargetLine) && vert.Any(z => z.Line == item.SourceLine))
                    {
                        var vx1 = task.Vars.First(zz => zz.Name == "y" + vind0);
                        var vx2 = task.Vars.First(zz => zz.Name == "y" + vind1);
                        var vx3 = task.Vars.First(zz => zz.Name == "y" + tind0);
                        var vx4 = task.Vars.First(zz => zz.Name == "y" + tind1);
                        if (Math.Abs(t1.Dir.Y - t2.Dir.Y) > 1)
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx1.Name + "-" + vx2.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }
                        else
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx2.Name + "-" + vx1.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }

                    }
                    if (horiz.Any(z => z.Line == item.TargetLine) && horiz.Any(z => z.Line == item.SourceLine))
                    {
                        var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
                        var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
                        var vx3 = task.Vars.First(zz => zz.Name == "x" + tind0);
                        var vx4 = task.Vars.First(zz => zz.Name == "x" + tind1);
                        if (Math.Abs(t1.Dir.X - t2.Dir.X) > 1)
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx1.Name + "-" + vx2.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }
                        else
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx2.Name + "-" + vx1.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }

                    }
                }

                foreach (var item in linears)
                {
                    if (!(item.Element1 is DraftPoint dp0 && item.Element2 is DraftPoint dp1)) continue;
                    var vind0 = DraftPoints.ToList().IndexOf(dp0);
                    var vind1 = DraftPoints.ToList().IndexOf(dp1);

                    var fr = topo.Lines.FirstOrDefault(z => new[] { z.Line.V0, z.Line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2);
                    if (fr == null)
                    {
                        //add dist equation
                        continue;
                    }
                    var dd = fr.Dir;
                    vind0 = DraftPoints.ToList().IndexOf(fr.Line.V0);
                    vind1 = DraftPoints.ToList().IndexOf(fr.Line.V1);
                    var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
                    var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
                    var vy1 = task.Vars.First(zz => zz.Name == "y" + vind0);
                    var vy2 = task.Vars.First(zz => zz.Name == "y" + vind1);
                    if (Math.Abs(dd.X) == 1)
                    {
                        task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vx1, vx2 }, Expression = vx2.Name + "=" + vx1.Name + (dd.X > 0 ? "+" : "-") + item.Length });
                    }
                    if (Math.Abs(dd.Y) == 1)
                    {
                        task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vy1, vy2 }, Expression = vy2.Name + "=" + vy1.Name + (dd.Y > 0 ? "+" : "-") + item.Length });
                    }
                }
            }
            return task;
        }
        public bool Solve()
        {
            var task = ExtractCSPTask();
            CSPVarContext ctx = new CSPVarContext() { Task = task };
            bool res;
            int vcntr = 0;
            if (res = ctx.Solve())
            {
                vcntr = 0;
                foreach (var item in DraftPoints)
                {
                    var xv = task.Vars.First(zz => zz.Name == "x" + vcntr);
                    var yv = task.Vars.First(zz => zz.Name == "y" + vcntr);
                    item.SetLocation(ctx.Infos.First(z => z.Var == xv).Value, ctx.Infos.First(z => z.Var == yv).Value);
                    vcntr++;
                }
            }
            return res;
        }
        public void RecalcConstraints()
        {
            if (!_inited) return;
            return;
            //ConstraintSolverContext ccc = new ConstraintSolverContext();
            ///*var ppc = Constraints.OfType<PointPositionConstraint>().ToArray();
            //foreach (var item in ppc)
            //{
            //    item.Update();
            //}*/
            //// ccc.FreezedPoints.AddRange(ppc.Select(z => z.Point).ToArray());

            ////expand graph solver
            //if (Constraints.Any(z => z is TopologyConstraint))
            //{
            //    CSPTask task = new CSPTask();
            //    int vcntr = 0;
            //    foreach (var item in DraftPoints)
            //    {
            //        task.Vars.Add(new CSPVar() { Name = "x" + vcntr });
            //        task.Vars.Add(new CSPVar() { Name = "y" + vcntr });
            //        vcntr++;
            //    }
            //    var ppc2 = Constraints.OfType<PointPositionConstraint>().ToArray();
            //    foreach (var item in ppc2)
            //    {
            //        var vind = DraftPoints.ToList().IndexOf(item.Point);
            //        task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "x" + vind), item.Location.X));
            //        task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "y" + vind), item.Location.Y));
            //    }
            //    var vert = Constraints.OfType<VerticalConstraint>().ToArray();
            //    foreach (var item in vert)
            //    {
            //        var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
            //        var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
            //        task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "x" + vind0), task.Vars.First(uu => uu.Name == "x" + vind1)) { });
            //    }
            //    var horiz = Constraints.OfType<HorizontalConstraint>().ToArray();
            //    foreach (var item in horiz)
            //    {
            //        var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
            //        var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
            //        task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "y" + vind0), task.Vars.First(uu => uu.Name == "y" + vind1)) { });
            //    }
            //    var linears = Constraints.OfType<LinearConstraint>().ToArray();
            //    var topo = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
            //    foreach (var item in linears)
            //    {
            //        if (!(item.Element1 is DraftPoint dp0 && item.Element2 is DraftPoint dp1)) continue;
            //        var vind0 = DraftPoints.ToList().IndexOf(dp0);
            //        var vind1 = DraftPoints.ToList().IndexOf(dp1);

            //        var fr = topo.Lines.FirstOrDefault(z => new[] { z.Line.V0, z.Line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2);
            //        if (fr == null) continue;
            //        var dd = fr.Dir;
            //        vind0 = DraftPoints.ToList().IndexOf(fr.Line.V0);
            //        vind1 = DraftPoints.ToList().IndexOf(fr.Line.V1);
            //        var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
            //        var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
            //        var vy1 = task.Vars.First(zz => zz.Name == "y" + vind0);
            //        var vy2 = task.Vars.First(zz => zz.Name == "y" + vind1);
            //        if (Math.Abs(dd.X) == 1)
            //        {
            //            task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vx1, vx2 }, Expression = vx2.Name + "=" + vx1.Name + (dd.X > 0 ? "+" : "-") + item.Length });
            //        }
            //        if (Math.Abs(dd.Y) == 1)
            //        {
            //            task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vy1, vy2 }, Expression = vy2.Name + "=" + vy1.Name + (dd.Y > 0 ? "+" : "-") + item.Length });
            //        }
            //    }

            //    CSPVarContext ctx = new CSPVarContext() { Task = task };
            //    if (ctx.Solve())
            //    {
            //        vcntr = 0;
            //        foreach (var item in DraftPoints)
            //        {
            //            var xv = task.Vars.First(zz => zz.Name == "x" + vcntr);
            //            var yv = task.Vars.First(zz => zz.Name == "y" + vcntr);
            //            item.SetLocation(ctx.Infos.First(z => z.Var == xv).Value, ctx.Infos.First(z => z.Var == yv).Value);
            //            vcntr++;
            //        }
            //        return;
            //    }
            //    /*   var top = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
            //       ccc.FreezedLinesDirs = top.Lines.ToList();
            //       if (expandGraphSolver(ccc))
            //           return;*/
            //}
            //return;
            //RandomSolve();
        }
        public void RandomSolve()
        {
            ConstraintSolverContext ccc = new ConstraintSolverContext();
            var lc = Constraints.Where(z => z.Enabled).ToArray();
            int counter = 0;
            int limit = 1000;
            Stopwatch stw = Stopwatch.StartNew();
            StringWriter sw = new StringWriter();

            Store(sw);
            int timeLimit = 5;
            var elem = XElement.Parse(sw.ToString());

            while (true)
            {
                if (stw.Elapsed.TotalSeconds > timeLimit)
                {
                    DebugHelpers.Error("constraints satisfaction error");
                    Restore(XElement.Parse(sw.ToString()));
                    break;
                }
                counter++;
                if (lc.All(z => z.IsSatisfied())) break;
                //preserve Refs?
                //Restore(elem);

                /*if (counter > limit)
                {
                    DebugHelpers.Error("constraints satisfaction error");
                    break;
                }*/

                foreach (var item in lc)
                {
                    if (item.IsSatisfied())
                        continue;

                    item.RandomUpdate(ccc);
                }
            }
        }
        public List<DraftElement> Elements = new List<DraftElement>();
        public List<IDraftHelper> Helpers = new List<IDraftHelper>();
        public IEnumerable<IDraftConstraintHelper> ConstraintHelpers => Helpers.OfType<IDraftConstraintHelper>();
        public List<DraftConstraint> Constraints = new List<DraftConstraint>();
        public void AddHelper(IDraftHelper h)
        {
            Helpers.Add(h);
            h.Parent = this;
        }

        public Action<DraftConstraint> ConstraintAdded;
        public Action<DraftConstraint> BeforeConstraintChanged;
        public void AddConstraint(DraftConstraint h)
        {
            h.BeforeChanged = () =>
            {
                BeforeConstraintChanged?.Invoke(h);
            };
            Constraints.Add(h);
            RecalcConstraints();
            ConstraintAdded?.Invoke(h);
        }

        public decimal CalcTotalCutLength()
        {
            decimal ret = 0;
            ret += DraftEllipses.Sum(z => z.CutLength());
            foreach (var item in DraftLines)
            {
                ret += (decimal)((item.V0.Location - item.V1.Location).Length);
            }
            return ret;
        }

        public PlaneHelper Plane;

        public decimal CalcArea()
        {
            //get nest
            //calc area

            return 0;
        }

        public Vector2d[] Points => DraftPoints.Select(z => z.Location).ToArray();
        public DraftPoint[] DraftPoints => Elements.OfType<DraftPoint>().ToArray();
        public DraftLine[] DraftLines => Elements.OfType<DraftLine>().ToArray();
        public DraftEllipse[] DraftEllipses => Elements.OfType<DraftEllipse>().ToArray();
        public void EndEdit()
        {
            //2d->3d
            var basis = Plane.GetBasis();
            Points3D.Clear();
            foreach (var item in DraftLines)
            {
                Points3D.Add(basis[0] * item.V0.X + basis[1] * item.V0.Y + Plane.Position);
                Points3D.Add(basis[0] * item.V1.X + basis[1] * item.V1.Y + Plane.Position);
            }
        }
        public override void Draw()
        {

        }
        public override void RemoveChild(IDrawable dd)
        {
            if (dd is IDraftConstraintHelper dh)
            {
                Helpers.Remove(dh);
                Constraints.Remove(dh.Constraint);
            }

            Childs.Remove(dd);
        }

        public void AddElement(DraftElement h)
        {
            if (Elements.Contains(h))
                return;

            Elements.Add(h);
            h.Parent = this;
        }

        public void RemoveElement(IDraftHelper de)
        {
            Helpers.Remove(de);
        }

        public void RemoveElement(DraftElement de)
        {
            if (de is DraftPoint dp)
            {
                var ww = Elements.OfType<DraftLine>().Where(z => z.V0 == dp || z.V1 == dp).ToArray();
                var ww2 = Elements.OfType<DraftEllipse>().Where(z => z.Center == dp).ToArray();
                var ww3 = ww.OfType<DraftElement>().Union(ww2).ToArray();
                foreach (var item in ww3)
                {
                    Constraints.RemoveAll(z => z.ContainsElement(item));
                    Helpers.RemoveAll(zz => zz is IDraftConstraintHelper z && z.Constraint.ContainsElement(item));
                    Elements.Remove(item);
                }
            }
            Constraints.RemoveAll(z => z.ContainsElement(de));
            Helpers.RemoveAll(zz => zz is IDraftConstraintHelper z && z.Constraint.ContainsElement(de));
            Elements.Remove(de);
        }

    }
    public abstract class DraftElement
    {
        public int Id { get; set; }
        public bool Frozen { get; set; }//can't be changed during constraints satisfaction        
        public Draft Parent;
        public bool Dummy { get; set; }//dummy line. don't export

        protected DraftElement(Draft parent)
        {
            Parent = parent;
            Id = FactoryHelper.NewId++;
        }
        protected DraftElement(XElement e, Draft parent)
        {
            if (e.Attribute("id") != null)
            {
                Id = int.Parse(e.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
            Parent = parent;
        }

        public abstract void Store(TextWriter writer);

    }
    public class DraftPoint : DraftElement
    {
        public Vector2d _location;
        public Vector2d Location { get => _location; private set => _location = value; }

        public double X
        {
            get => Location.X;
            set
            {
                _location.X = value;
                Parent.RecalcConstraints();
            }
        }
        public double Y
        {
            get => Location.Y;
            set
            {
                _location.Y = value;
                Parent.RecalcConstraints();
            }
        }

        public DraftPoint(Draft parent, float x, float y) : base(parent)
        {

            Location = new Vector2d(x, y);
        }
        public DraftPoint(Draft parent, double x, double y) : base(parent)
        {
            Location = new Vector2d(x, y);
        }

        public DraftPoint(XElement item2, Draft d) : base(item2, d)
        {
            X = double.Parse(item2.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            Y = double.Parse(item2.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            if (item2.Attribute("frozen") != null)
                Frozen = bool.Parse(item2.Attribute("frozen").Value);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<point id=\"{Id}\" x=\"{X}\" y=\"{Y}\" frozen=\"{Frozen}\" />");
        }

        public void SetLocation(double x, double y)
        {
            SetLocation(new Vector2d(x, y));
        }
        public void SetLocation(Vector2d vector2d)
        {
            if (Frozen)
            {
                throw new LiteCadException("try update frozen");
            }
            _location = vector2d;
        }
    }
    public abstract class DraftConstraint
    {
        public Draft Parent { get; private set; }
        public DraftConstraint(Draft parent)
        {
            Parent = parent;
            Id = FactoryHelper.NewId++;
        }
        public int Id { get; set; }
        public abstract bool IsSatisfied(float eps = 1e-6f);
        public abstract void RandomUpdate(ConstraintSolverContext ctx);
        public bool Enabled { get; set; } = true;
        public Action BeforeChanged;
        public abstract bool ContainsElement(DraftElement de);

        internal abstract void Store(TextWriter writer);
    }
    public class Contour
    {
        //public BRepWire Wire;
        public List<Segment> Elements = new List<Segment>();

        public Vector2d Start
        {
            get
            {
                return Elements[0].Start;
            }
        }

        public Vector2d End
        {
            get
            {
                return Elements.Last().End;
            }
        }

        public virtual bool Contains(Vector2d point)
        {
            return Math.Abs(((point - Start).Length + (point - End).Length) - (End - Start).Length) < float.Epsilon;
        }

        public static double DistByRing(double v1, double v2, double diap)
        {
            var dist1 = Math.Abs(v1 - v2);
            var dist2 = Math.Abs(v1 - (v2 - diap));
            var dist3 = Math.Abs(v2 - (v1 - diap));
            return new[] { dist1, dist2, dist3 }.Min();
        }
        public static double DistByXRing(Vector2d v1, Vector2d v2, double xdiap)
        {
            return new Vector2d(DistByRing(v1.X, v2.X, xdiap), v2.Y - v1.Y).Length;
        }
        public Contour ConnectNext(Contour[] cntr, bool useEnd = true, bool useStart = true)
        {
            if (Elements.Count == 0)
            {
                //Wire = cntr[0].Wire;
                Elements.AddRange(cntr[0].Elements);
                return cntr[0];
            }

            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            double rmindist = double.MaxValue;
            Contour minsegm = null;
            bool reverse = false;
            bool insert = false;
            Vector2d connectPoint1 = Vector2d.One;
            Vector2d connectPoint2 = Vector2d.One;

            foreach (var item in cntr)
            {
                if (useEnd)
                {
                    var v1 = new Vector2d(DistByRing(end.X, item.Start.X, Math.PI * 2), end.Y - item.Start.Y);
                    var dist1 = v1.Length;
                    var rdist1 = Math.Abs((end - item.Start).Length);
                    if (dist1 < mindist || rdist1 < rmindist)
                    {
                        connectPoint1 = end;
                        connectPoint2 = item.Start;
                        mindist = dist1;
                        rmindist = rdist1;
                        minsegm = item;
                        reverse = false;
                        insert = false;

                    }

                    var v2 = new Vector2d(DistByRing(end.X, item.End.X, Math.PI * 2), end.Y - item.End.Y);
                    var dist2 = v2.Length;
                    var rdist2 = Math.Abs((end - item.End).Length);
                    if (dist2 < mindist || rdist2 < rmindist)
                    {
                        connectPoint1 = end;
                        connectPoint2 = item.End;
                        mindist = dist2;
                        rmindist = rdist2;
                        minsegm = item;
                        reverse = true;
                        insert = false;

                    }
                }
                if (useStart)
                {

                    var v3 = new Vector2d(DistByRing(start.X, item.Start.X, Math.PI * 2), start.Y - item.Start.Y);
                    var dist3 = v3.Length;
                    var rdist3 = Math.Abs((start - item.Start).Length);
                    if (dist3 < mindist || rdist3 < rmindist)
                    {
                        connectPoint1 = start;
                        connectPoint2 = item.Start;
                        rmindist = rdist3;
                        mindist = dist3;
                        minsegm = item;
                        reverse = true;
                        insert = true;
                    }

                    var v4 = new Vector2d(DistByRing(start.X, item.End.X, Math.PI * 2), start.Y - item.End.Y);
                    var dist4 = v4.Length;
                    var rdist4 = Math.Abs((start - item.End).Length);
                    if (dist4 < mindist || rdist4 < rmindist)
                    {
                        connectPoint1 = start;
                        connectPoint2 = item.End;
                        mindist = dist4;
                        rmindist = rdist4;
                        minsegm = item;
                        reverse = false;
                        insert = true;
                    }
                }
            }

            var diffX = Math.Abs(connectPoint2.X - connectPoint1.X);
            double epsilon = 1e-5;
            if (minsegm != null && mindist < epsilon)
            {
                var item = minsegm;
                if (reverse)
                {
                    item = new Contour();
                    foreach (var ritem in minsegm.Elements.ToArray().Reverse())
                    {
                        item.Elements.Add(new Segment() { Start = ritem.End, End = ritem.Start }); ;
                    }
                }
                if (Math.Abs(diffX - Math.PI * 2) < epsilon)
                {

                    var temp = new Contour();
                    foreach (var ritem in item.Elements.ToArray())
                    {
                        var shift = ((connectPoint1.X < connectPoint2.X) ? -1 : 1) * new Vector2d(Math.PI * 2, 0);
                        temp.Elements.Add(new Segment() { Start = ritem.Start + shift, End = ritem.End + shift });
                    }
                    item = temp;
                }
                if (insert)
                {

                    Elements.InsertRange(0, item.Elements);
                }
                else
                {


                    Elements.AddRange(item.Elements);
                }
                return minsegm;
            }

            return null;
        }


        public Segment ConnectNext(Segment[] segments)
        {
            if (Elements.Count == 0)
            {
                Elements.Add(segments[0]);
                return segments[0];
            }
            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            Segment minsegm = null;
            bool reverse = false;

            bool insert = false;
            foreach (var item in segments)
            {
                var dist1 = Math.Abs((end - item.Start).Length);
                if (dist1 < mindist)
                {
                    mindist = dist1;
                    minsegm = item;
                    reverse = false;
                    insert = false;

                }
                var dist2 = Math.Abs((end - item.End).Length);

                if (dist2 < mindist)
                {
                    mindist = dist2;
                    minsegm = item;
                    reverse = true;
                    insert = false;
                }

                var dist3 = Math.Abs((start - item.Start).Length);
                if (dist3 < mindist)
                {
                    mindist = dist3;
                    minsegm = item;
                    reverse = true;
                    insert = true;
                }
                var dist4 = Math.Abs((start - item.End).Length);

                if (dist4 < mindist)
                {
                    mindist = dist4;
                    minsegm = item;
                    reverse = false;
                    insert = true;

                }

            }
            double epsilon = 1e-5;
            if (minsegm != null && mindist < epsilon)
            {
                if (insert)
                {
                    var item = minsegm;
                    if (reverse)
                    {
                        item = new Segment() { End = minsegm.Start, Start = minsegm.End };
                    }
                    Elements.Insert(0, item);
                }
                else
                {
                    var item = minsegm;
                    if (reverse)
                    {
                        item = new Segment() { End = minsegm.Start, Start = minsegm.End };
                    }
                    Elements.Add(item);
                }

                return minsegm;
            }

            return null;
        }

        public double Area()
        {
            var ar = GeometryUtils.CalculateArea(Elements.Select(z => z.End).ToArray());
            return Math.Abs(ar);
        }
        internal void Reduce(double eps = 1e-8)
        {
            Elements.RemoveAll(x => x.Length() < eps);
        }
    }
    public static class GeometryUtils
    {
        public static Vector2d GetProjPoint(Vector2d point, Vector2d loc, Vector2d norm)
        {
            var v = point - loc;
            var dist = Vector2d.Dot(v, norm);
            //var proj = point - dist * norm;
            //return proj;
            return dist * norm + loc;

        }
        public static Line3D[] SplitByPlane(this MeshNode mn, Matrix4d matrix, Plane pl)
        {
            List<Line3D> vv = new List<Line3D>();
            List<TriangleInfo> trianglesModifed = new List<TriangleInfo>();
            foreach (var item in mn.Triangles)
            {
                trianglesModifed.Add(item.Multiply(matrix));
            }
            foreach (var item in trianglesModifed)
            {
                try
                {
                    vv.AddRange(item.SplitByPlane(pl));
                }
                catch (Exception ex)
                {

                }
            }
            return vv.ToArray();
        }
        public static bool IntersectSegments(Vector2d p0, Vector2d p1, Vector2d q0, Vector2d q1, ref Vector2d c0)
        {
            double ux = p1.X - p0.X;
            double uy = p1.Y - p0.Y;
            double vx = q1.X - q0.X;
            double vy = q1.Y - q0.Y;
            double wx = p0.X - q0.X;
            double wy = p0.Y - q0.Y;

            double d = (ux * vy - uy * vx);
            double s = (vx * wy - vy * wx) / d;

            // Intersection point
            c0.X = p0.X + s * ux;
            c0.Y = p0.Y + s * uy;
            if (!IsPointInsideSegment(p0, p1, c0)) return false;
            if (!IsPointInsideSegment(q0, q1, c0)) return false;
            return true;
        }
        public static bool IsPointOnLine(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - start;
            if (d1.Length < tolerance) return true;
            if ((end - start).Length < tolerance) throw new LiteCadException("degenerated line");
            d1 = d1.Normalized();
            var p2 = (end - start).Normalized();
            var crs = Vector3d.Cross(new Vector3d(d1.X, d1.Y, 0), new Vector3d(p2.X, p2.Y, 0));
            return Math.Abs(crs.Length) < epsilon;
        }
        public static bool IsPointInsideSegment(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            if (!IsPointOnLine(start, end, pnt, epsilon)) return false;
            var diff1 = (pnt - start).Length + (pnt - end).Length;
            return Math.Abs(diff1 - (end - start).Length) < epsilon;
        }
        public static Vector3d? Intersect3dCrossedLines(Line3D ln0, Line3D ln1)
        {
            var v0 = ln0.Start;
            var v1 = ln1.Start;
            var d0 = ln0.Dir;
            var d1 = ln1.Dir;
            var d0n = ln0.Dir.Normalized();
            var d1n = ln1.Dir.Normalized();
            var check1 = Vector3d.Dot(Vector3d.Cross(d0n, d1n), v0 - v1);
            if (Math.Abs(check1) > 10e-6) return null;//parallel


            var cd = v1 - v0;
            var a1 = Vector3d.Cross(d1, cd).Length;
            var a2 = Vector3d.Cross(d1, d0).Length;
            var vv0 = v0 + d0n * 10000;
            var vv1 = v1 + d1n * 10000;
            var m1 = v0 + (a1 / a2) * d0;

            Line3D l1 = new Line3D() { Start = v0, End = vv0 };
            Line3D l2 = new Line3D() { Start = v1, End = vv1 };

            var m2 = v0 - (a1 / a2) * d0;
            if (Vector3d.Distance(m1, m2) < 10e-6) return m1;
            float epsilon = 10e-6f;
            if (l1.IsPointOnLine(m1, epsilon) && l2.IsPointOnLine(m1, epsilon)) return m1;

            if (l1.IsPointOnLine(m2, epsilon) && l2.IsPointOnLine(m2, epsilon)) return m2;

            return m1;
        }
        public static Vector2d[][] TriangulateWithHoles(Vector2d[][] points, Vector2d[][] holes, bool checkArea = true)
        {
            //holes = holes.Where(z => z.Length > 2).ToArray();//skip bad holes
            #region checker
            double area = 0;
            foreach (var item in points)
            {
                area += Math.Abs(signedArea(item));
            }

            if (holes != null)
                foreach (var item in holes)
                {
                    area -= Math.Abs(signedArea(item));
                }

            #endregion
            Polygon poly2 = new Polygon();

            foreach (var item in points)
            {
                var item2 = item;
                if (signedArea(item) < 0) { item2 = item.Reverse().ToArray(); }
                var a = item2.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    poly2.Add(new TriangleNet.Geometry.Contour(a));
                }
            }

            if (holes != null)
                foreach (var item in holes)
                {
                    var item2 = item;
                    if (signedArea(item) < 0) { item2 = item.Reverse().ToArray(); }
                    var a = item2.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                    var interior = GetRandomInteriorPoint(item2);
                    if (a.Count() > 2)
                    {
                        poly2.Add(new TriangleNet.Geometry.Contour(a), new TriangleNet.Geometry.Point(interior.X, interior.Y));
                    }
                }

            ConstraintMesher.ScoutCounter = 0;


            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions() { }, new QualityOptions());


            var ret1 = trng.Triangles.Select(z => new Vector2d[] {
                    new Vector2d(z.GetVertex(0).X, z.GetVertex(0).Y),
                    new Vector2d(z.GetVertex(1).X, z.GetVertex(1).Y),
                    new Vector2d(z.GetVertex(2).X, z.GetVertex(2).Y)
                }
            ).ToArray();

            double area2 = 0;
            foreach (var item in ret1)
            {
                area2 += Math.Abs(signedArea(item));
            }
            if (checkArea && Math.Abs(area2 - area) > 10e-3)
            {
                throw new LiteCadException("wrong triangulation. area mismatch");
            }

            return ret1;
        }
        public static bool pnpoly(PointF[] verts, float testx, float testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }
        public static bool pnpoly(Vector2d[] verts, double testx, double testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }

        public static Random Random = new Random();
        public static PointF GetRandomInteriorPoint(Vector2d[] polygon)
        {
            var rand = Random;
            var maxx = polygon.Max(z => z.X);
            var minx = polygon.Min(z => z.X);
            var maxy = polygon.Max(z => z.Y);
            var miny = polygon.Min(z => z.Y);
            var tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
            var ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
            PointF test = new PointF(tx, ty);

            int cntr = 0;
            while (true)
            {
                cntr++;
                if (cntr > 1000)
                {
                    throw new LiteCadException("GetRandomInteriorPoint failed");
                }
                if (pnpoly(polygon.ToArray(), test.X, test.Y))
                {
                    break;
                }
                tx = rand.Next((int)(minx * 10000), (int)(maxx * 10000)) / 10000f;
                ty = rand.Next((int)(miny * 10000), (int)(maxy * 10000)) / 10000f;
                test = new PointF(tx, ty);
            }
            return test;
        }
        static double signedArea(Vector2d[] polygon)
        {
            double area = 0.0;

            int j = 1;
            for (int i = 0; i < polygon.Length; i++, j++)
            {
                j = j % polygon.Length;
                area += (polygon[j].X - polygon[i].X) * (polygon[j].Y + polygon[i].Y);
            }

            return area / 2.0;
        }


        public static bool AlmostEqual(double a, double b, double eps = 1e-8)
        {
            return Math.Abs(a - b) <= eps;
        }
        public static double CalculateArea(Vector2d[] Points)
        {
            // Add the first point to the end.
            int num_points = Points.Length;
            Vector2d[] pts = new Vector2d[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return Math.Abs(area);
        }


        public static double CalculateAngle(Vector3d dir1, Vector3d dir2, Vector3d axis)
        {
            var crs = Vector3d.Cross(dir2, dir1);
            var ang2 = Vector3d.CalculateAngle(dir1, dir2);
            if (!(Vector3d.Dot(axis, crs) < 0))
            {
                ang2 = (2 * Math.PI) - ang2;
            }
            return ang2;
        }

        internal static string PointHashKey(Vector2d z, int v)
        {
            return (int)(z.X * v) + ";" + (int)(z.Y * v);
        }



        public static double polygonArea(NFP polygon)
        {
            double area = 0;
            int i, j;
            for (i = 0, j = polygon.Points.Length - 1; i < polygon.Points.Length; j = i++)
            {
                area += (polygon.Points[j].X + polygon.Points[i].X) * (polygon.Points[j].Y
                    - polygon.Points[i].Y);
            }
            return 0.5f * area;
        }

        internal static double signed_area(Vector2d[] polygon)
        {
            double area = 0.0;

            int j = 1;
            for (int i = 0; i < polygon.Length; i++, j++)
            {
                j = j % polygon.Length;
                area += (polygon[j].X - polygon[i].X) * (polygon[j].Y + polygon[i].Y);
            }

            return area / 2.0;

        }
    }
    public abstract class AbstractDraftTool : ITool
    {

        public AbstractDraftTool(IDraftEditor editor)
        {
            Editor = editor;
        }

        public abstract void Deselect();


        public abstract void Draw();

        public IDraftEditor Editor;
        public abstract void MouseDown(MouseEventArgs e);

        public abstract void MouseUp(MouseEventArgs e);

        public abstract void Select();

        public abstract void Update();
    }
    public interface IDraftEditor
    {
        object nearest { get; }
        Draft Draft { get; }
        IDrawingContext DrawingContext { get; }
        void Backup();
        void ResetTool();

    }
    public class TriangleInfo
    {
        public VertexInfo[] Vertices;

        public double Area()
        {
            var v0 = Vertices[1].Position - Vertices[0].Position;
            var v1 = Vertices[2].Position - Vertices[0].Position;
            var crs = Vector3d.Cross(v0, v1);
            return crs.Length / 2;
        }

        internal bool IsSame(TriangleInfo tr)
        {
            float eps = 1e-8f;
            return tr.Vertices.All(uu =>
            {
                return Vertices.Any(zz => (zz.Position - uu.Position).Length < eps);
            });
        }

        public Vector3d Normal()
        {
            var v0 = Vertices[1].Position - Vertices[0].Position;
            var v1 = Vertices[2].Position - Vertices[0].Position;
            var crs = Vector3d.Cross(v0, v1);
            return crs.Normalized();
        }

        public Vector3d Center()
        {
            Vector3d z1 = Vector3d.Zero;
            foreach (var item in Vertices)
            {
                z1 += item.Position;
            }
            z1 /= 3;
            return z1;
        }
        public Vector3d V0 => Vertices[0].Position;

        public TriangleInfo Multiply(Matrix4d matrix)
        {
            TriangleInfo ret = new TriangleInfo();
            ret.Vertices = new VertexInfo[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                ret.Vertices[i] = new VertexInfo();
                ret.Vertices[i].Position = Vector3d.Transform(Vertices[i].Position, matrix);
            }

            return ret;
        }

        public Vector3d V1 => Vertices[1].Position;
        public Vector3d V2 => Vertices[2].Position;
        internal Line3D[] GetLines()
        {
            List<Line3D> ret = new List<Line3D>();
            ret.Add(new Line3D() { Start = V0, End = V1 });
            ret.Add(new Line3D() { Start = V1, End = V2 });
            ret.Add(new Line3D() { Start = V2, End = V0 });
            return ret.ToArray();
        }
        public Plane GetPlane()
        {
            var n0 = V2 - V0;
            var n1 = V1 - V0;
            var normal = Vector3d.Cross(n0, n1).Normalized();
            return (new Plane() { Position = V0, Normal = normal });
        }
        public Line3D[] SplitByPlane(Plane pl)
        {

            List<Vector3d> ret = new List<Vector3d>();
            var pl0 = GetPlane();
            var crs = Vector3d.Cross(pl0.Normal, pl.Normal);
            if (crs.Length < 1e-5f) return new Line3D[] { };

            var ln = pl0.Intersect(pl);
            if (ln == null) return null;
            var lns = GetLines();
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in lns)
            {
                var l3 = item;
                var inter = GeometryUtils.Intersect3dCrossedLines(ln, l3);
                if (inter != null && l3.IsPointInsideSegment(inter.Value)) pp.Add(inter.Value);
            }

            var pnts = pp.ToArray();
            List<Vector3d> pnts3 = new List<Vector3d>();
            foreach (var item in pnts)
            {
                bool good = true;
                for (int i = 0; i < pnts3.Count; i++)
                {
                    if ((pnts[i] - item).Length < 1e-6)
                    {
                        good = false;
                        break;
                    }
                }
                if (good) pnts3.Add(item);
            }
            pnts = pnts3.ToArray();
            if (pnts.Length == 2)
                return new[] { new Line3D() { Start = pnts[0], End = pnts[1] } };

            return new Line3D[] { };
            //return pnts;

        }

        internal void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<triangle>");
            foreach (var item in Vertices)
            {
                writer.WriteLine($"<vertex pos=\"{item.Position.X};{item.Position.Y};{item.Position.Z}\" normal=\"{item.Normal.X};{item.Normal.Y};{item.Normal.Z}\"/>");
            }
            writer.WriteLine("</triangle>");
        }

        internal void RestoreXml(XElement t)
        {
            int cnt = 0;
            Vertices = new VertexInfo[t.Elements("vertex").Count()];

            foreach (var tt in t.Elements("vertex"))
            {
                Vertices[cnt] = new VertexInfo();
                Vertices[cnt].Position = Helpers.ParseVector(tt.Attribute("pos").Value);
                Vertices[cnt].Normal = Helpers.ParseVector(tt.Attribute("normal").Value);
                cnt++;
            }
        }

        public bool Contains(Vector3d v, double eps = 1e-8)
        {
            foreach (var item in Vertices)
            {
                if ((item.Position - v).Length < eps) { return true; }
            }
            return false;
        }
    }
    [XmlName(XmlName = "linearConstraint")]
    public class LinearConstraint : DraftConstraint
    {
        public DraftElement Element1;
        public DraftElement Element2;

        decimal _length;
        public decimal Length
        {
            get => _length; set
            {
                BeforeChanged?.Invoke();
                _length = value;
                Element1.Parent.RecalcConstraints();
            }
        }
        public LinearConstraint(XElement el, Draft parent) : base(parent)
        {
            if (el.Attribute("id") != null)
                Id = int.Parse(el.Attribute("id").Value);

            Element1 = parent.Elements.First(z => z.Id == int.Parse(el.Attribute("p0").Value));
            Element2 = parent.Elements.First(z => z.Id == int.Parse(el.Attribute("p1").Value));
            Length = Helpers.ParseDecimal(el.Attribute("length").Value);
        }

        public LinearConstraint(DraftElement draftPoint1, DraftElement draftPoint2, decimal len, Draft parent) : base(parent)
        {
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
            Length = len;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var elems = new[] { Element1, Element2 };
            if (Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)
            {
                var diff = (dp1.Location - dp0.Location).Length;
                return Math.Abs(diff - (double)Length) < eps;
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();

                //get proj of point to line
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);
                var diff = (pp - dp.Location).Length;
                return Math.Abs(diff - (double)Length) < eps;
            }
            throw new NotImplementedException();
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
            var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.SetLocation(dp0.Location + diff * (double)Length);
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var elems = new[] { Element1, Element2 };

            if (Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)
            {
                if ((dp0.Frozen && dp1.Frozen) || (ctx.FreezedPoints.Contains(dp1) && ctx.FreezedPoints.Contains(dp0)))
                {
                    throw new ConstraintsException("double frozen");
                }
                var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
                dp0 = ar[0];
                dp1 = ar[1];
                if (dp1.Frozen || ctx.FreezedPoints.Contains(dp1))
                {
                    var temp = dp1;
                    dp1 = dp0;
                    dp0 = temp;
                }
                var diff = (dp1.Location - dp0.Location).Normalized();
                //preserve location
                bool good = false;
                var fr = ctx.FreezedLinesDirs.FirstOrDefault(z => (z.Line.V0 == dp0 && z.Line.V1 == dp1) || (z.Line.V0 == dp1 && z.Line.V1 == dp0));
                if (fr != null)
                {
                    var lns1 = dp0.Parent.DraftLines.FirstOrDefault(uu => (uu.V0 == dp0 && uu.V1 == dp1) || (uu.V0 == dp1 && uu.V1 == dp0));
                    if (lns1 != null)
                    {
                        var fr2 = ctx.FreezedLinesDirs.FirstOrDefault(zz => zz.Line == lns1);
                        if (fr2 != null)
                        {
                            diff = fr2.Dir;
                            dp0 = Element1 as DraftPoint;
                            dp1 = Element2 as DraftPoint;
                            if (ctx.FreezedPoints.Contains(dp1) && !ctx.FreezedPoints.Contains(dp0))
                            {
                                dp0.SetLocation(dp1.Location - diff * (double)Length);
                                good = true;
                            }
                            if (ctx.FreezedPoints.Contains(dp0) && !ctx.FreezedPoints.Contains(dp1))
                            {
                                dp1.SetLocation(dp0.Location + diff * (double)Length);
                                good = true;
                            }
                        }
                    }
                }
                if (!good)
                    dp1.SetLocation(dp0.Location + diff * (double)Length);
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);

                var cand1 = pp + dl.Normal * (double)Length;
                var cand2 = pp - dl.Normal * (double)Length;
                if (GeometryUtils.Random.Next(100) < 50)
                {
                    dp.SetLocation(cand1);
                }
                else
                {
                    dp.SetLocation(cand2);
                }
            }
        }
        public bool IsSame(LinearConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }
        public bool IsLineConstraint(DraftLine line)
        {
            if (!(Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)) return false;
            return new[] { line.V0, line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2;
        }
        public override bool ContainsElement(DraftElement de)
        {
            return Element1 == de || Element2 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<linearConstraint id=\"{Id}\" length=\"{Length}\" p0=\"{Element1.Id}\" p1=\"{Element2.Id}\"/>");
        }
    }
    public static class GUIHelpers
    {
        public static DialogResult ShowQuestion(string text, string caption = null, MessageBoxButtons btns = MessageBoxButtons.YesNo)
        {
            if (caption == null) { /*caption = Form1.Form.Text;*/ }
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Question);
        }
        public static DialogResult Warning(string text, string caption = null, MessageBoxButtons btns = MessageBoxButtons.OK)
        {
            if (caption == null) {/* caption = Form1.Form.Text; */}
            return MessageBox.Show(text, caption, btns, MessageBoxIcon.Warning);
        }
        public static object EditorStart(object init, string nm, Type control, bool dialog = true)
        {
            Form f = new Form() { Text = nm };
            f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterScreen;
            var cc = Activator.CreateInstance(control) as UserControl;
            (cc as IPropEditor).Init(init);
            f.Controls.Add(cc);
            f.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            f.AutoSize = true;
            if (dialog)
            {
                f.ShowDialog();
            }
            else
            {
                f.TopMost = true;
                f.Show();
            }
            return (cc as IPropEditor).ReturnValue;
        }
    }
    public class ParallelConstraintTool : AbstractTool
    {
        public ParallelConstraintTool(IEditor editor) : base(editor) { }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        public override void MouseDown(MouseEventArgs e)
        {

        }

        public override void MouseUp(MouseEventArgs e)
        {

        }

        public override void Select()
        {


        }

        public override void Update()
        {

        }
    }
    public class HorizontalConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly HorizontalConstraint constraint;
        public HorizontalConstraintHelper(HorizontalConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent { get; private set; }

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Line.Center;
            var perp = new Vector2d(-constraint.Line.Dir.Y, constraint.Line.Dir.X);

            SnapPoint = (dp0);
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            //create bezier here
            ctx.FillCircle(Brushes.Green, tr0.X, tr0.Y, gap);
            ctx.SetPen(new Pen(Brushes.Orange, 3));
            ctx.DrawLine(tr0.X - 5, tr0.Y, tr0.X + 5, tr0.Y);
        }

        public override void Draw()
        {

        }
    }
    public class EqualsConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly EqualsConstraint constraint;

        public Draft DraftParent { get; private set; }
        public EqualsConstraintHelper(Draft parent, EqualsConstraint c)
        {
            DraftParent = parent;
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(IDrawingContext ctx)
        {
            var editor = ctx.Tag as IDraftEditor;


            var hovered = editor.nearest == this;

            var dp0 = constraint.TargetLine.Center;
            var perp = new Vector2d(-constraint.TargetLine.Dir.Y, constraint.TargetLine.Dir.X);

            SnapPoint = (dp0) + constraint.TargetLine.Dir * 10 / ctx.zoom;
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            var gap2 = 6;
            var offset = 25;
            //create bezier here
            var shiftX = (int)(constraint.TargetLine.Dir.X * offset);
            var shiftY = (int)(constraint.TargetLine.Dir.Y * offset);

            ctx.FillCircle(hovered ? Brushes.Blue : Brushes.Green, tr0.X + shiftX, tr0.Y + shiftY, gap);
            ctx.SetPen(new Pen(Brushes.Violet, 3));
            ctx.DrawLine(tr0.X - gap2 + shiftX, tr0.Y - 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y - 4 + shiftY);
            ctx.DrawLine(tr0.X - gap2 + shiftX, tr0.Y + 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y + 4 + shiftY);

            if (hovered)
            {
                var tr00 = ctx.Transform(constraint.SourceLine.V0.Location);
                var tr1 = ctx.Transform(constraint.SourceLine.V1.Location);
                var tr2 = ctx.Transform(constraint.TargetLine.V0.Location);
                var tr3 = ctx.Transform(constraint.TargetLine.V1.Location);
                ctx.FillCircle(Brushes.Red, tr2.X, tr2.Y, 5);
                ctx.FillCircle(Brushes.Red, tr3.X, tr3.Y, 5);

                ctx.FillCircle(Brushes.Red, tr00.X, tr00.Y, 5);
                ctx.FillCircle(Brushes.Red, tr1.X, tr1.Y, 5);
            }
        }

        public override void Draw()
        {

        }
    }
}
