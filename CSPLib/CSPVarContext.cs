using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

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

}
