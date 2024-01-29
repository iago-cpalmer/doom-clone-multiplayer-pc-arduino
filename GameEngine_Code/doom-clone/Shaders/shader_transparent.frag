#version 330 core
//#include <Includes/Matrices.ubo>
//#include <Includes/Lights.s>
//#include <Includes/Material.struct>
out vec4 outputColor;

in vec2 texCoord;
in vec3 normal;
in vec3 fragPosition;

uniform Material material;
uniform float transparency;
void main()
{   
   /*
    
    vec4 textureColor = texture(material.diffuseTexture, texCoord);
    vec3 specularTextureColor = vec3(texture(material.specularTexture, texCoord));

    // Ambient
    vec3 ambient = ambientColor * textureColor.xyz;
    // Diffuse
    vec3 dirFromLight = vec3(lightPosition - fragPosition);
    dirFromLight = normalize(dirFromLight);
    vec3 normalNormalized = normalize(normal);
    float diff = dot(normalNormalized, dirFromLight);
    vec3 diffuse = diffuseColor * diff * textureColor.xyz;

    // Specular
    vec3 reflection = reflect(-dirFromLight, normalNormalized);
    float spec = pow(max(dot(reflection, normalize(-fragPosition)), 0.0f), 32);
    vec3 specular = specularColor * specularTextureColor * material.shininess * spec;

    vec3 emission = vec3(texture(material.emissionTexture, texCoord));
    

    outputColor = vec4((ambient+diffuse+specular+emission) * material.ambientColor, transparency * textureColor.w);*/
    outputColor = vec4(1.0f);
}