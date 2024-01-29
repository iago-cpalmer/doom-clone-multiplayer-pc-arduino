using OpenTK.Mathematics;

namespace doom_clone.Rendering.Lighting
{
    public struct PointLight
    {
        public Vector3 Position;
        public float constant;
        public Vector3 AmbientColor;
        public float linear;
        public Vector3 DiffuseColor;
        public float quadratic;
        public Vector3 SpecularColor;
        public float Intensity;

        public PointLight(Vector3 position, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float constant, float linear, float quadratic, float intensity)
        {
            Position = position;
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            this.constant = constant;
            this.linear = linear;
            this.quadratic = quadratic;
            Intensity = intensity;
        }
    }
}
