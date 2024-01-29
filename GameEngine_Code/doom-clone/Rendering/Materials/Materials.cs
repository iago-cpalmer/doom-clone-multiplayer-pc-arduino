using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using doom_clone.Rendering.Shaders;

namespace doom_clone.Rendering.Materials
{
    public class Materials
    {
        public static readonly Material WALL_MATERIAL;
        public static readonly Material FLOOR_MATERIAL;
        public static readonly Material UI_MATERIAL;
        public static readonly Material PLAYER_MATERIAL;
        public static readonly Material ENEMY_MATERIAL;
        public static readonly Material BULLET_MATERIAL;
        public static readonly Material HEALTH_MATERIAL;
        static Materials()
        {
            WALL_MATERIAL = new DefaultMaterial(Shaders.Shaders.DEFAULT_SHADER, texturePath: Textures.Textures.WALL_TEXTURE_PATH, specularTexturePath:Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath:Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            FLOOR_MATERIAL = new DefaultMaterial(Shaders.Shaders.DEFAULT_SHADER, texturePath: Textures.Textures.FLOOR_TEXTURE_PATH, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            UI_MATERIAL = new DefaultMaterial(Shaders.Shaders.DEFAULT_SHADER, texturePath: Textures.Textures.BLACK_TEXTURE_PATH, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);

            // TODO: Instead of default shader, the shader will have to be a billboard shader?
            // TODO: Change texture to player texture
            PLAYER_MATERIAL = new DefaultMaterial(Shaders.Shaders.DEFAULT_SHADER, texturePath: Textures.Textures.PLAYER_TEXTURE, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            // TODO: Change texture to enemy texture
            ENEMY_MATERIAL = new DefaultMaterial(Shaders.Shaders.DEFAULT_SHADER, texturePath: Textures.Textures.EMPTY_TEXTURE_PATH, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            BULLET_MATERIAL = new DefaultMaterial(Shaders.Shaders.UI_SHADER, texturePath: Textures.Textures.BULLET_FULL_ICON_PATH, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
            HEALTH_MATERIAL = new DefaultMaterial(Shaders.Shaders.UI_SHADER, texturePath: Textures.Textures.HEART_FULL_ICON_PATH, specularTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
        }
    }
}
