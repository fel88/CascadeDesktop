using CSPLib.Interfaces;

namespace CSPLib
{
    public interface IDraftEditor
    {
        object nearest { get; }
        Draft Draft { get; }
        IDrawingContext DrawingContext { get; }
        void Backup();
        void ResetTool();

    }
}
