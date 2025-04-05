namespace CSPLib
{
    public interface IPropEditor
    {
        void Init(object o);
        object ReturnValue { get; }
    }
}
