using System;
using Engine.Core.Entities;

namespace Engine.Runtime.Physics
{
    /// <summary>
    /// Axis-Aligned Bounding Box для коллизий.
    /// </summary>
    public struct AABB
    {
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;

        public AABB(float minX, float minY, float minZ,
                    float maxX, float maxY, float maxZ)
        {
            MinX = minX; MinY = minY; MinZ = minZ;
            MaxX = maxX; MaxY = maxY; MaxZ = maxZ;
        }

        /// <summary>Создаёт AABB из центра и половины размера.</summary>
        public static AABB FromCenterHalfSize(Vec3 center, float hx, float hy, float hz)
            => new AABB(
                center.X - hx, center.Y - hy, center.Z - hz,
                center.X + hx, center.Y + hy, center.Z + hz);

        /// <summary>Создаёт AABB для блока 1×1×1.</summary>
        public static AABB ForBlock(Vec3 position)
            => new AABB(
                position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f,
                position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);

        /// <summary>Проверяет пересечение с другим AABB.</summary>
        public bool Intersects(AABB other)
            => MaxX > other.MinX && MinX < other.MaxX &&
               MaxY > other.MinY && MinY < other.MaxY &&
               MaxZ > other.MinZ && MinZ < other.MaxZ;

        /// <summary>
        /// Вычисляет перекрытие (penetration vector) между двумя AABB.
        /// Возвращает вектор для выталкивания A из B.
        /// </summary>
        public static bool GetPenetration(AABB a, AABB b,
            out float px, out float py, out float pz)
        {
            px = py = pz = 0f;
            if (!a.Intersects(b)) return false;

            float ox = Math.Min(a.MaxX - b.MinX, b.MaxX - a.MinX);
            float oy = Math.Min(a.MaxY - b.MinY, b.MaxY - a.MinY);
            float oz = Math.Min(a.MaxZ - b.MinZ, b.MaxZ - a.MinZ);

            // Выталкиваем по минимальной оси
            if (oy <= ox && oy <= oz)
            {
                // Y — вертикаль
                py = (a.MinY + a.MaxY) * 0.5f < (b.MinY + b.MaxY) * 0.5f
                    ? -oy : oy;
            }
            else if (ox <= oy && ox <= oz)
            {
                px = (a.MinX + a.MaxX) * 0.5f < (b.MinX + b.MaxX) * 0.5f
                    ? -ox : ox;
            }
            else
            {
                pz = (a.MinZ + a.MaxZ) * 0.5f < (b.MinZ + b.MaxZ) * 0.5f
                    ? -oz : oz;
            }
            return true;
        }

        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterY => (MinY + MaxY) * 0.5f;
        public float CenterZ => (MinZ + MaxZ) * 0.5f;

        public AABB Offset(float dx, float dy, float dz)
            => new AABB(MinX + dx, MinY + dy, MinZ + dz,
                        MaxX + dx, MaxY + dy, MaxZ + dz);

        public override string ToString()
            => $"AABB[({MinX:F2},{MinY:F2},{MinZ:F2}) - ({MaxX:F2},{MaxY:F2},{MaxZ:F2})]";
    }
}
