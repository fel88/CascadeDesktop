namespace CascadeDesktop.Common.Interfaces
{
    public interface IPropEditor
    {
        void Init(object o);
        object ReturnValue { get; }
    }
}
