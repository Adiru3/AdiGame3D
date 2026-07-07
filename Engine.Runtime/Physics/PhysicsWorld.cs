using System.Collections.Generic;
using Engine.Core.Entities;
using Engine.Core.Scene;

namespace Engine.Runtime.Physics
{
    /// <summary>
    /// Мировая физика: проверяет коллизии между объектами и блоками сцены.
    /// </summary>
    public class PhysicsWorld
    {
        private SceneManager _scene;

        // Кэш AABB блоков (обновляется при изменении сцены)
        private List<AABB> _blockBoxes = new List<AABB>();

        public float Gravity { get; set; } = -12f;

        public PhysicsWorld(SceneManager scene)
        {
            _scene = scene;
            RebuildCache();
            scene.SceneLoaded += RebuildCache;
        }

        public void RebuildCache()
        {
            _blockBoxes.Clear();
            foreach (var e in _scene.CurrentScene.Entities)
            {
                if ((int)e.Type < 100)  // Только блоки
                    _blockBoxes.Add(AABB.ForBlock(e.Position));
            }
        }

        /// <summary>
        /// Разрешает коллизию AABB obj со всеми блоками сцены.
        /// Возвращает скорректированную позицию и признак "стоит на земле".
        /// </summary>
        public void Resolve(
            ref float posX, ref float posY, ref float posZ,
            ref float velX, ref float velY, ref float velZ,
            float halfW, float halfH, float halfD,
            out bool onGround)
        {
            onGround = false;

            // Итерируем несколько раз для устойчивости
            for (int iter = 0; iter < 3; iter++)
            {
                var obj = new AABB(
                    posX - halfW, posY - halfH, posZ - halfD,
                    posX + halfW, posY + halfH, posZ + halfD);

                foreach (var block in _blockBoxes)
                {
                    if (!obj.Intersects(block)) continue;

                    float px, py, pz;
                    if (!AABB.GetPenetration(obj, block, out px, out py, out pz))
                        continue;

                    posX += px; posY += py; posZ += pz;

                    if (py > 0f)                // Стоим на земле
                    {
                        onGround = true;
                        if (velY < 0f) velY = 0f;
                    }
                    else if (py < 0f)           // Удар в потолок
                    {
                        if (velY > 0f) velY = 0f;
                    }

                    if (px != 0f) velX = 0f;
                    if (pz != 0f) velZ = 0f;

                    // Обновляем AABB после выталкивания
                    obj = new AABB(
                        posX - halfW, posY - halfH, posZ - halfD,
                        posX + halfW, posY + halfH, posZ + halfD);
                }
            }
        }

        public void OnEntityAdded(Entity e)
        {
            if ((int)e.Type < 100)
                _blockBoxes.Add(AABB.ForBlock(e.Position));
        }

        public void OnEntityRemoved(Entity e)
        {
            // Полная перестройка (простота > производительность для MVP)
            RebuildCache();
        }
    }
}
