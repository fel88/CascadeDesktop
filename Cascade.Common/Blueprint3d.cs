using System.Collections.Generic;

namespace Cascade.Common
{
    public class Blueprint3d
    {
        public string Name;
        public List<BlueprintContour3d> Contours = new List<BlueprintContour3d>();
        public List<BlueprintItem3d> Items = new List<BlueprintItem3d>();
    }
}
