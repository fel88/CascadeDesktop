using System.Collections.Generic;
using System.Text;

namespace CSPLib
{
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

}
