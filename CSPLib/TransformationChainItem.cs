using OpenTK;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    public abstract class TransformationChainItem : IXmlStorable
    {
        public abstract Matrix4d Matrix();

        void IXmlStorable.RestoreXml(XElement elem)
        {
            RestoreXml(elem);
        }

        internal abstract void StoreXml(TextWriter writer);
        internal abstract void RestoreXml(XElement elem);

        void IXmlStorable.StoreXml(TextWriter writer)
        {
            StoreXml(writer);
        }

        internal abstract TransformationChainItem Clone();
    }
}
