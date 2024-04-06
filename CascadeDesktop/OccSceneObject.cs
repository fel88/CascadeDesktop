using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public class OccSceneObject : SceneObject
    {
        readonly IOCCTProxyInterface Proxy;
        public readonly ManagedObjHandle Handle;

        public bool IsEquals(ManagedObjHandle h)
        {
            return h.Handle == Handle.Handle;
        }

        public OccSceneObject(ManagedObjHandle h, IOCCTProxyInterface proxy)
        {
            Handle = h;
            Proxy = proxy;
        }
        public void SetTransparency(TransparencyLevel t)
        {
            Transparency = t;
            switch (Transparency)
            {
                case TransparencyLevel.None:
                    Proxy.SetTransparency(Handle, 0.0);
                    break;
                case TransparencyLevel.Half:
                    Proxy.SetTransparency(Handle, 0.5);
                    break;
                case TransparencyLevel.Full:
                    Proxy.SetTransparency(Handle, 1.0);
                    break;
                default:
                    break;
            }
        }

        public void SwitchTransparency()
        {
            switch (Transparency)
            {
                case TransparencyLevel.None:
                    Transparency = TransparencyLevel.Half;
                    Proxy.SetTransparency(Handle, 0.5);
                    break;
                case TransparencyLevel.Half:
                    Transparency = TransparencyLevel.Full;
                    Proxy.SetTransparency(Handle, 1.0);

                    break;
                case TransparencyLevel.Full:
                    Transparency = TransparencyLevel.None;
                    Proxy.SetTransparency(Handle, 0.0);


                    break;
                default:
                    break;
            }
        }

        internal void Remove()
        {
            Proxy.Erase(Handle);
        }

        string GetXml(string name, string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<root>");
            var tr = Proxy.GetObjectMatrixValues(Handle);

            sb.Append($"<model name=\"{name}\" path=\"{path}\" color=\"{Color.R};{Color.G};{Color.G}\" transparency=\"{Transparency}\" ");
            sb.Append($"matrix=\"{string.Join(";", tr)}\"");
            sb.AppendLine("/>");

            sb.AppendLine("</root>");
            return sb.ToString();
        }

        internal virtual void StoreToZip(IOZipContext ctx)
        {
            //store xml with description
            //store file
            //autonomous or external file
            var prefixName = Name;
            if (string.IsNullOrEmpty(Name))
            {
                prefixName = "unknown";
            }
            var name = $"{prefixName}.model";
            int counter = 0;
            while (ctx.Zip.Entries.Any(z => z.Name == name))
            {
                name = $"{prefixName}_{counter++}.model";
            }

            var xml = GetXml(Name, name);
            var xfile = ctx.Zip.CreateEntry($"info_{ctx.ModelIdx++}.xml");
            using (var entryStream = xfile.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write(xml);
            }


            var file = ctx.Zip.CreateEntry(name);

            var r = Proxy.ExportStepStream(Handle).ToArray();
            //ctx..ExportStep(proxy.GetSelectedObject(), sfd.FileName);
            MemoryStream ms = new MemoryStream();
            ms.Write(r, 0, r.Length);
            ms.Seek(0, SeekOrigin.Begin);
            using (var entryStream = file.Open())
            //using (var streamWriter = new StreamWriter(entryStream))
            {
                ms.CopyTo(entryStream);
            }
        }

        public Color Color { get; private set; }
        internal void SetColor(Color color)
        {
            Color = color;
            Proxy.SetColor(Handle, color.R, color.G, color.B);
        }

        internal void SetMatrix(double[] doubles)
        {
            Proxy.SetMatrixValues(Handle, doubles.ToList());
        }

        public enum TransparencyLevel
        {
            None, Half, Full
        }

        public TransparencyLevel Transparency;
    }

}
