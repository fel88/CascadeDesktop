using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace CascadeDesktop
{
    public class MeshModel
    {
        public class Face
        {
            public Face(MeshModel parent, int[] indices)
            {
                Indices = indices;
                Parent = parent;
                Points = indices.Select(z => parent.Points[z]).ToArray();
                Normals = indices.Select(z => parent.Normals[z]).ToArray();
            }

            public Vector3d[] Points { get; private set; }
            public Vector3d[] Normals { get; private set; }
            public int[] Indices;
            public MeshModel Parent;
        }

        public List<Face> Faces = new List<Face>();
        public List<Vector3d> Points = new List<Vector3d>();
        public List<Vector3d> Normals = new List<Vector3d>();

        public GpuObject ToGpuObject()
        {
            List<Vector3d> triags = new List<Vector3d>();
            List<Vector3d> norms = new List<Vector3d>();
            foreach (var item in Faces)
            {
                for (int i = 0; i < item.Indices.Length; i++)
                {
                    triags.Add(Points[item.Indices[i]]);
                    norms.Add(Normals[item.Indices[i]]);
                }
            }
            return new GpuObject(triags.ToArray(), norms.ToArray());
        }
    }
}
