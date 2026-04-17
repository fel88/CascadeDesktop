using FxEngine.Cameras;
using FxEngine.Fonts;
using FxEngine.Shaders;
using OpenTK.Mathematics;
using System.Drawing;

namespace CascadeDesktop
{
    public class GpuDrawingContext
    {
        public Camera Camera;
        Vector3 lightPos = new Vector3(1000f, 1000f, 400.0f);
        Color ModelColor = Color.FromArgb(255, 128, 64);
        Color LightColor = Color.FromArgb(255, 255, 255);
        float _diffuseValue = 0.8f;
        float _ambientValue = 0.8f;

        public FreeTypeTextRenderer TextRenderer;

        public Shader ModelShader;

        public void SetModelShader()
        {
            ModelShader.use();
            // be sure to activate shader when setting uniforms/drawing objects
            ModelShader.use();
            ModelShader.setVec3("light.position", lightPos);
            ModelShader.setVec3("viewPos", Camera.Eye);

            // light properties
            Vector3 lightColor = new Vector3
            {

                X = LightColor.R / 255.0f,
                Y = LightColor.G / 255.0f,
                Z = LightColor.B / 255.0f
            };

            Vector3 diffuseColor = lightColor * new Vector3(_diffuseValue); // decrease the influence
            Vector3 ambientColor = diffuseColor * new Vector3(_ambientValue); // low influence

            ModelShader.setVec3("light.ambient", ambientColor);
            ModelShader.setVec3("light.diffuse", diffuseColor);
            ModelShader.setVec3("light.specular", 1.0f, 1.0f, 1.0f);

            // material properties
            var modelColor = new Vector3
            {
                X = ModelColor.R / 255.0f,
                Y = ModelColor.G / 255.0f,
                Z = ModelColor.B / 255.0f
            };

            ModelShader.setVec3("material.ambient", modelColor.X, modelColor.Y, modelColor.Z);
            ModelShader.setVec3("material.diffuse", modelColor.X, modelColor.Y, modelColor.Z);

            ModelShader.setVec3("material.specular", 0.5f, 0.5f, 0.5f); // specular lighting doesn't have full effect on this object's material
            ModelShader.setFloat("material.shininess", 32.0f);

            // view/projection transformations       
            ModelShader.setMat4("projection", Camera.ProjectionMatrix);
            ModelShader.setMat4("view", Camera.ViewMatrix);

            // world transformation
            Matrix4 model = Matrix4.Identity;
            ModelShader.setMat4("model", model);
        }
    }
}
