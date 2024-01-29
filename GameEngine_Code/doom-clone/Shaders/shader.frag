#version 330 core
//#include <Includes/Material.struct>
//#include <Includes/Matrices.ubo>

layout (shared) uniform MainLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
// Constants
const float colorStep = 0.08f; // 0.125f^3 = 512 colors
const float PixelsX = 256.0;
const float PixelsY = 512.0;
const float dx = 15.0 * (1.0 / PixelsX);
const float dy = 10.0 * (1.0 / PixelsY);
// Functions
vec3 CalcDirLight(vec3 normal, vec3 fragPos, vec3 diffTexture, vec3 specTexture);
vec3 findNearestPaletteColor(vec3 color);

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
    vec3 norm = normalize(normal);

    // Calculate tiling texture coordinates -> to repeat the texture depending of number of tiles
    vec2 numTiles = abs(floor(texCoord));
    vec2 tilingTexCoords = texCoord;
    float tileSize = 500;
    if(numTiles.xy != vec2(0,0)) 
    {
        tilingTexCoords = texCoord - numTiles;
        
        vec2 flooredTexCoords = floor((texCoord - numTiles) * tileSize)/tileSize;
        numTiles = numTiles + vec2(1,1);

        tilingTexCoords = flooredTexCoords + mod(((tilingTexCoords - flooredTexCoords)*numTiles)*tileSize,1)/tileSize;
    }

    // Pixelize textures
    vec2 Coord = vec2(dx * floor(tilingTexCoords.x / dx),
                      dy * floor(tilingTexCoords.y / dy));

    vec4 diffTexture = vec4(texture(material.diffuseTexture, Coord));
    vec3 specTexture = vec3(texture(material.specularTexture, Coord));
    // phase 1: Directional lighting
    vec3 result = CalcDirLight(norm, fragPosition, diffTexture.xyz, specTexture);

    vec3 emission = vec3(texture(material.emissionTexture, Coord));
    outputColor = vec4((result + emission) * material.ambientColor, 1.0);
    
    // Apply here limited palette
    vec3 nearestColor = findNearestPaletteColor(outputColor.rgb);
    outputColor = vec4(nearestColor, diffTexture.w);
    
}

vec3 CalcDirLight(vec3 normal, vec3 viewDir, vec3 diffTexture, vec3 specTexture) {
    vec3 norm = normalize(normal);
    vec3 lightDir = normalize(direction);

    // Diffuse shading
    float diff = max(dot(norm, lightDir), 0.0);

    // Specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    spec = material.shininess;
    // Combine results
    vec3 ambientComponent  = ambient * diffTexture;
    vec3 diffuseComponent  = diffuse * diff * diffTexture;
    vec3 specularComponent = specular * spec * specTexture;

    return (ambientComponent + diffuseComponent + specularComponent);
}

vec3 findNearestPaletteColor(vec3 color) {
    return round(color.xyz/colorStep)*colorStep;
}

