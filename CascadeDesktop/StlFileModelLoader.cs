using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CascadeDesktop.MeshModel;

namespace CascadeDesktop
{
    public class StlFileModelLoader 
    {
        public static MeshModel ParseFile(string fileName)
        {
            MeshModel ret = new MeshModel();
            bool isBinary = true;
            using (var file = File.OpenRead(fileName))
            using (var reader = new StreamReader(file))
            {
                if (reader.ReadLine().StartsWith("solid"))
                    isBinary = false;
            }
            if (!isBinary)
            {
                //text format
                TriangleInfo tr = null;
                Vector3d normal = Vector3d.Zero;
                using var file = File.OpenRead(fileName);
                using var reader = new StreamReader(file);
                List<VertexInfo> verts = new List<VertexInfo>();
                while (!reader.EndOfStream)
                {
                    var item = reader.ReadLine();

                    var line = item.Trim().ToLower();
                    if (line.StartsWith("facet"))
                    {
                        var spl = line.Split([' '], StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(2).Select(z => z.ToDouble()).ToArray();
                        normal = new Vector3d(db[0], db[1], db[2]);
                        tr = new TriangleInfo();
                        //mm.Mesh.Triangles.Add(tr);
                    }
                    if (line.StartsWith("endfacet"))
                    {
                        tr.Vertices = verts.ToArray();
                        verts.Clear();
                    }
                    if (line.StartsWith("vertex"))
                    {
                        var spl = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(1).Select(z => z.ToDouble()).ToArray();
                        verts.Add(new VertexInfo()
                        {
                            Normal = normal,
                            Position = new Vector3d(db[0], db[1], db[2])
                        });

                    }
                }
            }
            else
            {
                using (var rdr = File.OpenRead(fileName))
                {
                    byte[] data = new byte[50];
                    rdr.Seek(80, SeekOrigin.Begin);
                    rdr.Read(data, 0, 4);
                    var cnt = BitConverter.ToInt32(data, 0);
                    for (int i = 0; i < cnt; i++)
                    {
                        Vector3d normal = new Vector3d();
                        Vector3d v1 = new Vector3d();
                        Vector3d v2 = new Vector3d();
                        Vector3d v3 = new Vector3d();
                        rdr.Read(data, 0, 50);

                        for (int j = 0; j < 3; j++)
                            normal[j] = BitConverter.ToSingle(data, j * 4);

                        for (int j = 0; j < 3; j++)
                            v1[j] = BitConverter.ToSingle(data, 12 + j * 4);

                        for (int j = 0; j < 3; j++)
                            v2[j] = BitConverter.ToSingle(data, 24 + j * 4);

                        for (int j = 0; j < 3; j++)
                            v3[j] = BitConverter.ToSingle(data, 36 + j * 4);
                        
                        ret.Points.AddRange([v1, v2, v3]);
                        ret.Normals.AddRange([normal, normal, normal]);
                        ret.Faces.Add(new Face(ret, [i * 3, i * 3 + 1, i * 3 + 2]));

                    }
                }
            }
            return ret;
        }

    }
}
