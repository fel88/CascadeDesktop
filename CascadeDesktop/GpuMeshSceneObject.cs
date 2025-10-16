using OpenTK.Graphics.OpenGL;
using System.Xml.Linq;

namespace CascadeDesktop
{
    public class GpuMeshSceneObject :ISceneObject
    {

        public void RestoreXml(XElement elem)
        {

        }

      

        protected GpuObject gpuObject;
        public GpuMeshSceneObject()
        {

        }
        

        public GpuMeshSceneObject(GpuObject gpuObject)
        {
            this.gpuObject = gpuObject;
        }

        public bool Wireframe { get; set; }
        public bool Fill { get; set; } = true;
        public int Id { get; set; }

        public  void Draw(GpuDrawingContext ctx)
        {
            
            GL.PushMatrix();
            
            
            ctx.SetModelShader();

            gpuObject.Draw();

            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {


            }
            if (Wireframe)
            {

            }
            GL.PopMatrix();

        }
    }
}
