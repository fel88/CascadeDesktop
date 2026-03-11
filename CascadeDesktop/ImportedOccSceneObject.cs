using OCCTProxy.Common.Interfaces;
using System;

namespace CascadeDesktop
{
    public class ImportedOccSceneObject : OccSceneObject
    {
        public readonly string Path;

        public ImportedOccSceneObject(string path, IManagedObjHandle h, IOCCTProxyInterface proxy) : base(h, proxy)
        {
            Path = path;
        }
    }

}
