using System;
using Engine.Core.Entities;
using Engine.Runtime.Physics;

namespace Engine.Runtime.Player
{
    /// <summary>
    /// Физика и управление игроком на основе AABB-коллизий.
    /// </summary>
    public class PlayerController
    {
        // ─── Трансформация ────────────────────────────────────────────────

        public Vec3  Position { get; set; } = new Vec3(0, 2, 0);
        public float VelX     { get; private set; }
        public float VelY     { get; private set; }
        public float VelZ     { get; private set; }
        public bool  OnGround { get; private set; }
        public bool  IsAlive  { get; private set; } = true;

        // ─── Параметры движения ───────────────────────────────────────────

        public float MoveSpeed   { get; set; } = 5.0f;
        public float SprintSpeed { get; set; } = 8.5f;
        public float JumpForce   { get; set; } = 5.5f;
        public float Friction    { get; set; } = 12f;

        // AABB: W=0.6, H=1.8, D=0.6 (как в Minecraft)
        private const float HalfW = 0.3f;
        private const float HalfH = 0.9f;
        private const float HalfD = 0.3f;

        // ─── Инпут ────────────────────────────────────────────────────────

        public bool InputForward, InputBack, InputLeft, InputRight;
        public bool InputJump, InputSprint;
        public float ViewYaw; // текущий угол взгляда (для движения относительно камеры)

        // ─── Ссылки ───────────────────────────────────────────────────────

        private PhysicsWorld _physics;

        public PlayerController(PhysicsWorld physics)
        {
            _physics = physics;
        }

        // ─── Обновление ───────────────────────────────────────────────────

        public void Update(float dt)
        {
            if (!IsAlive) return;

            ApplyMovement(dt);
            ApplyGravity(dt);
            ResolveCollisions();
            CheckKillZone();
        }

        private void ApplyMovement(float dt)
        {
            float speed = InputSprint ? SprintSpeed : MoveSpeed;

            // Направление движения в мировом пространстве (относительно Yaw камеры)
            float yr = (float)(ViewYaw * Math.PI / 180.0);
            float fx = (float)Math.Sin(yr);
            float fz = (float)Math.Cos(yr);
            float rx =  fz;
            float rz = -fx;

            float dx = 0, dz = 0;
            if (InputForward) { dx += fx; dz += fz; }
            if (InputBack)    { dx -= fx; dz -= fz; }
            if (InputRight)   { dx += rx; dz += rz; }
            if (InputLeft)    { dx -= rx; dz -= rz; }

            // Нормализуем диагональное движение
            float len = (float)Math.Sqrt(dx * dx + dz * dz);
            if (len > 0f) { dx /= len; dz /= len; }

            if (OnGround)
            {
                // На земле: резкая, отзывчивая скорость
                VelX = dx * speed;
                VelZ = dz * speed;
            }
            else
            {
                // В воздухе: ограниченное управление
                VelX += dx * speed * 0.08f;
                VelZ += dz * speed * 0.08f;
                // Ограничиваем горизонтальную скорость в воздухе
                float hv = (float)Math.Sqrt(VelX * VelX + VelZ * VelZ);
                if (hv > speed)
                {
                    VelX = VelX / hv * speed;
                    VelZ = VelZ / hv * speed;
                }
            }

            // Прыжок
            if (InputJump && OnGround)
                VelY = JumpForce;

            // Трение на земле
            if (OnGround && !InputForward && !InputBack && !InputLeft && !InputRight)
            {
                VelX = MoveTowards(VelX, 0f, Friction * dt);
                VelZ = MoveTowards(VelZ, 0f, Friction * dt);
            }
        }

        private void ApplyGravity(float dt)
        {
            if (!OnGround)
                VelY += _physics.Gravity * dt;

            // Ограничение скорости падения
            if (VelY < -25f) VelY = -25f;

            float px = Position.X + VelX * dt;
            float py = Position.Y + VelY * dt;
            float pz = Position.Z + VelZ * dt;
            Position = new Vec3(px, py, pz);
        }

        private void ResolveCollisions()
        {
            float px = Position.X, py = Position.Y, pz = Position.Z;
            float vx = VelX, vy = VelY, vz = VelZ;
            bool ground;

            _physics.Resolve(ref px, ref py, ref pz,
                             ref vx, ref vy, ref vz,
                             HalfW, HalfH, HalfD,
                             out ground);

            Position = new Vec3(px, py, pz);
            VelX = vx; VelY = vy; VelZ = vz;
            OnGround = ground;
        }

        private void CheckKillZone()
        {
            // Упал ниже -50? Убиваем и воскрешаем
            if (Position.Y < -50f)
            {
                VelX = VelY = VelZ = 0f;
                Position = new Vec3(0f, 3f, 0f);
            }
        }

        private static float MoveTowards(float cur, float target, float maxDelta)
        {
            float diff = target - cur;
            if (Math.Abs(diff) <= maxDelta) return target;
            return cur + Math.Sign(diff) * maxDelta;
        }

        public void Respawn(Vec3 spawnPos)
        {
            Position = new Vec3(spawnPos.X, spawnPos.Y + 1f, spawnPos.Z);
            VelX = VelY = VelZ = 0f;
            IsAlive = true;
        }
    }
}
