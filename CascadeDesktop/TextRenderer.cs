using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CascadeDesktop
{
    public class TextRenderer
    {
        Shader shader;
        public void Init(int width, int height)
        {
            shader = new DefaultTextShader();

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Viewport(0, 0, width, height);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);

            shader.use();
            // glUniformMatrix4fv(glGetUniformLocation(shader.ID, "projection"), 1, false, glm::value_ptr(projection));

            shader.setMat4("projection", projection);
            InitFonts();


            // configure VAO/VBO for texture quads
            // -----------------------------------
            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, 0, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
        public static byte[] ReadResourceRaw(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private void InitFonts()
        {
            // FreeType
            // --------
            Library ft = new Library();

            // All functions return a value different than 0 whenever an error occurred
            //if (FT_Init_FreeType(&ft))
            {
                //   std::cout << "ERROR::FREETYPE: Could not init FreeType Library" << std::endl;
                //  return -1;
            }

            // find path to font
            var fontBytes = ReadResourceRaw("OCRAEXT.TTF");

            // load font as face
            SharpFont.Face face = new (ft, fontBytes, 0);
            face.SetPixelSizes(0, 48);
            // set size to load glyphs as
            //FT_Set_Pixel_Sizes(face, 0, 48);

            // disable byte-alignment restriction
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            // load first 128 characters of ASCII set
            for (char c = (char)0; c < 128; c++)
            {
                face.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);
                //   face.Glyph.RenderGlyph(RenderMode.Normal);
                // Load character glyph 
                //if (FT_Load_Char(face, c, FT_LOAD_RENDER))
                {
                    // std::cout << "ERROR::FREETYTPE: Failed to load Glyph" << std::endl;
                    // continue;
                }
                // generate texture
                int texture;
                GL.GenTextures(1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                  PixelInternalFormat.R8,
                      face.Glyph.Bitmap.Width,
                     face.Glyph.Bitmap.Rows,
                    0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Red,
                    PixelType.UnsignedByte,
                      face.Glyph.Bitmap.Buffer
                );

                // set texture options
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                // now store character for later use
                Character character = new Character()
                {
                    TextureID = texture,
                    Size = new Vec2i(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                    Bearing = new Vec2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                    Advance = (face.Glyph.Advance.X.ToInt32())
                };
                Characters.Add(c, character);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // destroy FreeType once we're finished
            //     FT_Done_Face(face);
            //    FT_Done_FreeType(ft);

        }
        struct Vec2i
        {
            public Vec2i(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X;
            public int Y;
        }

        /// Holds all state information relevant to a character as loaded using FreeType
        struct Character
        {
            public int TextureID; // ID handle of the glyph texture
            public Vec2i Size;      // Size of glyph
            public Vec2i Bearing;   // Offset from baseline to left/top of glyph
            public int Advance;   // Horizontal offset to advance to next glyph
        };
        Dictionary<char, Character> Characters = new Dictionary<char, Character>();
        int VAO, VBO;

        // render line of text
        // -------------------
        public void RenderText(string text, float x, float y, float scale, Vector3 color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // activate corresponding render state	
            shader.use();
            //glUniform3f(glGetUniformLocation(shader.ID, "textColor"), color.x, color.y, color.z);
            shader.setVec3("textColor", color);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(VAO);

            // iterate through all characters
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                Character ch = Characters[c];

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;
                // update VBO for each character
                float[,] vertices = new float[6, 4]{
                    {xpos,     ypos + h,   0.0f, 0.0f },
            { xpos,     ypos,       0.0f, 1.0f },
            { xpos + w, ypos,       1.0f, 1.0f },

            { xpos,     ypos + h,   0.0f, 0.0f },
            {xpos + w, ypos, 1.0f, 1.0f },
            {xpos +w, ypos + h,   1.0f, 0.0f}
                };

                // render glyph texture over quad
                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                // update content of VBO memory
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0, 6 * 4 * sizeof(float), vertices);// be sure to use glBufferSubData and not glBufferData

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                // render quad
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                // now advance cursors for next glyph (note that advance is number of 1/64 pixels)
                x += (ch.Advance) * scale; // bitshift by 6 to get value in pixels (2^6 = 64 (divide amount of 1/64th pixels by 64 to get amount of pixels))
            }
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }
    }
}
