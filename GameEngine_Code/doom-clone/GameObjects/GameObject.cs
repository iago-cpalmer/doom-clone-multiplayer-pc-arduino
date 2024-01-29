using OpenTK.Mathematics;
using doom_clone.Rendering.Model;

namespace doom_clone.GameObjects
{
    public class GameObject
    {
        public Vector3 Position { get { return _position; } set { _position = value; } }
        public Vector3 Rotation { get { return _rotation; } set { _rotation = value; } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; } }
        public Mesh Mesh { get { return _mesh; } set { _mesh = value; } }
        public string Name { get { return _name; } set { _name = value; } }

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;
        private Mesh _mesh;

        private string _name = "unidentified";

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scale, Mesh mesh)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
            _mesh = mesh;
        }

        public GameObject(Mesh mesh)
        {
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scale = Vector3.One;
            _mesh = mesh;
        }

        public GameObject Clone()
        {
            return (GameObject) MemberwiseClone();
        }

        public string ToString()
        {
            return Position + ", " + Rotation + ", " + Scale;
        }
    }
}
