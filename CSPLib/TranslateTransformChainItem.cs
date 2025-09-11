using OpenTK;
using OpenTK.Mathematics;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    [XmlName(XmlName = "translate")]
    public class TranslateTransformChainItem : TransformationChainItem
    {
        public Vector3d Vector;
        public override Matrix4d Matrix()
        {
            return Matrix4d.CreateTranslation(Vector);
        }

        internal override TransformationChainItem Clone()
        {
            return new TranslateTransformChainItem() { Vector = Vector };
        }

        internal override void RestoreXml(XElement elem)
        {
            Vector = Helpers.ParseVector(elem.Attribute("vec").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<translate vec=\"{Vector.X};{Vector.Y};{Vector.Z}\"/>");
        }
    }
}
