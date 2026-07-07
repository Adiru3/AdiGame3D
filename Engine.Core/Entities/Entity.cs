using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Engine.Core.Entities
{
    /// <summary>
    /// Базовая сущность сцены. Хранит трансформацию, тип и кастомные свойства.
    /// </summary>
    public class Entity
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty("name")]
        public string Name { get; set; } = "Entity";

        [JsonProperty("type")]
        public EntityType Type { get; set; } = EntityType.Block;

        [JsonProperty("position")]
        public Vec3 Position { get; set; } = Vec3.Zero;

        [JsonProperty("rotation")]
        public Vec3 Rotation { get; set; } = Vec3.Zero;

        [JsonProperty("scale")]
        public Vec3 Scale { get; set; } = Vec3.One;

        [JsonProperty("color")]
        public ColorRGB Color { get; set; } = ColorRGB.White;

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public Entity() { }

        public Entity(EntityType type, Vec3 position)
        {
            Type     = type;
            Position = position;
            Name     = type.ToString();
            Color    = EntityTypeColors.GetColor(type);
        }

        public Entity Clone()
        {
            return new Entity
            {
                Id         = Guid.NewGuid(),
                Name       = Name + "_copy",
                Type       = Type,
                Position   = new Vec3(Position.X, Position.Y, Position.Z),
                Rotation   = new Vec3(Rotation.X, Rotation.Y, Rotation.Z),
                Scale      = new Vec3(Scale.X, Scale.Y, Scale.Z),
                Color      = new ColorRGB(Color.R, Color.G, Color.B),
                Properties = new Dictionary<string, string>(Properties)
            };
        }

        public override string ToString() =>
            $"[{Type}] {Name} @ ({Position.X:F1}, {Position.Y:F1}, {Position.Z:F1})";
    }

    /// <summary>
    /// JSON-сериализуемый 3D-вектор.
    /// </summary>
    public class Vec3
    {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }
        [JsonProperty("z")] public float Z { get; set; }

        public Vec3() { }
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public static Vec3 Zero => new Vec3(0, 0, 0);
        public static Vec3 One  => new Vec3(1, 1, 1);
        public static Vec3 Up   => new Vec3(0, 1, 0);

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 a, float s) => new Vec3(a.X * s, a.Y * s, a.Z * s);

        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vec3 Normalized()
        {
            float l = Length();
            return l > 0 ? new Vec3(X / l, Y / l, Z / l) : Zero;
        }

        public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
    }

    /// <summary>
    /// Цвет сущности для рендеринга.
    /// </summary>
    public class ColorRGB
    {
        [JsonProperty("r")] public float R { get; set; }
        [JsonProperty("g")] public float G { get; set; }
        [JsonProperty("b")] public float B { get; set; }

        public ColorRGB() { }
        public ColorRGB(float r, float g, float b) { R = r; G = g; B = b; }

        public static ColorRGB White   => new ColorRGB(1f, 1f, 1f);
        public static ColorRGB Red     => new ColorRGB(1f, 0.2f, 0.2f);
        public static ColorRGB Green   => new ColorRGB(0.2f, 1f, 0.2f);
        public static ColorRGB Blue    => new ColorRGB(0.2f, 0.4f, 1f);
        public static ColorRGB Yellow  => new ColorRGB(1f, 0.9f, 0.1f);
        public static ColorRGB Orange  => new ColorRGB(1f, 0.55f, 0.1f);
        public static ColorRGB Purple  => new ColorRGB(0.7f, 0.2f, 1f);
        public static ColorRGB Cyan    => new ColorRGB(0.1f, 0.9f, 0.9f);
        public static ColorRGB Gray    => new ColorRGB(0.5f, 0.5f, 0.5f);
        public static ColorRGB Brown   => new ColorRGB(0.6f, 0.3f, 0.1f);
    }

    /// <summary>
    /// Маппинг типов сущностей на дефолтные цвета.
    /// </summary>
    public static class EntityTypeColors
    {
        public static ColorRGB GetColor(EntityType type)
        {
            switch (type)
            {
                case EntityType.Block:        return new ColorRGB(0.6f, 0.6f, 0.65f);
                case EntityType.Stone:        return new ColorRGB(0.45f, 0.45f, 0.45f);
                case EntityType.Wood:         return new ColorRGB(0.55f, 0.35f, 0.15f);
                case EntityType.Glass:        return new ColorRGB(0.7f, 0.85f, 1.0f);
                case EntityType.Metal:        return new ColorRGB(0.7f, 0.7f, 0.8f);
                case EntityType.Brick:        return new ColorRGB(0.72f, 0.25f, 0.15f);
                case EntityType.Grass:        return new ColorRGB(0.25f, 0.65f, 0.2f);
                case EntityType.Sand:         return new ColorRGB(0.92f, 0.82f, 0.55f);
                case EntityType.Water:        return new ColorRGB(0.1f, 0.4f, 0.9f);
                case EntityType.Lava:         return new ColorRGB(1.0f, 0.35f, 0.0f);
                case EntityType.Ice:          return new ColorRGB(0.75f, 0.9f, 1.0f);
                case EntityType.Dirt:         return new ColorRGB(0.55f, 0.35f, 0.2f);
                case EntityType.PlayerSpawn:  return new ColorRGB(0.1f, 0.9f, 0.2f);
                case EntityType.Light:        return new ColorRGB(1.0f, 0.95f, 0.5f);
                case EntityType.Trigger:      return new ColorRGB(0.8f, 0.1f, 0.8f);
                case EntityType.Checkpoint:   return new ColorRGB(0.2f, 0.8f, 1.0f);
                case EntityType.KillZone:     return new ColorRGB(1.0f, 0.1f, 0.1f);
                case EntityType.Model3D:      return new ColorRGB(0.4f, 0.7f, 0.4f);
                case EntityType.SoundPoint:   return new ColorRGB(0.9f, 0.2f, 0.6f);
                case EntityType.CameraWaypoint: return new ColorRGB(0.2f, 0.5f, 0.9f);
                default:                      return ColorRGB.White;
            }
        }
    }
}
