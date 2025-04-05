using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using CSPLib.Interfaces;

namespace CSPLib
{
    public abstract class AbstractDrawable : IDrawable
    {
        public static IMessageReporter MessageReporter;
        public AbstractDrawable()
        {
            Id = FactoryHelper.NewId++;
        }
        public bool Frozen { get; set; }

        public AbstractDrawable(XElement item)
        {
            if (item.Attribute("id") != null)
            {
                Id = int.Parse(item.Attribute("id").Value);
                FactoryHelper.NewId = Math.Max(FactoryHelper.NewId, Id + 1);
            }
            else
            {
                Id = FactoryHelper.NewId++;
            }
        }
        public string Name { get; set; }

        public abstract void Draw();

        public virtual void RemoveChild(IDrawable dd)
        {
            Childs.Remove(dd);
        }

        public virtual void Store(TextWriter writer)
        {

        }

        public virtual IDrawable[] GetAll(Predicate<IDrawable> p)
        {
            if (Childs.Count == 0)
                return new[] { this };
            return new[] { this }.Union(Childs.SelectMany(z => z.GetAll(p))).ToArray();
        }

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }

        public List<IDrawable> Childs { get; set; } = new List<IDrawable>();

        public IDrawable Parent { get; set; }
        public int Id { get; set; }

        protected TransformationChain _matrix = new TransformationChain();
        public TransformationChain Matrix { get => _matrix; set => _matrix = value; }
        public int Z { get; set; }
    }
}
