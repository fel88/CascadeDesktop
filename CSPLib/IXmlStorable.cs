using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    public interface IXmlStorable
    {
        void StoreXml(TextWriter writer);
        void RestoreXml(XElement elem);

    }
}
