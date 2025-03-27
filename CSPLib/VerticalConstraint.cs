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
    [XmlName(XmlName = "verticalConstraint")]
    public class VerticalConstraint : DraftConstraint, IXmlStorable
    {
        public DraftLine Line;
        public VerticalConstraint(DraftLine line, Draft parent) : base(parent)
        {
            Line = line;
        }
        public VerticalConstraint(XElement el, Draft parent) : base(parent)
        {
            Line = parent.Elements.OfType<DraftLine>().First(z => z.Id == int.Parse(el.Attribute("targetId").Value));
        }
        public override bool IsSatisfied(float eps = 1E-06F)
        {
            return Math.Abs(Line.V0.X - Line.V1.X) < eps;
        }

        ChangeCand[] GetCands(ConstraintSolverContext ctx)
        {
            List<ChangeCand> ret = new List<ChangeCand>();
            if (!ctx.FreezedPoints.Contains(Line.V0))
                ret.Add(new ChangeCand() { Point = Line.V0, Position = new Vector2d(Line.V1.X, Line.V0.Y) });
            if (!ctx.FreezedPoints.Contains(Line.V1))
                ret.Add(new ChangeCand() { Point = Line.V1, Position = new Vector2d(Line.V0.X, Line.V1.Y) });
            return ret.Where(z => !z.Point.Frozen).ToArray();
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var cc = GetCands(ctx);
            var ar = cc.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            var fr = ar.First();
            fr.Apply();
        }

        public bool IsSame(VerticalConstraint cc)
        {
            return cc.Line == Line;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Line == de || Line.V0 == de || Line.V1 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<verticalConstraint targetId=\"{Line.Id}\"/>");
        }

        public void StoreXml(TextWriter writer)
        {
            Store(writer);
        }

        public void RestoreXml(XElement elem)
        {
            //   var targetId = int.Parse(elem.Attribute("targetId").Value);
            // Line = Line.Parent.Elements.OfType<DraftLine>().First(z => z.Id == targetId);
        }
    }
}
