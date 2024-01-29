using OpenTK.Mathematics;
using doom_clone.Utils;
using System;

namespace doom_clone.GameObjects.Cameras
{
    public class FPCamera : Camera
    {
        private const float MIN_SPEED = 0.1f, MAX_SPEED = 50.0f;
        private const float MIN_FOV = 10.0f, MAX_FOV = 120.0f;
        private float _speed = 10f;
        private float _sensitivity = 0.5f;

        // Variable to avoid creating it in Update function multiple times, for GC
        private Vector3 _direction = Vector3.Zero;
        public FPCamera(Vector3 position, Vector3 rotation) : base(position, rotation)
        {
            OnFovChanged += Window.Instance.UpdateProjectionMatrix;
            // Set yaw and pitch values to initial rotation
            Yaw = rotation.Y;
            Pitch = rotation.X;
        }
        public override void Update()
        {
            // Calculate rotation
            Yaw += InputHandler.DeltaMousePosition.X * _sensitivity;
            Pitch += InputHandler.DeltaMousePosition.Y * _sensitivity;
            Pitch = MathHelper.Clamp(Pitch, -89, 89);
            Yaw = MathHelper.NormalizeAngle(Yaw);
            
            Rotation.X = Pitch;
            Rotation.Y = Yaw;
            
            Forward = ComputeForwardVector();
            Right = ComputeRightVector();
        }
    }
}
