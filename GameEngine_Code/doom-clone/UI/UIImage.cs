using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Model;
using doom_clone.Rendering.Shaders;
using doom_clone.Rendering.Textures;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace doom_clone.UI
{
    class UIImage : UIElement
    {
        public UIImage(Vector3 position, Vector3 rotation, Vector3 scale, String path) : base(position, rotation, scale)
        {
            Material material = new DefaultMaterial(Shaders.UI_SHADER, texturePath: path, specularTexturePath: Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            Mesh = Mesh.CreateYPlane(Vector2.Zero, Vector2.One, material);
        }
    }
}
