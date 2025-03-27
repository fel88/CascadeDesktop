using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.IO;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Drawing2D;
using SkiaSharp.Views.Desktop;
using OpenTK;

namespace CSPLib
{
    public class CSPVarContext
    {
        public CSPTask Task;
        public List<CSPVarInfo> Infos = new List<CSPVarInfo>();

        public bool Resolved(CSPVar var)
        {
            return Infos.Any(z => z.Var == var);
        }
        public bool Unresolved(CSPVar var)
        {
            return !Resolved(var);
        }

        public void SubtaskSolve(CSPConstrEqualExpression[] constrs)
        {
            var unres = constrs.SelectMany(z => z.Vars).Distinct().Where(Unresolved).ToArray();
            if (unres.Length != constrs.Length) return;
            // substitute and solve 1st equation and then 2nd equation            

            if (constrs.Length == 2)
            {
                //construct tree and simpificate. transformers required
                var fr = constrs.First(z => z.LeftVars(this).Length == 1);
                var tkns1 = Tokenize(fr.Expression.Substring(0, fr.Expression.IndexOf('=')));
                var tkns2 = Tokenize(fr.Expression.Substring(fr.Expression.IndexOf('=') + 1));
                //construct new equation with substitute
                var sec = constrs.First(z => z != fr);
                List<Token> tkns = new List<Token>();
                var tkns3 = Tokenize(sec.Expression);
                foreach (var item in tkns3)
                {
                    if (item.Tag is CSPVar v && tkns1[0].Tag == item.Tag)
                    {
                        bool bracetsReq = false;
                        if (tkns.Any() && tkns.Last().Text == "-")
                        {
                            bracetsReq = true;
                        }
                        if (bracetsReq)
                            //  tkns.Add(new Token() { Text = "(" });
                            //inverse signs?
                            for (int i = 0; i < tkns2.Length; i++)
                            {
                                if (tkns2[i].Text == "-")
                                {
                                    tkns2[i].Text = "+";
                                }
                                else
                                    if (tkns2[i].Text == "+")
                                {
                                    tkns2[i].Text = "-";
                                }
                            }
                        tkns.AddRange(tkns2.ToArray());
                        //if (bracetsReq)
                        //tkns.Add(new Token() { Text = ")" });
                    }
                    else
                        tkns.Add(item);
                }
                //get expression
                StringBuilder sb = new StringBuilder();
                foreach (var item in tkns)
                {
                    if (item.Tag == unres.First(z => z != tkns1[0].Tag))
                    {
                        sb.Append("z");
                    }
                    else
                    if (item.Tag is CSPVarInfo inf)
                    {
                        sb.Append(inf.Value);
                    }
                    else
                        sb.Append(item.Text);
                }
                var expr = sb.ToString();
                var res = SolveOneVarEquation(expr);
                Infos.Add(new CSPVarInfo(unres.First(z => z != tkns1[0].Tag) as CSPVar) { Value = res });
            }
            else
            {
                double[,] input = new double[constrs.Length, constrs.Length + 1];
                for (int i1 = 0; i1 < constrs.Length; i1++)
                {
                    CSPConstrEqualExpression cc = constrs[i1];
                    var expr = cc.Expression;
                    var left = expr.Substring(0, expr.IndexOf("="));
                    var right = expr.Substring(expr.IndexOf("=") + 1);

                    var lt = Tokenize(left);
                    var rt = Tokenize(right);
                    int sign = 1;


                    for (int i = 0; i < lt.Length; i++)
                    {
                        if (lt[i].Text == "-")
                        {
                            sign = -1;
                            continue;
                        }
                        if (lt[i].Text == "+")
                        {
                            sign = 1;
                            continue;
                        }
                        if (lt[i].Tag is CSPVar vv)
                        {
                            var vind = Array.IndexOf(unres, vv);
                            input[i1, vind] += sign;
                            sign = 1;
                        }
                        else if (lt[i].Tag is CSPVarInfo vinf)
                        {
                            input[i1, input.GetLength(1) - 1] -= sign * vinf.Value;
                            sign = 1;
                        }
                        else
                        {
                            input[i1, input.GetLength(1) - 1] -= sign * double.Parse(lt[i].Text);
                            sign = 1;
                        }
                    }
                    sign = 1;
                    for (int i = 0; i < rt.Length; i++)
                    {
                        if (rt[i].Text == "-")
                        {
                            sign = -1;
                            continue;
                        }
                        if (rt[i].Text == "+")
                        {
                            sign = 1;
                            continue;
                        }
                        if (rt[i].Tag is CSPVar vv)
                        {
                            var vind = Array.IndexOf(unres, vv);
                            input[i1, vind] -= sign;
                            sign = 1;
                        }
                        else if (rt[i].Tag is CSPVarInfo vinf)
                        {
                            input[i1, input.GetLength(1) - 1] += sign * vinf.Value;
                            sign = 1;
                        }
                        else
                        {
                            input[i1, input.GetLength(1) - 1] += sign * double.Parse(rt[i].Text);
                            sign = 1;
                        }
                    }

                }

                try
                {
                    Matrix<double> T = Matrix<double>.Build.DenseOfArray(input);
                    Matrix<double> A = T.SubMatrix(0, T.RowCount, 0, T.ColumnCount - 1);
                    MathNet.Numerics.LinearAlgebra.Vector<double> B = T.Column(T.ColumnCount - 1);
                    if (A.Rank() == T.Rank())
                    {
                        var res = A.LU().Solve(B);
                        for (int i = 0; i < res.Count; i++)
                        {
                            Infos.Add(new CSPVarInfo(unres[i]) { Value = res[i] });
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void Print(double[,] M)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < M.GetLength(0); i++)
            {
                for (int j = 0; j < M.GetLength(1); j++)
                {
                    sb.Append(M[i, j] + " ");
                }
                sb.AppendLine();
            }
            Clipboard.SetText(sb.ToString());
        }



        public double SolveOneVarEquation(string expr)
        {
            var left = expr.Substring(0, expr.IndexOf("="));
            var right = expr.Substring(expr.IndexOf("=") + 1);
            double sumLeft = 0;
            double sumRight = 0;
            double sumVarLeft = 0;
            double sumVarRight = 0;
            var lt = Tokenize(left);
            var rt = Tokenize(right);
            int sign = 1;
            for (int i = 0; i < lt.Length; i++)
            {
                if (lt[i].Text == "-")
                {
                    sign = -1;
                    continue;
                }
                if (lt[i].Text == "+")
                {
                    sign = 1;
                    continue;
                }
                if (lt[i].Text == "z")
                {
                    sumVarLeft += sign;
                    sign = 1;
                }
                else
                {
                    sumLeft += sign * double.Parse(lt[i].Text);
                    sign = 1;
                }
            }
            sign = 1;
            for (int i = 0; i < rt.Length; i++)
            {
                if (rt[i].Text == "-")
                {
                    sign = -1;
                    continue;
                }
                if (rt[i].Text == "+")
                {
                    sign = 1;
                    continue;
                }
                if (rt[i].Text == "z")
                {
                    sumVarRight += sign;
                    sign = 1;
                }
                else
                {
                    sumRight += sign * double.Parse(rt[i].Text);
                    sign = 1;
                }
            }
            var totalVars = sumVarLeft - sumVarRight;
            var totalSum = sumRight - sumLeft;
            return totalSum / totalVars;

            //construct tree
            //solve
            //return 0;
        }

        public double GetVal(CSPVar var)
        {
            return Infos.First(z => z.Var == var).Value;
        }

        internal bool Solve()
        {
            var txt = Task.Dump();
            Clipboard.SetText(txt);
            List<CSPConstr> remains = new List<CSPConstr>();
            remains.AddRange(Task.Constrs);
            while (true)
            {
                var unsolved = Task.Vars.Count(zz => !Resolved(zz));
                if (Task.Vars.All(z => Infos.Any(uu => uu.Var == z))) return true;
                var ee1 = remains.OfType<CSPConstrEqualVarValue>().ToArray();
                foreach (var eee in ee1)
                {
                    Infos.Add(new CSPVarInfo(eee.Var1) { Value = eee.Value });
                    remains.Remove(eee);
                }
                var eq1 = remains.OfType<CSPConstrEqualTwoVars>().ToArray();
                foreach (var item in eq1)
                {
                    bool good = false;
                    if (Resolved(item.Var1) && !Resolved(item.Var2))
                    {
                        good = true;
                        Infos.Add(new CSPVarInfo(item.Var2) { Value = GetVal(item.Var1) });
                    }
                    if (!Resolved(item.Var1) && Resolved(item.Var2))
                    {
                        good = true;
                        Infos.Add(new CSPVarInfo(item.Var1) { Value = GetVal(item.Var2) });
                    }
                    if (Resolved(item.Var1) && Resolved(item.Var2))
                    {
                        good = true;
                    }

                    if (good)
                        remains.Remove(item);
                }

                var exprs1 = remains.OfType<CSPConstrEqualExpression>().Where(z => z.Vars.Count(uu => !Resolved(uu)) == 1).ToArray();

                foreach (var item in exprs1)
                {
                    var unres = item.Vars.FirstOrDefault(Unresolved);
                    if (unres == null) continue;

                    if (item.LeftVars(this).Count() == 1)
                    {
                        var res1 = item.Solve(this);
                        if (res1 != null)
                        {
                            Infos.Add(res1);
                            remains.Remove(item);
                        }
                    }
                }

                //search SOE (system of equations)



                var pairs = remains.OfType<CSPConstrEqualExpression>().Where(z => z.Vars.Count(Unresolved) == 2).ToArray();
                var grp = pairs.GroupBy(zz => string.Join(";", zz.Vars.Where(Unresolved).Select(t => t.Name).OrderBy(z => z).ToArray())).ToArray();
                foreach (var group in grp)
                {
                    SubtaskSolve(group.ToArray());
                }

                var unres2 = Task.Vars.Where(Unresolved).ToArray();
                var constrs2unres = remains.OfType<CSPConstrEqualExpression>().Where(z => z.Vars.Any(uu => unres2.Contains(uu))).ToArray();
                if (constrs2unres.SelectMany(z => z.Vars.Where(Unresolved)).Distinct().Count() == unres2.Length)
                {
                    SubtaskSolve(constrs2unres.ToArray());
                }
                var unsolved2 = Task.Vars.Count(zz => !Resolved(zz));
                if (unsolved2 == unsolved && unsolved2 != 0)
                {
                    //failed to satisfy
                    return false;
                }

            }
        }

        public Token[] Tokenize(string item)
        {
            List<Token> ss = new List<Token>();
            string accum = "";
            //tokenize required
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i] == '=' || item[i] == '+' || item[i] == '-')
                {
                    if (accum.Length > 0)
                    {
                        var fr = Task.Vars.FirstOrDefault(zz => zz.Name == accum);
                        if (fr != null && Infos.FirstOrDefault(z => z.Var == fr) != null)
                        {
                            var fr2 = Infos.FirstOrDefault(z => z.Var == fr);
                            //ss.Push(fr2.Value.ToString());
                            ss.Add(new Token() { Text = accum, Tag = fr2 });
                        }
                        else
                        {
                            ss.Add(new Token() { Text = accum, Tag = fr });
                            //ss.Push(accum);
                        }
                        accum = "";
                    }
                    ss.Add(new Token() { Text = item[i] + "" }); continue;
                }

                accum += item[i];
            }
            if (accum.Length > 0)
            {
                var fr = Task.Vars.FirstOrDefault(zz => zz.Name == accum);
                if (fr != null && Infos.FirstOrDefault(z => z.Var == fr) != null)
                {
                    var fr2 = Infos.FirstOrDefault(z => z.Var == fr);
                    //ss.Push(fr2.Value.ToString());
                    ss.Add(new Token() { Text = accum, Tag = fr2 });
                }
                else
                {
                    //ss.Push(accum);
                    ss.Add(new Token() { Text = accum, Tag = fr });
                }
                accum = "";
            }
            return ss.ToArray();
        }
    }
    public class Token
    {
        public string Text;
        public object Tag;
    }
    public abstract class CSPConstr
    {

    }
    public class CSPTask
    {
        public List<CSPVar> Vars = new List<CSPVar>();
        public List<CSPConstr> Constrs = new List<CSPConstr>();

