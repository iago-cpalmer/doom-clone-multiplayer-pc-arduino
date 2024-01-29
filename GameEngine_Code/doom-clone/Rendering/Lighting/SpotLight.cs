using OpenTK.Mathematics;

namespace doom_clone.Rendering.Lighting
{
    public struct SpotLight
    {
        public Vector3 Position;
        public float Cutoff;
        public Vector3 Direction;
        public float OuterCutoff;
        public Vector3 AmbientColor;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;

        public SpotLight(Vector3 position, Vector3 direction, float cutoff, float outerCutoff, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor)
        {
            Position = position;
            Direction = direction;
            Cutoff = cutoff;
            OuterCutoff = outerCutoff;
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
        }
    }
}
