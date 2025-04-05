namespace CSPLib
{
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
}
