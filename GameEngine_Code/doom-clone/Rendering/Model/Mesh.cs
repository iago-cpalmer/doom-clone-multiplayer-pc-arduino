using OpenTK.Graphics.OpenGL4;
using doom_clone.Rendering.Materials;
using System;
using OpenTK.Mathematics;

namespace doom_clone.Rendering.Model
{
    public class Mesh
    {
        public int VboId { get { return _vboId; } }
        public int VaoId { get { return _vaoId; } }
        public int EboId { get { return _eboId; } }
        public int IndexCount { get { return _indexCount; } }
        public int VertexCount { get { return _vertexCount; } }
        public Material Material {  get { return _material; } set { _material = value; } }

        private int _vboId;
        private int _vaoId;
        private int _eboId;
        private int _indexCount;
        private int _vertexCount;
        private Material _material;

        public Mesh(float[] vertices, uint[] indices, Material material)
        {
            _material = material;
            _vertexCount = vertices.Length;
            // VAO
            _vaoId = GL.GenVertexArray();
            GL.BindVertexArray(_vaoId);

            // VBO
            _vboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // EBO
            _indexCount = indices.Length;
            _eboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                int vertexLocation = _material.Shader.GetAttribLocation("aPosition");
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                // Normals
                int normalsLocation = _material.Shader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalsLocation);
                GL.VertexAttribPointer(normalsLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                // Color
                int textureLocation = _material.Shader.GetAttribLocation("aTexCoord");
                GL.EnableVertexAttribArray(textureLocation);
                GL.VertexAttribPointer(textureLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            

        }
        public Mesh(ObjectMesh mesh, Material material)
        {
            _material = material;
            _vertexCount = mesh.Faces[0].Vertices.Length;
            // VAO
            _vaoId = GL.GenVertexArray();
            GL.BindVertexArray(_vaoId);

            // VBO
            _vboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Faces[0].Vertices.Length * sizeof(float), mesh.Faces[0].Vertices, BufferUsageHint.StaticDraw);

            // EBO
            _indexCount = mesh.Faces[0].Indices.Length;
            _eboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Faces[0].Indices.Length * sizeof(uint), mesh.Faces[0].Indices, BufferUsageHint.StaticDraw);


            int vertexLocation = _material.Shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // Normals
            int normalsLocation = _material.Shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalsLocation);
            GL.VertexAttribPointer(normalsLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            // Color
            int textureLocation = _material.Shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(textureLocation);
            GL.VertexAttribPointer(textureLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        }
          

        public Mesh(string path)
        {
            throw new NotImplementedException("Mesh constructor from file path not implemented yet!");
        }
        public static Mesh CreatePlane(Vector2 a, Vector2 b, Material material)
        {
            float[] vertices = new float[]
            {
                a.X, 0f, a.Y,
                0.0f, 1.0f, 0.0f,
                0f, 0f,

                a.X, 0, b.Y,
                0.0f, 1.0f, 0.0f,
                0.0f, (a.Y- b.Y)/10,

                b.X, 0f, a.Y,
                0.0f, 1.0f, 0.0f,
                (a.X- b.X)/10, 0.0f,

                b.X, 0f, b.Y,
                0.0f, 1.0f, 0.0f,
                (a.X- b.X)/10, (a.Y- b.Y)/10
            };
            uint[] indices = new uint[]
            {
                0, 3, 2,
                0, 1, 3
            };
            return new Mesh(vertices, indices, material);
        }

        public static Mesh CreateYPlane(Vector2 a, Vector2 b, Material material)
        {
            float[] vertices = new float[]
            {
                a.X, a.Y, 0.0f,
                0.0f, 1.0f, 0.0f,
                0f, 0f,

                a.X, b.Y,0,
                0.0f, 1.0f, 0.0f,
                0.0f, 1,

                b.X, a.Y, 0,
                0.0f, 1.0f, 0.0f,
                1, 0.0f,

                b.X, b.Y, 0,
                0.0f, 1.0f, 0.0f,
                1, 1
            };
            uint[] indices = new uint[]
            {
                0, 3, 2,
                0, 1, 3
            };
            return new Mesh(vertices, indices, material);
        }
    }


}