        internal string Dump()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Vars)
            {
                sb.AppendLine("var " + item.Name);
            }

            foreach (var item in Constrs)
            {
                if (item is CSPConstrEqualExpression expr)
                {
                    sb.AppendLine(expr.Expression);
                }
            }
            return sb.ToString();
        }
    }
    public class CSPVarInfo
    {
        public CSPVarInfo(CSPVar v)
        {
            Var = v;
        }
        public readonly CSPVar Var;
        public double Value;

        public override string ToString()
        {
            return $"CSP var info: {Var.Name} = {Value}";
        }
    }
    public class CSPConstrEqualExpression : CSPConstr
    {
        public string Expression;
        public CSPVar[] Vars;

        public override string ToString()
        {
            return "CSP constr expr: " + Expression;
        }

        internal CSPVarInfo Solve(CSPVarContext ctx)
        {
            if (Vars.Count(ctx.Unresolved) != 1)
            {
                return null;
            }
            var unres = Vars.First(z => ctx.Unresolved(z));

            var tkns = ctx.Tokenize(Expression);

            var list = tkns.ToList();
            //list.Reverse();
            bool inside = false;
            double res = 0;
            double sign = 1;
            foreach (var itemz in list)
            {
                if (itemz.Text == "=")
                {
                    inside = true;
                    continue;
                }
                if (!inside) continue;
                if (itemz.Text.All(char.IsDigit))
                {
                    res += sign * double.Parse(itemz.Text);
                    sign = 1;
                }
                if (itemz.Tag is CSPVarInfo inf)
                {
                    res += sign * inf.Value;
                    sign = 1;
                }
                if (itemz.Text == "-") sign = -1;
            }

            string expr = "";
            foreach (var itemz in list)
            {
                if (itemz.Text.All(char.IsDigit))
                {
                    expr += itemz.Text;
                }
                else
                if (itemz.Tag is CSPVarInfo inf)
                {
                    expr += inf.Value;
                }
                else
                if (itemz.Tag is CSPVar vr)
                {
                    expr += "z";
                }
                else
                    expr += itemz.Text;
            }
            res = ctx.SolveOneVarEquation(expr);

            return new CSPVarInfo(unres) { Value = res };
        }

        internal CSPVar[] LeftVars(CSPVarContext ctx)
        {
            //get all vars before equal sign
            var t = ctx.Tokenize(Expression);
            List<CSPVar> left = new List<CSPVar>();
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i].Text == "=") break;
                if (t[i].Tag is CSPVar v)
                {
                    left.Add(v);
                }
                if (t[i].Tag is CSPVarInfo vv)
                {
                    left.Add(vv.Var);
                }
            }
            return left.ToArray();
        }

    }
    public class CSPVar
    {
        public string Name;
        public override string ToString()
        {
            return "CSP var: " + Name;
        }
    }
  
   
    public class SkiaGLDrawingContext : AbstractDrawingContext
    {
        public override void DrawPolygon(Pen p, PointF[] pointFs)
        {

        }

        public override void FillCircle(Brush brush, float v1, float v2, int rad)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (brush as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawCircle(v1, v2, rad, paint);
            }
        }



        public override void DrawLineTransformed(PointF point, PointF point2)
        {
            var canvas = Surface.Canvas;
            var pp = Transform(point);
            var pp2 = Transform(point2);
            DrawLine(pp, pp2);
        }

        public override void DrawCircle(Pen pen, float v1, float v2, float rad)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = pen.Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Stroke;
                if (pen.DashStyle != DashStyle.Solid)
                {
                    paint.PathEffect = SKPathEffect.CreateDash(pen.DashPattern, 0);
                }
                canvas.DrawCircle(v1, v2, rad, paint);
            }
        }

        public override void FillRoundRectangle(Brush blue, SKRoundRect rr)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (blue as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawRoundRect(rr, paint);
            }
        }

        public override void DrawArrowedLine(Pen p, PointF tr0, PointF tr1, int v)
        {
            DrawLine(tr0, tr1);
            var canvas = Surface.Canvas;
            var sk0 = new OpenTK.Vector2d(tr0.X, tr0.Y);
            var sk1 = new OpenTK.Vector2d(tr1.X, tr1.Y);
            var dir = (sk0 - sk1).Normalized();


            var atan2 = (float)Math.Atan2(dir.Y, dir.X);
            SKPath path2 = new SKPath();
            path2.RMoveTo(-2 * v, 0);
            path2.RLineTo(0, v);
            path2.RLineTo(2 * v, -v);
            path2.RLineTo(-2 * v, -v);
            path2.RLineTo(0, v);
            path2.Close();
            var mtr = canvas.TotalMatrix;
            canvas.Save();
            canvas.Translate(tr0.X, tr0.Y);
            canvas.RotateRadians(atan2);

            FillPath(new SolidBrush(p.Color), path2);
            
            
            canvas.SetMatrix(in mtr);
            canvas.Translate(tr1.X, tr1.Y);
            canvas.RotateRadians(atan2);
            canvas.RotateDegrees(180);


            FillPath(new SolidBrush(p.Color), path2);
            //canvas.SetMatrix(mtr);
            
            canvas.SetMatrix(in mtr);


        }

        public override void DrawRoundRectangle(Pen pen, SKRoundRect rect)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = pen.Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawRoundRect(rect, paint);
            }
        }

        public override SizeF MeasureString(string text, Font font)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {

                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                paint.TextSize = font.GetHeight();
                using (var font1 = new SKFont(SKTypeface.FromFamilyName(font.FontFamily.Name)))
                {
                    return new SizeF(paint.MeasureText(text), paint.TextSize);
                }
            }
        }

        public override void DrawLine(PointF pp, PointF pp2)
        {
            var canvas = Surface.Canvas;
            canvas.DrawLine(pp.X, pp.Y, pp2.X, pp2.Y, CurrentPaint);
        }

        public override void DrawString(string text, Font font, Brush brush, PointF position)
        {
            DrawString(text, font, brush, position.X, position.Y);
        }

        public void FillPath(Brush red, SKPath path)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (red as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawPath(path, paint);
            }
        }

        public void DrawPath(Pen pen, SKPath path)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = pen.Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawPath(path, paint);
            }
        }

        public override void FillRectangle(Brush blue, float v1, float v2, float v3, float v4)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (blue as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(v1, v2, v3, v4, paint);
            }
        }

        public override void DrawLine(float x0, float y0, float x1, float y1)
        {
            DrawLine(new PointF(x0, y0), new PointF(x1, y1));
        }

        /*internal void FillEllipse(Brush black, float v1, float v2, float v3, float v4)
        {
            var pp = Transform(new PointF(v1, v2));
            gr.FillEllipse(black, pp.X, pp.Y, v3 * scale, v4 * scale);
        }*/



        public override void DrawString(string text, Font font, Brush brush, float x, float y)
        {
            var canvas = Surface.Canvas;
            using (SKPaint paint = new SKPaint())
            {
                var clr = (brush as SolidBrush).Color;
                paint.Color = new SKColor(clr.R, clr.G, clr.B);
                paint.IsAntialias = true;
                //paint.StrokeWidth = pen.Width;
                paint.Style = SKPaintStyle.Fill;
                paint.TextSize = font.GetHeight();

                using (var font1 = new SKFont(SKTypeface.FromFamilyName(font.FontFamily.Name)))
                {
                    canvas.DrawText(text, x, y + paint.TextSize, font1, paint);
                }
            }
        }
        /*public override void DrawRectangle(SKPaint paint, float rxm, float rym, float rdx, float rdy)
        {
            var canvas = Surface.Canvas;
            canvas.DrawRect(rxm, rym, rdx, rdy, paint);
        }*/
        SKPaint CurrentPaint = new SKPaint();
        public override void SetPen(Pen pen)
        {
            CurrentPaint.Color = pen.Color.ToSKColor();
            CurrentPaint.IsAntialias = true;
            CurrentPaint.StrokeWidth = pen.Width;
            CurrentPaint.Style = SKPaintStyle.Stroke;
            CurrentPaint.PathEffect = null;
            if (pen.DashStyle != DashStyle.Solid)
            {
                CurrentPaint.PathEffect = SKPathEffect.CreateDash(pen.DashPattern, pen.DashOffset);
            }
        }

        public override void DrawRectangle(float rxm, float rym, float rdx, float rdy)
        {
            var canvas = Surface.Canvas;
            canvas.DrawRect(rxm, rym, rdx, rdy, CurrentPaint);
        }

        public override void InitGraphics()
        {

        }

        SKCanvas Canvas => Surface.Canvas;
        public override void Clear(Color white)
        {
            Canvas.Clear(white.ToSKColor());
        }

        public static bool GlSupport = true;
        public override System.Windows.Forms.Control GenerateRenderControl()
        {
            System.Windows.Forms.Control co = null;
            if (GlSupport)
            {
                co = new SKGLControl();
                ((SKGLControl)co).PaintSurface += Co_PaintSurface;
            }
            else
            {
                co = new SKControl();
                ((SKControl)co).PaintSurface += Co_PaintSurface1;
            }
            return co;
        }

        private void Co_PaintSurface1(object sender, SKPaintSurfaceEventArgs e)
        {
            Surface = e.Surface;
            PaintAction?.Invoke();
        }

        private void Co_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            Surface = e.Surface;
            PaintAction?.Invoke();
        }

        public override void DrawImage(Bitmap image, float x1, float y1, float x2, float y2)
        {
            var s = image.ToSKImage();
            var temp = CurrentPaint.FilterQuality;
            CurrentPaint.FilterQuality = SKFilterQuality.High;
            Canvas.DrawImage(s, new SKRect(x1, y1, x2, y2), CurrentPaint);
            CurrentPaint.FilterQuality = temp;

        }

        public override void ResetMatrix()
        {
            Canvas.ResetMatrix();
        }

        public override void RotateDegress(float deg)
        {
            Canvas.RotateDegrees(deg);
        }

        public override void Translate(double x, double y)
        {
            Canvas.Translate((float)x, (float)y);
        }

        Stack<SKMatrix> stack = new Stack<SKMatrix>();
        public override void PushMatrix()
        {
            stack.Push(Canvas.TotalMatrix);
        }

        public override void PopMatrix()
        { var t = stack.Pop();
            Canvas.SetMatrix(in t  );
            
        }

        public override void Scale(double x, double y)
        {
            Canvas.Scale((float)x, (float)y);
        }
    }
    public abstract class AbstractDrawingContext : IDrawingContext
    {
        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        public abstract void SetPen(Pen pen);

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
        public float scale = 1;
        public SkiaSharp.SKSurface Surface;
        public object Tag { get; set; }

        public float startx { get; set; }
        public float starty { get; set; }
        public float sx { get; set; }
        public float sy { get; set; }
        public abstract void DrawLineTransformed(PointF point, PointF point2);

        public float origsx, origsy;
        public bool isDrag = false;
        public bool isMiddleDrag { get; set; } = false;
        public bool isLeftDrag { get; set; } = false;
        public bool MiddleDrag { get { return isMiddleDrag; } }

        public float zoom { get; set; } = 1;

        public abstract void FillCircle(Brush brush, float v1, float v2, int rad);

        public PointF GetCursor()
        {
            var p = PictureBox.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }
        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            isMiddleDrag = false;
            isLeftDrag = false;

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
        public EventWrapperPictureBox PictureBox { get; set; }
        public Action PaintAction { get; set; }

        public void Init(System.Windows.Forms.Control pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }
        public MouseButtons DragButton { get; set; } = MouseButtons.Right;
        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == DragButton)
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
            }
            if (e.Button == MouseButtons.Left)
            {
                //isLeftDrag= true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
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
        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;

            pb.SizeChangedAction = Pb_SizeChanged;

            //pb.SizeChanged += Pb_SizeChanged;
            //pb.MouseWheel += PictureBox1_MouseWheel;
            //pb.MouseUp += PictureBox1_MouseUp;
            //pb.MouseDown += PictureBox1_MouseDown;
            //pb.MouseMove += PictureBox1_MouseMove;

            //Bmp = new Bitmap(pb.Control.Width, pb.Control.Height);
            //  gr = Graphics.FromImage(Bmp);
        }
        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            InitGraphics();
        }

        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //zoom *= Math.Sign(e.Delta) * 1.3f;
            //zoom += Math.Sign(e.Delta) * 0.31f;

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
        public abstract void InitGraphics();

        public abstract void DrawPolygon(Pen p, PointF[] pointFs);


        public abstract void DrawCircle(Pen pen, float v1, float v2, float rad);

        public abstract SizeF MeasureString(string text, Font font);


        public abstract void DrawString(string text, Font font, Brush brush, PointF position);


        public abstract void DrawString(string text, Font font, Brush brush, float x, float y);


        public abstract void DrawLine(float x0, float y0, float x1, float y1);


        public abstract void DrawLine(PointF pp, PointF pp2);


        public abstract void FillRoundRectangle(Brush blue, SKRoundRect rr);


        public abstract void DrawArrowedLine(Pen p, PointF tr0, PointF tr1, int v);


        public abstract void DrawRoundRectangle(Pen pen, SKRoundRect rect);


        public abstract void DrawRectangle(float rxm, float rym, float rdx, float rdy);




        public abstract void FillRectangle(Brush blue, float v1, float v2, float v3, float v4);

        public abstract void Clear(Color white);

        public abstract System.Windows.Forms.Control GenerateRenderControl();

        public abstract void DrawImage(Bitmap image, float x1, float y1, float x2, float y2);

        public abstract void ResetMatrix();

        public abstract void RotateDegress(float deg);

        public abstract void Translate(double x, double y);
        public abstract void Scale(double x, double y);

        public abstract void PushMatrix();

        public abstract void PopMatrix();

        public void DrawCircle(Pen pen, float v1, float v2, float rad, int angles, float startAngle)
        {
            var step = 360f / angles;
            List<Vector2d> pp = new List<Vector2d>();
            for (int i = 0; i < angles; i++)
            {
                var ang = step * i;
                var radd = ang * Math.PI / 180f;
                var xx = v1 + rad * Math.Cos(radd);
                var yy = v2 + rad * Math.Sin(radd);
                pp.Add(new Vector2d(xx, yy));
            }
            for (int i = 1; i <= pp.Count; i++)
            {
                var p0 = pp[i - 1].ToPointF();
                var p1 = pp[i % pp.Count].ToPointF();
                DrawLine(p0, p1);
            }
        }
    }
    public class EventWrapperPictureBox
    {
        public System.Windows.Forms.Control Control;
        public EventWrapperPictureBox(System.Windows.Forms.Control control)
        {
            Control = control;
            control.MouseUp += WrapGlControl_MouseUp;
            control.MouseDown += Control_MouseDown;
            control.KeyDown += Control_KeyDown;
            control.MouseMove += WrapGlControl_MouseMove;
            control.MouseWheel += Control_MouseWheel;
            control.KeyUp += Control_KeyUp;
            control.SizeChanged += Control_SizeChanged;
        }

        private void Control_SizeChanged(object sender, EventArgs e)
        {
            SizeChangedAction?.Invoke(sender, e);
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpUpAction?.Invoke(sender, e);
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelAction?.Invoke(sender, e);

        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDownAction?.Invoke(sender, e);
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownAction?.Invoke(sender, e);
        }

        private void WrapGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            MouseUpAction?.Invoke(sender, e);

        }

        private void WrapGlControl_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveAction?.Invoke(sender, e);

        }
        public Action<object, EventArgs> SizeChangedAction;
        public Action<object, MouseEventArgs> MouseMoveAction;
        public Action<object, MouseEventArgs> MouseUpAction;
        public Action<object, MouseEventArgs> MouseDownAction;
        public Action<object, MouseEventArgs> MouseWheelAction;
        public Action<object, KeyEventArgs> KeyUpUpAction;
        public Action<object, KeyEventArgs> KeyDownAction;

        /*public Bitmap Image
        {
            get { return (Bitmap)Control.Image; }
            set { Control.Image = value; }
        }*/

        public System.Drawing.Point PointToClient(System.Drawing.Point position)
        {
            return Control.PointToClient(position);
        }
    }

}
