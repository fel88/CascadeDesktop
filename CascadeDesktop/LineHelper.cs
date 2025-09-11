using Cascade.Common;
using CascadeDesktop.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public class LineHelper : HelperItem, IPointsProvider
    {

        public Vector3d Start;

        public Vector3d End;

        public Vector3d Diff
        {
            get
            {
                return End - Start;
            }
        }


        public Line3D Get3DLine()
        {
            return new Line3D()
            {
                Start = new Vertex(Start.X, Start.Y, Start.Z),
                End = new Vertex(End.X, End.Y, End.Z)
            };
        }

        public LineHelper() { }

        public LineHelper(XElement item) : base(item)
        {
            if (item.Attribute("showCrosses") != null)
                ShowCrosses = bool.Parse(item.Attribute("showCrosses").Value);

            var pos = item.Attribute("start").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Start = new Vector3d(pos[0], pos[1], pos[2]);
            var nrm = item.Attribute("end").Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            End = new Vector3d(nrm[0], nrm[1], nrm[2]);
            if (item.Attribute("drawSize") != null)
                DrawSize = StaticHelpers.ParseFloat(item.Attribute("drawSize").Value);
        }

        public override void MoveTo(Vector3d vector)
        {
            Start += vector;
            End += vector;
        }

        public float DrawSize { get; set; } = 2;

        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public double Length
        {
            get
            {
                return (End - Start).Length;
            }
            set
            {
                End = Start + Dir * value;
            }
        }


        public bool ShowCrosses { get; set; } = true;

        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            if (Selected)
                GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();

            if (ShowCrosses)
            {
                DrawHelpers.DrawCross(Start, DrawSize);
                DrawHelpers.DrawCross(End, DrawSize);
            }
        }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<line name=\"{Name}\" start=\"{Start.X};{Start.Y};{Start.Z}\" end=\"{End.X};{End.Y};{End.Z}\" drawSize=\"{DrawSize}\" showCrosses=\"{ShowCrosses}\"/>");
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            return new[] { Start, End };
        }

    }
}
