using System;
using System.Collections.Generic;
using Engine.Core.Entities;
using Newtonsoft.Json;

namespace Engine.Core.Scene
{
    /// <summary>
    /// Контейнер всей сцены — сериализуется в JSON-файл уровня.
    /// </summary>
    public class Scene
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Untitled Level";

        [JsonProperty("author")]
        public string Author { get; set; } = "Unknown";

        [JsonProperty("created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [JsonProperty("modified")]
        public DateTime Modified { get; set; } = DateTime.UtcNow;

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("skyColor")]
        public ColorRGB SkyColor { get; set; } = new ColorRGB(0.35f, 0.55f, 0.85f);

        [JsonProperty("ambientLight")]
        public float AmbientLight { get; set; } = 0.3f;

        [JsonProperty("gravityStrength")]
        public float GravityStrength { get; set; } = 9.81f;

        [JsonProperty("entities")]
        public List<Entity> Entities { get; set; } = new List<Entity>();

        /// <summary>Найти первую точку спавна игрока.</summary>
        [JsonIgnore]
        public Entity PlayerSpawn
        {
            get
            {
                foreach (var e in Entities)
                    if (e.Type == EntityType.PlayerSpawn)
                        return e;
                return null;
            }
        }

        /// <summary>Все блоки сцены (не специальные объекты).</summary>
        [JsonIgnore]
        public IEnumerable<Entity> Blocks
        {
            get
            {
                foreach (var e in Entities)
                    if ((int)e.Type < 100)
                        yield return e;
            }
        }
    }
}
