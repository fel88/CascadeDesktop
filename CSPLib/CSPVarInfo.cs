namespace CSPLib
{
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

}
