using System.Collections.Generic;

namespace CascadeDesktop
{
    public interface IEditor
    {
        IOCCTProxyInterface Proxy { get; }
        void ResetTool();
        OccSceneObject GetSelectedOccObject();                
        List<OccSceneObject> Objs { get; }
        void SetStatus(string text, InfoType type = InfoType.Info);        
    }
}
