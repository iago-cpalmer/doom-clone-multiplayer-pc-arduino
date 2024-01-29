using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Model;
using OpenTK.Mathematics;

namespace doom_clone.GameObjects
{
    public class Prefabs
    {
        public static GameObject PLAYER_PREFAB;
        public static GameObject MAIN_PLAYER_PREFAB;
        public static GameObject BULLET_PREFAB;
        public static GameObject BULLET_POWERUP_PREFAB;
        public static GameObject HEALTH_POWERUP_PREFAB;

        public static void Init()
        {
            //PLAYER_PREFAB = new GameObject(new Mesh(Meshes.PLANE_MESH, Materials.PLAYER_MATERIAL));
            PLAYER_PREFAB = new GameObject(Mesh.CreateYPlane(new Vector2(-7,-10), new Vector2(7,0),Materials.PLAYER_MATERIAL));
            MAIN_PLAYER_PREFAB = new GameObject(null);
            BULLET_PREFAB = new GameObject(Mesh.CreateYPlane(new Vector2(-5, -5), new Vector2(5,5), Materials.BULLET_MATERIAL));
            BULLET_POWERUP_PREFAB = new GameObject(Mesh.CreateYPlane(new Vector2(-5, -5), new Vector2(5,5), Materials.BULLET_MATERIAL));
            HEALTH_POWERUP_PREFAB = new GameObject(Mesh.CreateYPlane(new Vector2(-5, -5), new Vector2(5,5), Materials.HEALTH_MATERIAL));
        }
    }
}
