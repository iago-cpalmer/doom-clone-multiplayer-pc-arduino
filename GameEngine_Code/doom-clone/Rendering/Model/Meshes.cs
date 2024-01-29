namespace doom_clone.Rendering.Model
{
    public class Meshes
    {
        public static ObjectMesh PLANE_MESH = new ObjectMesh(new MeshFace[]
        {
            // Front
            new MeshFace(
                vertices: new float[]{
                    0f, 0f,  0f,
                    0f, 1f, 0f,
                    1f, 0f, 0f,
                    1f, 1f, 0f },
                normals: new float[]
                {
                    0.0f, 0.0f, -1.0f,
                },
                uVs: new float[]
                {
                    0f, 0f,
                    0.0f, 1.0f,
                    1.0f, 0.0f,
                    1.0f, 1.0f
                },
                indices:new int[]
                {
                    0, 3, 2,
                    0, 1, 3
                }
                ),
        });
    }
}
