using System;
using OpenTK;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    [XmlName(XmlName = "rotate")]
    public class RotationTransformChainItem : TransformationChainItem
    {
        public Vector3d Axis = Vector3d.UnitZ;
        public double Angle { get; set; }
        public override Matrix4d Matrix()
        {
            return Matrix4d.Rotate(Axis, Angle * Math.PI / 180);
        }

        internal override TransformationChainItem Clone()
        {
            return new RotationTransformChainItem() { Axis = Axis, Angle = Angle };
        }

        internal override void RestoreXml(XElement elem)
        {
            Axis = Helpers.ParseVector(elem.Attribute("axis").Value);
            Angle = Helpers.ParseDouble(elem.Attribute("angle").Value);
        }

        internal override void StoreXml(TextWriter writer)
        {
            writer.WriteLine($"<rotate axis=\"{Axis.X};{Axis.Y};{Axis.Z}\" angle=\"{Angle}\"/>");

        }
    }
}
