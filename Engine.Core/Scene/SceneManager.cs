using System;
using System.IO;
using Engine.Core.Entities;
using Newtonsoft.Json;

namespace Engine.Core.Scene
{
    /// <summary>
    /// Менеджер сцены: добавление/удаление сущностей, сохранение и загрузка.
    /// </summary>
    public class SceneManager
    {
        private Scene _scene;

        public Scene CurrentScene => _scene;

        public event Action<Entity> EntityAdded;
        public event Action<Entity> EntityRemoved;
        public event Action SceneLoaded;
        public event Action SceneSaved;

        public SceneManager()
        {
            NewScene();
        }

        // ─── Управление сценой ───────────────────────────────────────────────

        public void NewScene()
        {
            _scene = new Scene
            {
                Name    = "New Level",
                Author  = Environment.UserName,
                Created = DateTime.UtcNow
            };
        }

        // ─── CRUD сущностей ──────────────────────────────────────────────────

        public Entity AddEntity(EntityType type, Vec3 position)
        {
            var e = new Entity(type, position);
            // Сетка 1×1: округляем к ближайшему целому при добавлении
            e.Position = new Vec3(
                (float)Math.Round(position.X),
                (float)Math.Round(position.Y),
                (float)Math.Round(position.Z));
            _scene.Entities.Add(e);
            EntityAdded?.Invoke(e);
            return e;
        }

        public Entity AddEntityRaw(Entity entity)
        {
            _scene.Entities.Add(entity);
            EntityAdded?.Invoke(entity);
            return entity;
        }

        public bool RemoveEntity(Guid id)
        {
            for (int i = 0; i < _scene.Entities.Count; i++)
            {
                if (_scene.Entities[i].Id == id)
                {
                    var removed = _scene.Entities[i];
                    _scene.Entities.RemoveAt(i);
                    EntityRemoved?.Invoke(removed);
                    return true;
                }
            }
            return false;
        }

        public Entity FindById(Guid id)
        {
            foreach (var e in _scene.Entities)
                if (e.Id == id)
                    return e;
            return null;
        }

        public bool HasEntityAt(Vec3 pos)
        {
            int rx = (int)Math.Round(pos.X);
            int ry = (int)Math.Round(pos.Y);
            int rz = (int)Math.Round(pos.Z);
            foreach (var e in _scene.Entities)
            {
                if ((int)Math.Round(e.Position.X) == rx &&
                    (int)Math.Round(e.Position.Y) == ry &&
                    (int)Math.Round(e.Position.Z) == rz)
                    return true;
            }
            return false;
        }

        // ─── Сохранение и загрузка ───────────────────────────────────────────

        public void SaveScene(string path)
        {
            _scene.Modified = DateTime.UtcNow;
            var settings = new JsonSerializerSettings
            {
                Formatting        = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(_scene, settings);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            SceneSaved?.Invoke();
        }

        public void LoadScene(string path)
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            _scene = JsonConvert.DeserializeObject<Scene>(json);
            if (_scene == null) _scene = new Scene();
            SceneLoaded?.Invoke();
        }

        public string SerializeToJson()
        {
            _scene.Modified = DateTime.UtcNow;
            return JsonConvert.SerializeObject(_scene, Formatting.Indented);
        }

        public void LoadFromJson(string json)
        {
            _scene = JsonConvert.DeserializeObject<Scene>(json) ?? new Scene();
            SceneLoaded?.Invoke();
        }
    }
}
