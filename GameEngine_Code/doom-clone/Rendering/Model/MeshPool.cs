using doom_clone.Rendering.Materials;
using System.Collections.Generic;

namespace doom_clone.Rendering.Model
{
    public class MeshPool
    {
        private static List<Mesh> _meshPool;

        public static Mesh GetMesh(int vertexSize, int indexSize, Material material)
        {
            if(_meshPool==null)
            {
                _meshPool = new List<Mesh>();
            }
            Mesh minMesh = null;
            int meshIndex = -1;
            for(int i = 0; i < _meshPool.Count; i++)
            {
                //Console.WriteLine(_meshPool[i] == null || _meshPool[i].Vertices==null);
                if(_meshPool[i]!=null && _meshPool[i].VertexCount >= vertexSize && _meshPool[i].IndexCount >= indexSize && 
                    (minMesh==null || minMesh.VertexCount > _meshPool[i].VertexCount || minMesh.IndexCount > _meshPool[i].IndexCount))
                {
                    minMesh = _meshPool[i];
                    meshIndex = i;
                }
            }
            if(minMesh==null)
            {
                // No mesh found with that size, we must create one
                return new Mesh(Meshes.PLANE_MESH, material);
            }
            _meshPool.RemoveAt(meshIndex);
            minMesh.Material = material;
            return minMesh;
        }

        public static void AddMesh(Mesh mesh)
        {
            _meshPool.Add(mesh);
        }
    }
}
