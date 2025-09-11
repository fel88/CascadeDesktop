using System;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class DraftLine : DraftElement
    {
        public readonly DraftPoint V0;
        public readonly DraftPoint V1;

        public DraftLine(XElement el, Draft parent) : base(el, parent)
        {
            var v0Id = int.Parse(el.Attribute("v0").Value);
            var v1Id = int.Parse(el.Attribute("v1").Value);
            Dummy = bool.Parse(el.Attribute("dummy").Value);
            V0 = parent.DraftPoints.First(z => z.Id == v0Id);
            V1 = parent.DraftPoints.First(z => z.Id == v1Id);
        }

        public DraftLine(DraftPoint v0, DraftPoint v1, Draft parent) : base(parent)
        {
            this.V0 = v0;
            this.V1 = v1;
        }

        public Vector2d Center => (V0.Location + V1.Location) / 2;
        public Vector2d Dir => (V1.Location - V0.Location).Normalized();
        public Vector2d Normal => new Vector2d(-Dir.Y, Dir.X);
        public double Length => (V1.Location - V0.Location).Length;
        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<line id=\"{Id}\" dummy=\"{Dummy}\" v0=\"{V0.Id}\" v1=\"{V1.Id}\" />");
        }

        public bool ContainsPoint(Vector2d proj)
        {
            return Math.Abs(((V0.Location - proj).Length + (V1.Location - proj).Length) - Length) < 1e-8f;
        }
    }
}
