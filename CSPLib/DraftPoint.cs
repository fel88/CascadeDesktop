using OpenTK;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class DraftPoint : DraftElement
    {
        public Vector2d _location;
        public Vector2d Location { get => _location; private set => _location = value; }

        public double X
        {
            get => Location.X;
            set
            {
                _location.X = value;
                Parent.RecalcConstraints();
            }
        }
        public double Y
        {
            get => Location.Y;
            set
            {
                _location.Y = value;
                Parent.RecalcConstraints();
            }
        }

        public DraftPoint(Draft parent, float x, float y) : base(parent)
        {

            Location = new Vector2d(x, y);
        }
        public DraftPoint(Draft parent, double x, double y) : base(parent)
        {
            Location = new Vector2d(x, y);
        }

        public DraftPoint(XElement item2, Draft d) : base(item2, d)
        {
            X = double.Parse(item2.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            Y = double.Parse(item2.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            if (item2.Attribute("frozen") != null)
                Frozen = bool.Parse(item2.Attribute("frozen").Value);
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<point id=\"{Id}\" x=\"{X}\" y=\"{Y}\" frozen=\"{Frozen}\" />");
        }

        public void SetLocation(double x, double y)
        {
            SetLocation(new Vector2d(x, y));
        }
        public void SetLocation(Vector2d vector2d)
        {
            if (Frozen)
            {
                throw new LiteCadException("try update frozen");
            }
            _location = vector2d;
        }
    }
}
