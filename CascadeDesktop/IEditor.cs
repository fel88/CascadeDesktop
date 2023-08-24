namespace CascadeDesktop
{
    public interface IEditor
    {
        OCCTProxy Proxy { get; }
        void ResetTool();
    }
}
