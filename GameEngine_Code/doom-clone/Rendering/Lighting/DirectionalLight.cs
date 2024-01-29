using OpenTK.Mathematics;

namespace doom_clone.Rendering.Lighting
{
    public struct DirectionalLight
    {
        public Vector3 Direction;
        public Vector3 AmbientColor;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;

        public DirectionalLight(Vector3 direction, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor)
        {
            Direction = direction;
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
        }
    }
}
