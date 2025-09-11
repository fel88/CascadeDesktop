using System;
using System.Collections.Generic;
using OpenTK;
using System.IO;
using System.Xml.Linq;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class DraftEllipse : DraftElement
    {
        public readonly DraftPoint Center;
        public double X { get => Center.Location.X; set => Center.SetLocation(new Vector2d(value, Center.Y)); }
        public double Y { get => Center.Location.Y; set => Center.SetLocation(new Vector2d(Center.X, value)); }
        decimal _radius { get; set; }
        public decimal Radius { get => _radius; set => _radius = value; }
        public decimal Diameter { get => 2 * _radius; set => _radius = value / 2; }
        public bool SpecificAngles { get; set; }
        public int Angles { get; set; }
        public DraftEllipse(DraftPoint center, decimal radius, Draft parent)
            : base(parent)
        {
            this.Center = center;
            this.Radius = radius;
        }
        public DraftEllipse(XElement elem, Draft parent)
          : base(elem, parent)
        {
            var c = Helpers.ParseVector2(elem.Attribute("center").Value);
            Center = new DraftPoint(parent, c.X, c.Y);
            Radius = Helpers.ParseDecimal(elem.Attribute("radius").Value);
            if (elem.Attribute("angles") != null)
                Angles = int.Parse(elem.Attribute("angles").Value);
            if (elem.Attribute("specificAngles") != null)
                SpecificAngles = bool.Parse(elem.Attribute("specificAngles").Value);
        }

        internal decimal CutLength()
        {
            return (2 * (decimal)Math.PI * Radius);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<ellipse id=\"{Id}\" angles=\"{Angles}\" specificAngles=\"{SpecificAngles}\" center=\"{Center.X}; {Center.Y}\" radius=\"{Radius}\">");
            writer.WriteLine("</ellipse>");
        }

        public Vector2d[] GetPoints()
        {
            var step = 360f / Angles;
            List<Vector2d> pp = new List<Vector2d>();
            for (int i = 0; i < Angles; i++)
            {
                var ang = step * i;
                var radd = ang * Math.PI / 180f;
                var xx = Center.X + (double)Radius * Math.Cos(radd);
                var yy = Center.Y + (double)Radius * Math.Sin(radd);
                pp.Add(new Vector2d(xx, yy));
            }
            return pp.ToArray();
        }
    }
}
