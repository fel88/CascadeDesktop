using OpenTK;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    [XmlName(XmlName = "scale")]
    public class ScaleTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.Scale(Vector);
        }

        internal override TransformationChainItem Clone()
        {
            return new ScaleTransformChainItem() { Vector = Vector };
        }

        internal override void RestoreXml(XElement elem)
        {
            Vector = Helpers.ParseVector(elem.Attribute("vec").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<scale vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");

        }
    }
}
