using OCCT.Interfaces;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CascadeDesktop.Interfaces
{
    public interface IEditor
    {
        IOCCTProxyInterface Proxy { get; }
        void ResetTool();
        OccSceneObject GetSelectedOccObject();
        List<OccSceneObject> Objs { get; }
        void SetStatus(string text, InfoType type = InfoType.Info);
        void Remove(OccSceneObject item);
        void DeleteUI(OccSceneObject occ);
        void RenameUI(OccSceneObject occ, Form owner = null);
    }
}
