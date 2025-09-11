using System;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class TransformationChain
    {
        public void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<transformationChain>");
            foreach (var item in Items)
            {
                item.StoreXml(writer);
            }
            writer.WriteLine("</transformationChain>");
        }

        public void RestoreXml(XElement xElement)
        {
            Items.Clear();
            Type[] types = new[] {
                typeof(ScaleTransformChainItem),
                typeof(TranslateTransformChainItem),
                typeof(RotationTransformChainItem)
            };
            foreach (var item in xElement.Element("transformationChain").Elements())
            {
                var fr = types.First(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                var v = Activator.CreateInstance(fr) as TransformationChainItem;
                v.RestoreXml(item);
                Items.Add(v);
            }
        }

        public List<TransformationChainItem> Items = new List<TransformationChainItem>();
        public Matrix4d Calc()
        {
            var r = Matrix4d.Identity;
            foreach (var item in Items)
            {
                r *= item.Matrix();
            }
            return r;
        }

        public TransformationChain Clone()
        {
            TransformationChain ret = new TransformationChain();
            foreach (var item in Items)
            {
                ret.Items.Add(item.Clone());
            }
            return ret;
        }
    }
}
