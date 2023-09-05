namespace CascadeDesktop
{
    public interface IEditor
    {
        OCCTProxy Proxy { get; }
        void ResetTool();
        void SetStatus(string text, InfoType type = InfoType.Info);        
    }
}
