using OpenTK.Mathematics;
using System;

namespace doom_clone.GameObjects.Cameras
{
    public abstract class Camera : IUpdatable
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Forward;
        public Vector3 Right;
        public readonly Vector3 UP_VECTOR = new Vector3(0.0f, 1.0f, 0.0f);
        public float FovY = 45.0f;
        public Action OnFovChanged;
        public float Yaw;
        public float Pitch;
        public Camera(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
            Forward = ComputeForwardVector();
            Right = ComputeRightVector();
        }        
        public Vector3 ComputeForwardVector()
        {
            Vector3 front;
            front.X = (float)MathHelper.Cos(MathHelper.DegreesToRadians(Rotation.X)) * (float)MathHelper.Cos(MathHelper.DegreesToRadians(Rotation.Y));
            front.Y = (float)MathHelper.Sin(MathHelper.DegreesToRadians(Rotation.X));
            front.Z = (float)MathHelper.Cos(MathHelper.DegreesToRadians(Rotation.X)) * (float)MathHelper.Sin(MathHelper.DegreesToRadians(Rotation.Y));
            return Vector3.Normalize(front);
        }
        public Vector3 ComputeRightVector()
        {
            return Vector3.Normalize(Vector3.Cross(UP_VECTOR, Forward));
        }

        public abstract void Update();
    }
}