using System.Collections.Generic;

namespace Cascade.Common
{
    public class Blueprint
    {
        public string Name;
        public List<BlueprintContour> Contours = new List<BlueprintContour>();
        public List<BlueprintItem> Items = new List<BlueprintItem>();
    }
}
