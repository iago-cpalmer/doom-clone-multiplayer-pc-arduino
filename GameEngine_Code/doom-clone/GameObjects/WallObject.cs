using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Model;
using doom_clone.WorldClasses;
using OpenTK.Mathematics;
namespace doom_clone.GameObjects
{
    class WallObject : GameObject
    {
        public WallObject(Vector3 position, Vector3 rotation, Vector3 scale, Line line)
        : base(position, rotation, scale, CreateMeshFromLine(line))
        {

        }

        private static Mesh CreateMeshFromLine(Line line)
        {
            float[] vertices = new float[]
            {
                line.A.X, 0f, line.A.Y,
                0.0f, 0.0f, -1.0f,
                0f, 0f,

                line.A.X, GameManager.WALL_HEIGHT, line.A.Y,
                0.0f, 0.0f, -1.0f,
                0.0f, 1,

                line.B.X, 0f, line.B.Y,
                0.0f, 0.0f, -1.0f,
                ((line.A.X- line.B.X)+(line.A.Y- line.B.Y))/10, 0.0f,

                line.B.X, GameManager.WALL_HEIGHT, line.B.Y,
                0.0f, 0.0f, -1.0f,
                ((line.A.X- line.B.X)+(line.A.Y- line.B.Y))/10, 1
            };
            uint[] indices = new uint[]
            {
                0, 3, 2,
                0, 1, 3
            };
            return new Mesh(vertices, indices, Materials.WALL_MATERIAL);
        }
    }
}
