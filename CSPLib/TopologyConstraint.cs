using System;
using System.Collections.Generic;
using System.Data;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using CSPLib;

namespace DxfPad
{
    [XmlName(XmlName = "topologyConstraint")]
    public class TopologyConstraint : DraftConstraint
    {
        public readonly TopologyDraftLineInfo[] Lines;

        public TopologyConstraint(XElement el, Draft parent) : base(parent)
        {
            if (el.Attribute("id") != null)
                Id = int.Parse(el.Attribute("id").Value);

            List<TopologyDraftLineInfo> infos = new List<TopologyDraftLineInfo>();
            foreach (var line in el.Elements("item"))
            {
                var id = int.Parse(line.Attribute("id").Value);
                var dirr = (line.Attribute("dir").Value).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                infos.Add(new TopologyDraftLineInfo() { Line = parent.DraftLines.First(z => z.Id == id), Dir = new Vector2d(Helpers.ParseDouble(dirr[0]), Helpers.ParseDouble(dirr[1])) });
            }
            //Point = parent.Elements.OfType<DraftPoint>().First(z => z.Id == int.Parse(el.Attribute("pointId").Value));
            Lines = infos.ToArray();
        }

        public TopologyConstraint(DraftLine[] draftPoint1, Draft parent) : base(parent)
        {
            Lines = draftPoint1.Select(z => new TopologyDraftLineInfo() { Line = z, Dir = z.Dir }).ToArray();
            //this.Point = draftPoint1;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            foreach (var item in Lines)
            {
                var dot = Vector2d.Dot(item.Line.Dir, item.Dir);
                if (Math.Abs(dot - 1) > eps)
                {
                    return false;
                }
            }
            return true;
            //return (Point.Location - Location).Length < eps;
        }

        internal void Update()
        {
            //Point.SetLocation(Location);
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            //Point.SetLocation(Location);
        }

        public bool IsSame(TopologyConstraint cc)
        {
            var id1 = Lines.Select(z => z.Line.Id).ToArray();
            var inter = id1.Intersect(cc.Lines.Select(uu => uu.Line.Id)).ToArray(); ;
            return id1.Length == inter.Length;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Lines.Any(z => z.Line.V0 == de || z.Line.V1 == de || z.Line == de);
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<topologyConstraint id=\"{Id}\" >");
            foreach (var item in Lines)
            {
                writer.WriteLine($"<item id=\"{item.Line.Id}\" dir=\"{item.Dir.X};{item.Dir.Y}\"/>");
            }
            writer.WriteLine($"</topologyConstraint >");
        }
    }
}
