using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public abstract class HelperItem : AbstractHelperItem
    {
        public HelperItem() { }
        public HelperItem(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;
        }

        public override RectangleF? BoundingBox()
        {
            throw new NotImplementedException();
        }

        protected Vector3d parseVector(Vector3d defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Element(key);
            if (nrm == null)
            {
                if (required)
                    throw new ArgumentException();
                return defValue;
            }
            var pos = nrm.Attribute("pos").Value.Split(new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."),
                CultureInfo.InvariantCulture)).ToArray();
            return new Vector3d(pos[0], pos[1], pos[2]);
        }
        protected bool parseBool(bool defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Attribute(key);
            if (nrm == null)
            {
                if (required)
                    throw new ArgumentException();
                return defValue;
            }
            return bool.Parse(nrm.Value);
        }
        protected double parseDouble(double defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Attribute(key);
            if (nrm == null)
            {
                if (required)
                    throw new ArgumentException();
                return defValue;
            }
            return StaticHelpers.ParseDouble(nrm.Value);
        }



        public virtual void MoveTo(Vector3d vector)
        {

        }
    }
}
