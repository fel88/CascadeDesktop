using System;

namespace CSPLib.Interfaces
{
    public interface IEditor
    {
        IDrawable[] Parts { get; }
        ITool CurrentTool { get; }
        event Action<ITool> ToolChanged;
        IntersectInfo Pick { get; }
        void ObjectSelect(object nearest);
        void ResetTool();
        void Backup();
        void Undo();
    }
}
