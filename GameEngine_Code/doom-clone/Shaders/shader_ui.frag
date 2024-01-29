#version 330 core
//#include <Includes/Material.struct>
//#include <Includes/Matrices.ubo>

// Input
in vec2 texCoord;
in vec3 normal;
in vec3 fragPosition;
in vec3 viewDir;
uniform Material material;

// Output
out vec4 outputColor;


void main()
{
    vec4 diffTexture = vec4(texture(material.diffuseTexture, texCoord));
    vec4 specTexture = vec4(texture(material.specularTexture, texCoord));
    vec4 emission = vec4(texture(material.emissionTexture, texCoord));
    vec4 result = diffTexture+specTexture*material.shininess;
    outputColor = vec4((result.xyz + emission.xyz) * material.ambientColor, result.w);
    
}
