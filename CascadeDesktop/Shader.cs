using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CascadeDesktop
{
    public class Shader
    {
        public static string ReadResourceTxt(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public Shader()
        {

        }

        public Shader InitFromResources(string vShaderResourceName, string fShaderResourceName)
        {
            var vShader = ReadResourceTxt(vShaderResourceName);
            var fShader = ReadResourceTxt(fShaderResourceName);
            InitFromShaderCodes(vShader, fShader);
            return this;
        }

        public Shader InitFromShaderCodes(string vShaderCode, string fShaderCode)
        {
            // 2. compile shaders
            int vertex, fragment;
            // vertex shader

            vertex = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertex, vShaderCode);
            GL.CompileShader(vertex);
            checkCompileErrors(vertex, "VERTEX");
            // fragment Shader
            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fShaderCode);
            GL.CompileShader(fragment);
            checkCompileErrors(fragment, "FRAGMENT");
            // shader Program
            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertex);
            GL.AttachShader(ID, fragment);
            GL.LinkProgram(ID);
            checkCompileErrors(ID, "PROGRAM");
            // delete the shaders as they're linked into our program now and no longer necessary
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
            return this;
        }

        int ID;

        void checkCompileErrors(int shader, string type)
        {
            int success;
            string infoLog;
            if (type != "PROGRAM")
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(shader, out infoLog);
                    //std::cout << "ERROR::SHADER_COMPILATION_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
            else
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    GL.GetProgramInfoLog(shader, out infoLog);
                    throw new Exception(infoLog);
                    //  std::cout << "ERROR::PROGRAM_LINKING_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
        }
        public void setInt(string v1, int v2)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v1), v2);
        }
        public void setFloat(string v1, float v2)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v1), v2);
        }

        public void setMat4(string v, Matrix4 projection)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(ID, v), false, ref projection);
        }
        public void setMat4(string v, Matrix4d projection)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(ID, v), false, ref projection);
        }
        public void setVec3(string v, Vector3 newPos)
        {
            GL.Uniform3(GL.GetUniformLocation(ID, v), ref newPos);
        }
        public void setVec4(string v, Vector4 newPos)
        {
            GL.Uniform4(GL.GetUniformLocation(ID, v), ref newPos);
        }
        public void setVec3(string v, Vector3d newPos)
        {
            setVec3(v, newPos.ToVector3());
        }
        public void use()
        {
            GL.UseProgram(ID);
        }

        public void setVec3(string v1, float v2, float v3, float v4)
        {
            setVec3(v1, new Vector3(v2, v3, v4));
        }
    }
}
