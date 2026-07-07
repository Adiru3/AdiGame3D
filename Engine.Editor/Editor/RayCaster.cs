using System;
using OpenTK;
using Engine.Core.Entities;
using Engine.Core.Scene;

namespace Engine.Editor.Editor
{
    /// <summary>
    /// Трассировка лучей (Raycast) для взаимодействия мышью со сценой.
    /// </summary>
    public static class RayCaster
    {
        private const float MaxDistance = 200f;
        private const float Step        = 0.02f;

        /// <summary>
        /// Выбирает сущность под курсором методом DDA (пошаговый рейкаст).
        /// Возвращает ближайшую найденную сущность или null.
        /// </summary>
        public static Entity PickEntity(
            Rendering.Ray ray,
            SceneManager scene)
        {
            Entity closest = null;
            float  minT    = float.MaxValue;

            foreach (var e in scene.CurrentScene.Entities)
            {
                float t;
                if (RayAABB(ray, e.Position, e.Scale, out t))
                {
                    if (t < minT && t >= 0f)
                    {
                        minT    = t;
                        closest = e;
                    }
                }
            }
            return closest;
        }

        /// <summary>
        /// Находит позицию на сетке (Y=0 или на верхней грани блока),
        /// куда нужно поставить новый блок.
        /// </summary>
        public static bool GetPlacementPosition(
            Rendering.Ray ray,
            SceneManager scene,
            out Vec3 outPos)
        {
            outPos = null;
            float bestT = float.MaxValue;
            bool  found = false;

            // 1. Проверяем верхнюю грань каждого блока
            foreach (var e in scene.CurrentScene.Entities)
            {
                // Верхняя плоскость Y = pos.Y + 0.5
                float planeY = e.Position.Y + 0.5f;
                float t;
                if (RayPlaneY(ray, planeY, out t) && t > 0.001f && t < bestT)
                {
                    Vector3 hit = ray.GetPoint(t);
                    // Проверяем, что хит попадает в XZ-площадь блока
                    if (Math.Abs(hit.X - e.Position.X) <= 0.5f &&
                        Math.Abs(hit.Z - e.Position.Z) <= 0.5f)
                    {
                        // Блок будет размещён сверху
                        outPos = new Vec3(
                            (float)Math.Round(hit.X),
                            (float)Math.Round(e.Position.Y + 1f),
                            (float)Math.Round(hit.Z));
                        bestT = t;
                        found = true;
                    }
                }
            }

            // 2. Если ничего не нашли — пересечение с Y=0
            if (!found)
            {
                float t;
                if (RayPlaneY(ray, 0f, out t) && t > 0.001f)
                {
                    Vector3 hit = ray.GetPoint(t);
                    outPos = new Vec3(
                        (float)Math.Round(hit.X),
                        0f,
                        (float)Math.Round(hit.Z));
                    found = true;
                }
            }

            return found;
        }

        // ─── Пересечение луча с AABB ─────────────────────────────────────

        /// <summary>
        /// Ray - AABB (Axis-Aligned Bounding Box) пересечение.
        /// pos = центр блока, scale = размер (обычно Vec3(1,1,1)).
        /// </summary>
        public static bool RayAABB(
            Rendering.Ray ray,
            Vec3 center,
            Vec3 scale,
            out float tMin)
        {
            tMin = 0f;
            Vector3 bMin = new Vector3(
                center.X - scale.X * 0.5f,
                center.Y - scale.Y * 0.5f,
                center.Z - scale.Z * 0.5f);
            Vector3 bMax = new Vector3(
                center.X + scale.X * 0.5f,
                center.Y + scale.Y * 0.5f,
                center.Z + scale.Z * 0.5f);

            Vector3 o = ray.Origin;
            Vector3 d = ray.Direction;

            float tNear = float.NegativeInfinity;
            float tFar  = float.PositiveInfinity;

            // X
            if (Math.Abs(d.X) < 1e-6f)
            {
                if (o.X < bMin.X || o.X > bMax.X) return false;
            }
            else
            {
                float t1 = (bMin.X - o.X) / d.X;
                float t2 = (bMax.X - o.X) / d.X;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tNear = Math.Max(tNear, t1);
                tFar  = Math.Min(tFar,  t2);
                if (tNear > tFar) return false;
            }

            // Y
            if (Math.Abs(d.Y) < 1e-6f)
            {
                if (o.Y < bMin.Y || o.Y > bMax.Y) return false;
            }
            else
            {
                float t1 = (bMin.Y - o.Y) / d.Y;
                float t2 = (bMax.Y - o.Y) / d.Y;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tNear = Math.Max(tNear, t1);
                tFar  = Math.Min(tFar,  t2);
                if (tNear > tFar) return false;
            }

            // Z
            if (Math.Abs(d.Z) < 1e-6f)
            {
                if (o.Z < bMin.Z || o.Z > bMax.Z) return false;
            }
            else
            {
                float t1 = (bMin.Z - o.Z) / d.Z;
                float t2 = (bMax.Z - o.Z) / d.Z;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tNear = Math.Max(tNear, t1);
                tFar  = Math.Min(tFar,  t2);
                if (tNear > tFar) return false;
            }

            tMin = tNear;
            return tFar >= 0f;
        }

        /// <summary>
        /// Пересечение луча с горизонтальной плоскостью Y = planeY.
        /// </summary>
        public static bool RayPlaneY(Rendering.Ray ray, float planeY, out float t)
        {
            t = 0f;
            float denom = ray.Direction.Y;
            if (Math.Abs(denom) < 1e-6f) return false;
            t = (planeY - ray.Origin.Y) / denom;
            return t >= 0f;
        }
    }
}
