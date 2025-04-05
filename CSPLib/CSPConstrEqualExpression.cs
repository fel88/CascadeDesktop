using System.Collections.Generic;
using System.Linq;

namespace CSPLib
{
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

}
