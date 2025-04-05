namespace CSPLib
{
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
}
