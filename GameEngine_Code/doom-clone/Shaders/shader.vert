#version 330 core

//#include <Includes/Matrices.ubo>
//#include <Includes/Lights.s>
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;

uniform mat4 model;

out vec2 texCoord;
out vec3 normal;
out vec3 fragPosition;
out vec3 viewDir;
vec3 extractCameraPos(in mat4 viewMatrix);
void main(void)
{
    
    // Update the normals depending on view matrix and model matrix
    //normal = vec3(vec4(aNormal * mat3(transpose(inverse(view * model))), 1.0));
    normal = normalize(mat3(transpose(inverse(view * model))) * aNormal);

    // Texture coordinate
    texCoord = aTexCoord;

    // Update the frag position
    fragPosition = vec3(vec4(aPosition, 1.0) * model * view);
    viewDir = normalize(extractCameraPos(view) - fragPosition);
    // Update the position of the vertex
    gl_Position =  vec4(aPosition, 1.0) * model * view * projection;
}
vec3 extractCameraPos(in mat4 viewMatrix) {
    // Invert the view matrix
    mat4 invViewMatrix = inverse(viewMatrix);

    // Extract the translation (camera position)
    vec3 cameraPos = invViewMatrix[3].xyz;

    return cameraPos;
}