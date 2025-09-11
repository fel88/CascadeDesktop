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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml;
using System.Numerics;
using TriangleNet.Geometry;
using DxfPad;
using CSPLib.Interfaces;
using OpenTK.Mathematics;
using Vector2 = OpenTK.Mathematics.Vector2;

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
            RenderControl.MouseDoubleClick += RenderControl_MouseDoubleClick; 
            ctx.Tag = this;
            InitPaints();
        }

        private void RenderControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
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
            {
                var pos = PointToClient(Cursor.Position);

                var pp = ctx.BackTransform(pos);
                OpenTK.Mathematics.Vector2 v1 = new OpenTK.Mathematics.Vector2(pp.X, pp.Y);

                ctx.DrawString($"current tool: {editor.CurrentTool.GetType().Name}", SystemFonts.DefaultFont, Brushes.Black, 5, 5);
                ctx.DrawString($"position {pp.X} {pp.Y}", SystemFonts.DefaultFont, Brushes.Black, 5, 25);
            }
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
            if (_draft == null || _draft.Elements.Count() == 0)
                return;

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
        public object[] selected = new object[] { };
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

        public void Clear()
        {
            Backup();
            _draft.Clear();
        }

        ITool _currentTool;
        public ITool CurrentTool => _currentTool;
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
            if (!Visible)
                return;

            RenderControl.Refresh();
            return;
        }

        public void SetTool(ITool tool)
        {
            _currentTool = tool;
        }

        public void SolveCSP()
        {
            if (!Draft.Solve())
            {
                DebugHelpers.Error("constraints satisfaction error");
            }
        }
    }
}
