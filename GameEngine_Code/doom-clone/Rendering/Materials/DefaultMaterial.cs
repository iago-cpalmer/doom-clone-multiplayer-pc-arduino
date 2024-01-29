using OpenTK.Mathematics;
using doom_clone.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doom_clone.Rendering.Materials
{
    public class DefaultMaterial : Material
    {
        public DefaultMaterial(Shader shader, string texturePath, string specularTexturePath, string emissionTexturePath, Vector3 ambientColor, float shininess) : base(shader, texturePath, specularTexturePath, emissionTexturePath, ambientColor, shininess)
        {
        }
    }
}
