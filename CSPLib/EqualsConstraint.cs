using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    [XmlName(XmlName = "equalsConstraint")]
    public class EqualsConstraint : DraftConstraint, IXmlStorable
    {
        public DraftLine SourceLine;
        public DraftLine TargetLine;
        public EqualsConstraint(DraftLine target, DraftLine source, Draft parent) : base(parent)
        {
            SourceLine = source;
            TargetLine = target;
        }

        public EqualsConstraint(XElement el, Draft parent) : base(parent)
        {
            TargetLine = parent.Elements.OfType<DraftLine>().First(z => z.Id == int.Parse(el.Attribute("targetId").Value));
            SourceLine = parent.Elements.OfType<DraftLine>().First(z => z.Id == int.Parse(el.Attribute("sourceId").Value));
        }

        public override bool IsSatisfied(float eps = 1E-06F)
        {
            return Math.Abs(TargetLine.Length - SourceLine.Length) < eps;
        }

        ChangeCand[] GetCands()
        {
            List<ChangeCand> ret = new List<ChangeCand>();
            var dir = TargetLine.Dir;
            ret.Add(new ChangeCand() { Point = TargetLine.V0, Position = TargetLine.V1.Location + SourceLine.Length * (-dir) });
            ret.Add(new ChangeCand() { Point = TargetLine.V1, Position = TargetLine.V0.Location + SourceLine.Length * dir });
            return ret.Where(z => !z.Point.Frozen).ToArray();
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var cc = GetCands();
            var ar = cc.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            var fr = ar.First();
            fr.Apply();
        }

        public bool IsSame(EqualsConstraint cc)
        {
            return cc.TargetLine == TargetLine && cc.SourceLine == SourceLine;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return TargetLine == de || TargetLine.V0 == de || TargetLine.V1 == de || SourceLine == de || SourceLine.V0 == de || SourceLine.V1 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<equalsConstraint targetId=\"{TargetLine.Id}\" sourceId=\"{SourceLine.Id}\"/>");
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
