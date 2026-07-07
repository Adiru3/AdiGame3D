using System;
using OpenTK;
using Engine.Core.Entities;

namespace Engine.Runtime.Player
{
    /// <summary>
    /// First-person камера для рантайма.
    /// </summary>
    public class RuntimeCamera
    {
        public float Yaw   { get; set; } = -90f;
        public float Pitch { get; set; } = 0f;
        public float Fov   { get; set; } = 70f;
        public float Near  { get; set; } = 0.05f;
        public float Far   { get; set; } = 500f;
        public float Aspect{ get; set; } = 1.78f;

        // Высота камеры над позицией игрока
        public float EyeHeight { get; set; } = 0.8f;

        public Vector3? OverrideEyePosition { get; set; }

        public Vector3 GetEyePosition(Vec3 playerPos)
        {
            if (OverrideEyePosition.HasValue)
                return OverrideEyePosition.Value;
            return new Vector3(playerPos.X, playerPos.Y + EyeHeight, playerPos.Z);
        }

        public Vector3 Front
        {
            get
            {
                float yr = MathHelper.DegreesToRadians(Yaw);
                float pr = MathHelper.DegreesToRadians(Pitch);
                return new Vector3(
                    (float)(Math.Cos(pr) * Math.Cos(yr)),
                    (float)Math.Sin(pr),
                    (float)(Math.Cos(pr) * Math.Sin(yr))
                ).Normalized();
            }
        }

        public Matrix4 GetViewMatrix(Vec3 playerPos)
        {
            var eye = GetEyePosition(playerPos);
            return Matrix4.LookAt(eye, eye + Front, Vector3.UnitY);
        }

        public Matrix4 GetProjectionMatrix()
            => Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov), Aspect, Near, Far);

        public void ApplyMouseDelta(float dx, float dy, float sensitivity = 0.15f)
        {
            Yaw   += dx * sensitivity;
            Pitch -= dy * sensitivity;
            Pitch  = Math.Max(-88f, Math.Min(88f, Pitch));
        }
    }
}
