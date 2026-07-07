using System;
using System.Collections.Generic;
using System.Globalization;
using Engine.Core.Entities;

namespace Engine.Runtime.Player
{
    public class CutsceneManager
    {
        public class WaypointData
        {
            public Vec3 Position;
            public float Yaw;
            public float Pitch;
            public float Fov;
            public float Duration;
            public int Sequence;
        }

        private List<WaypointData> _waypoints = new List<WaypointData>();
        private int _currentIdx;
        private float _elapsedTime;

        public bool IsActive { get; private set; }
        public Vec3 CurrentPos { get; private set; } = Vec3.Zero;
        public float CurrentYaw { get; private set; }
        public float CurrentPitch { get; private set; }
        public float CurrentFov { get; private set; } = 70f;

        public event Action OnFinished;

        public void Start(IEnumerable<Entity> entities)
        {
            _waypoints.Clear();

            foreach (var e in entities)
            {
                if (e.Type != EntityType.CameraWaypoint) continue;

                int seq = 0;
                if (e.Properties.TryGetValue("sequence", out string sStr))
                    int.TryParse(sStr, out seq);

                float dur = 3f;
                if (e.Properties.TryGetValue("duration", out string dStr))
                    float.TryParse(dStr, NumberStyles.Float, CultureInfo.InvariantCulture, out dur);

                float fov = 70f;
                if (e.Properties.TryGetValue("fov", out string fStr))
                    float.TryParse(fStr, NumberStyles.Float, CultureInfo.InvariantCulture, out fov);

                _waypoints.Add(new WaypointData
                {
                    Position = e.Position,
                    Yaw = e.Rotation.Y,   // Используем Y в Rotation как Yaw
                    Pitch = e.Rotation.X, // Используем X в Rotation как Pitch
                    Fov = fov,
                    Duration = Math.Max(0.1f, dur),
                    Sequence = seq
                });
            }

            if (_waypoints.Count < 2)
            {
                Console.WriteLine("Cutscene requires at least 2 CameraWaypoints.");
                return;
            }

            // Сортируем по sequence
            _waypoints.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));

            _currentIdx = 0;
            _elapsedTime = 0f;
            IsActive = true;

            // Инициализируем стартовое состояние
            UpdateState(0f);
        }

        public void Stop()
        {
            IsActive = false;
        }

        public void Update(float dt)
        {
            if (!IsActive || _waypoints.Count < 2) return;

            _elapsedTime += dt;
            var currentSegment = _waypoints[_currentIdx];

            if (_elapsedTime >= currentSegment.Duration)
            {
                _elapsedTime -= currentSegment.Duration;
                _currentIdx++;

                if (_currentIdx >= _waypoints.Count - 1)
                {
                    // Кат-сцена закончена
                    IsActive = false;
                    OnFinished?.Invoke();
                    return;
                }
            }

            UpdateState(_elapsedTime / _waypoints[_currentIdx].Duration);
        }

        private void UpdateState(float t)
        {
            var p1 = _waypoints[_currentIdx];
            var p2 = _waypoints[_currentIdx + 1];

            // Интерполяция позиции
            float x = Lerp(p1.Position.X, p2.Position.X, t);
            float y = Lerp(p1.Position.Y, p2.Position.Y, t);
            float z = Lerp(p1.Position.Z, p2.Position.Z, t);
            CurrentPos = new Vec3(x, y, z);

            // Интерполяция углов
            CurrentYaw = LerpAngle(p1.Yaw, p2.Yaw, t);
            CurrentPitch = Lerp(p1.Pitch, p2.Pitch, t);

            // Интерполяция FOV
            CurrentFov = Lerp(p1.Fov, p2.Fov, t);
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float LerpAngle(float a, float b, float t)
        {
            float diff = b - a;
            while (diff > 180f) diff -= 360f;
            while (diff < -180f) diff += 360f;
            return a + diff * t;
        }
    }
}
