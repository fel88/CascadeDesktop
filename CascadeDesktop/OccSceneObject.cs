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
            /*var demoFile = ctx.Zip.CreateEntry("foo.txt");

            using (var entryStream = demoFile.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write("Bar!");
            }*/
        }

        public enum TransparencyLevel
        {
            None, Half, Full
        }

        public TransparencyLevel Transparency;
    }

}
