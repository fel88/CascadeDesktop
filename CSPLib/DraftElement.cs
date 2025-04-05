using System;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    public abstract class DraftElement
    {
        public int Id { get; set; }
        public bool Frozen { get; set; }//can't be changed during constraints satisfaction        
        public Draft Parent;
        public bool Dummy { get; set; }//dummy line. don't export

        protected DraftElement(Draft parent)
        {
            Parent = parent;
            Id = FactoryHelper.NewId++;
        }
        protected DraftElement(XElement e, Draft parent)
        {
            if (e.Attribute("id") != null)
            {
                Id = int.Parse(e.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
            Parent = parent;
        }

        public abstract void Store(TextWriter writer);

    }
}
