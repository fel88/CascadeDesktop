using OCCTProxy.Common.Interfaces;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public class OccScene
    {
        public List<OccSceneObject> Objs = new List<OccSceneObject>();
        public List<ISceneObject> Parts = new List<ISceneObject>();
        public XElement ToXml(XmlStoreContext ctx)
        {
            var proxy = ctx.Proxy;
            XElement root = new XElement("root");
            foreach (var item in Objs)
            {
                var r = proxy.ExportStepStream(item.TopHandle).ToArray();
                root.Add(item.GetXml());
            }
            return root;
        }

    }
    public class XmlStoreContext
    {
        public IOCCTProxyInterface Proxy;
        public List<ModelInfo> Models;
    }

    public class ModelInfo
    {
        byte[] Data;
        public string Hash;
    }
}
