using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using doom_clone.Rendering.Shaders;
using doom_clone.Rendering.Textures;

namespace doom_clone.Rendering.Materials
{
    public abstract class Material
    {
        private Texture _diffuseTexture;
        private Texture _specularTexture;
        private Texture _emissionTexture;
        public Shader Shader;
        private Vector3 _ambientColor;
        private float _shininess;
        public Material(Shader shader, string texturePath, string specularTexturePath, string emissionTexturePath, Vector3 ambientColor, float shininess)
        {
            Shader = shader;
            _diffuseTexture = Texture.LoadFromFile(texturePath);
            _diffuseTexture.Use(TextureUnit.Texture0);

            _specularTexture = Texture.LoadFromFile(specularTexturePath);
            _specularTexture.Use(TextureUnit.Texture1);

            _emissionTexture = Texture.LoadFromFile(emissionTexturePath);
            _emissionTexture.Use(TextureUnit.Texture2);

            Shader.SetInt("material.diffuseTexture", 0);
            Shader.SetInt("material.specularTexture", 1);
            Shader.SetInt("material.emissionTexture", 2);
            _ambientColor = ambientColor;
            _shininess = shininess;
        }

        public virtual void BindMaterial()
        {
            Shader.SetVector3("material.ambientColor", _ambientColor);
            Shader.SetFloat("material.shininess", _shininess);
            _diffuseTexture.Use(TextureUnit.Texture0);
            _specularTexture.Use(TextureUnit.Texture1);
            _emissionTexture.Use(TextureUnit.Texture2);
        }
    }
}
