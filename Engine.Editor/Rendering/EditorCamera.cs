using System;
using OpenTK;

namespace Engine.Editor.Rendering
{
    /// <summary>
    /// Fly-cam камера редактора: WASD+мышь (ПКМ), колёсико — скорость.
    /// </summary>
    public class EditorCamera
    {
        // ─── Позиция и ориентация ─────────────────────────────────────────

        public Vector3 Position { get; set; } = new Vector3(8f, 6f, 12f);
        public float   Yaw      { get; set; } = -100f;   // градусы
        public float   Pitch    { get; set; } = -20f;    // градусы

        // ─── Параметры проекции ───────────────────────────────────────────

        public float Fov    { get; set; } = 60f;         // FOV в градусах
        public float Near   { get; set; } = 0.05f;
        public float Far    { get; set; } = 1000f;
        public float Aspect { get; set; } = 1.78f;       // Обновляется при ресайзе окна

        // ─── Управление ───────────────────────────────────────────────────

        public float MoveSpeed     { get; set; } = 6f;
        public float FastMultiplier{ get; set; } = 3f;
        public float Sensitivity   { get; set; } = 0.18f;

        // Состояния клавиш
        public bool KeyW, KeyS, KeyA, KeyD, KeyQ, KeyE, KeyShift;

        // Мышь
        private bool  _mouseCapture;
        private int   _lastMouseX, _lastMouseY;

        // ─── Вычисляемые векторы ──────────────────────────────────────────

        public Vector3 Front
        {
            get
            {
                float yawR   = MathHelper.DegreesToRadians(Yaw);
                float pitchR = MathHelper.DegreesToRadians(Pitch);
                return new Vector3(
                    (float)(Math.Cos(pitchR) * Math.Cos(yawR)),
                    (float)Math.Sin(pitchR),
                    (float)(Math.Cos(pitchR) * Math.Sin(yawR))
                ).Normalized();
            }
        }

        public Vector3 Right => Vector3.Cross(Front, Vector3.UnitY).Normalized();
        public Vector3 Up    => Vector3.Cross(Right, Front).Normalized();

        // ─── Матрицы ──────────────────────────────────────────────────────

        public Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Position + Front, Vector3.UnitY);

        public Matrix4 GetProjectionMatrix()
            => Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov),
                Aspect, Near, Far);

        // ─── Обновление ───────────────────────────────────────────────────

        public void Update(float dt)
        {
            if (!_mouseCapture) return;

            float speed = MoveSpeed * (KeyShift ? FastMultiplier : 1f);
            Vector3 pos = Position;

            if (KeyW) pos += Front  * speed * dt;
            if (KeyS) pos -= Front  * speed * dt;
            if (KeyA) pos -= Right  * speed * dt;
            if (KeyD) pos += Right  * speed * dt;
            if (KeyE) pos += Vector3.UnitY * speed * dt;
            if (KeyQ) pos -= Vector3.UnitY * speed * dt;

            Position = pos;
        }

        // ─── Ввод мыши ────────────────────────────────────────────────────

        public void BeginMouseLook(int x, int y)
        {
            _mouseCapture = true;
            _lastMouseX   = x;
            _lastMouseY   = y;
        }

        public void EndMouseLook() => _mouseCapture = false;

        public bool IsMouseLooking => _mouseCapture;

        public void OnMouseMove(int x, int y)
        {
            if (!_mouseCapture) return;
            int dx = x - _lastMouseX;
            int dy = y - _lastMouseY;
            _lastMouseX = x;
            _lastMouseY = y;

            Yaw   += dx * Sensitivity;
            Pitch -= dy * Sensitivity;
            Pitch  = Math.Max(-89f, Math.Min(89f, Pitch));
        }

        public void OnScroll(float delta)
        {
            MoveSpeed = Math.Max(0.5f, MoveSpeed + delta * 0.5f);
        }

        // ─── Raycast ──────────────────────────────────────────────────────

        /// <summary>
        /// Преобразует позицию экрана (пиксель) в луч в мировом пространстве.
        /// </summary>
        public Ray ScreenPointToRay(int screenX, int screenY, int viewWidth, int viewHeight)
        {
            // NDC координаты
            float ndcX = (2f * screenX) / viewWidth  - 1f;
            float ndcY = 1f - (2f * screenY) / viewHeight;

            Matrix4 proj    = GetProjectionMatrix();
            Matrix4 view    = GetViewMatrix();
            Matrix4 invProj = Matrix4.Invert(proj);
            Matrix4 invView = Matrix4.Invert(view);

            // Clip space
            Vector4 clipCoords = new Vector4(ndcX, ndcY, -1f, 1f);

            // Eye space
            Vector4 eyeCoords = Vector4.Transform(clipCoords, invProj);
            eyeCoords = new Vector4(eyeCoords.X, eyeCoords.Y, -1f, 0f);

            // World space
            Vector4 worldCoords = Vector4.Transform(eyeCoords, invView);
            Vector3 dir = new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z).Normalized();

            return new Ray(Position, dir);
        }
    }

    /// <summary>
    /// Луч в трёхмерном пространстве.
    /// </summary>
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin    = origin;
            Direction = direction;
        }

        public Vector3 GetPoint(float t) => Origin + Direction * t;
    }
}
