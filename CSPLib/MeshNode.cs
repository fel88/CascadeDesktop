using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    public class MeshNode
    {
        public bool Visible { get; set; } = true;
        //public BRepFace Parent;
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        public bool Contains(TriangleInfo tr)
        {
            return Triangles.Any(z => z.IsSame(tr));
        }

        public virtual void SwitchNormal()
        {
            //if (!(Parent.Surface is BRepPlane pl)) return;

            foreach (var item in Triangles)
            {
                foreach (var vv in item.Vertices)
                {
                    vv.Normal *= -1;
                }
            }
        }

        public MeshNode RestoreXml(XElement mesh)
        {
            MeshNode ret = new MeshNode();
            foreach (var tr in mesh.Elements())
            {
                TriangleInfo tt = new TriangleInfo();
                tt.RestoreXml(tr);
                ret.Triangles.Add(tt);
            }
            return ret;
        }

        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<mesh>");
            foreach (var item in Triangles)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</mesh>");
        }

        public bool Contains(TriangleInfo target, Matrix4d mtr1)
        {
            return Triangles.Any(z => z.Multiply(mtr1).IsSame(target));
        }
    }
}
