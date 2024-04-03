using System;
using System.IO;
using System.IO.Compression;

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

        internal virtual void StoreToZip(StoreZipContext ctx)
        {
            //store xml with description
            //store file
            //autonomous or external file
            var file = ctx.Zip.CreateEntry($"{Name}.model");             

            var r = Proxy.ExportStepStream(Handle).ToArray();
            //ctx..ExportStep(proxy.GetSelectedObject(), sfd.FileName);
            MemoryStream ms = new MemoryStream();
            ms.Write(r, 0, r.Length);
            ms.Seek(0,SeekOrigin.Begin);
            using (var entryStream = file.Open())
            //using (var streamWriter = new StreamWriter(entryStream))
            {
                ms.CopyTo(entryStream);                
            }
        }

        public enum TransparencyLevel
        {
            None, Half, Full
        }

        public TransparencyLevel Transparency;
    }

}
