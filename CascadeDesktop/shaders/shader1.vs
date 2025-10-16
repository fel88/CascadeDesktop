 // Vertex Shader
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec4 color;
 //out vec4 vColor;
uniform vec3 lightPos; // we now define the uniform in the vertex shader and pass the 'view space' lightpos to the fragment shader. lightPos is currently in world space.


out vec3 FragPos;
out vec3 Normal;
out vec3 LightPos;
    
    void main()
    {
//vColor = color;
  gl_Position = projection * view * model * vec4(aPos, 1.0);
    Normal = mat3(transpose(inverse(view * model))) * aNormal;
      //gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
    LightPos = vec3(view * vec4(lightPos, 1.0)); // Transform world-space light position to view-space light position

    }
