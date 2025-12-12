using OCCT.Interfaces;
using System;

namespace CascadeDesktop
{
    public class ImportedOccSceneObject : OccSceneObject
    {
        public readonly string Path;

        public ImportedOccSceneObject(String path, ManagedObjHandle h, IOCCTProxyInterface proxy) : base(h, proxy)
        {
            Path = path;
        }
    }

}
