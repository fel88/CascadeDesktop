using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using DxfPad;

namespace CSPLib
{
    [XmlName(XmlName = "pointPositionConstraint")]
    public class PointPositionConstraint : DraftConstraint
    {
        public readonly DraftPoint Point;

        Vector2d _location;
        public Vector2d Location
        {
            get => _location; set
            {
                BeforeChanged?.Invoke();
                _location = value;
                Parent.RecalcConstraints();
            }
        }

        public double X
        {
            get => _location.X; set
            {
                _location.X = value;
                Parent.RecalcConstraints();
            }
        }
        public double Y
        {
            get => _location.Y; set
            {
                _location.Y = value;
                Parent.RecalcConstraints();
            }
        }
        public PointPositionConstraint(XElement el, Draft parent) : base(parent)
        {
            if (el.Attribute("id") != null)
                Id = int.Parse(el.Attribute("id").Value);

            Point = parent.Elements.OfType<DraftPoint>().First(z => z.Id == int.Parse(el.Attribute("pointId").Value));
            X = Helpers.ParseDouble(el.Attribute("x").Value);
            Y = Helpers.ParseDouble(el.Attribute("y").Value);
        }

        public PointPositionConstraint(DraftPoint draftPoint1, Draft parent) : base(parent)
        {
            this.Point = draftPoint1;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            return (Point.Location - Location).Length < eps;
        }

        internal void Update()
        {
            //Point.SetLocation(Location);
            var top = Point.Parent.Constraints.OfType<TopologyConstraint>().FirstOrDefault();
            var dir = Location - Point.Location;
            if (top != null)
            {
                //whole draft translate
                var d = Point.Parent;
                foreach (var item in d.DraftPoints)
                {
                    item.SetLocation(item.Location + dir);
                }
            }
            else
                Point.SetLocation(Location);
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            Update();
        }

        public bool IsSame(PointPositionConstraint cc)
        {
            return cc.Point == Point;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Point == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<pointPositionConstraint id=\"{Id}\" pointId=\"{Point.Id}\" x=\"{X}\" y=\"{Y}\"/>");
        }
    }
}
