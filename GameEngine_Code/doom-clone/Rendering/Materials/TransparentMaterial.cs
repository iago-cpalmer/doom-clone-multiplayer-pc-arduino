using OpenTK.Mathematics;
using doom_clone.Rendering.Shaders;

namespace doom_clone.Rendering.Materials
{
    public class TransparentMaterial : Material
    {
        private float _transparency;
        public TransparentMaterial(Shader shader, string texturePath, string specularTexturePath, string emissionTexturePath, Vector3 ambientColor, float shininess, float transparency) : base(shader, texturePath, specularTexturePath, emissionTexturePath, ambientColor, shininess)
        {
            _transparency = transparency;
        }

        public override void BindMaterial()
        {
            base.BindMaterial();
            Shader.SetFloat("transparency", _transparency);
        }
    }
}
