namespace doom_clone.Rendering.Model
{
    public struct ObjectMesh
    {
        public MeshFace[] Faces;

        public ObjectMesh(MeshFace[] faces)
        {
            Faces = faces;
        }
    }

        public struct MeshFace
        {
            public float[] Vertices;
            public float[] Normals;
            public float[] UVs;
            public int[] Indices;

            public MeshFace(float[] vertices, float[] normals, float[] uVs, int[] indices)
            {
                Vertices = vertices;
                Normals = normals;
                UVs = uVs;
                Indices = indices;
            }
        }
    }
