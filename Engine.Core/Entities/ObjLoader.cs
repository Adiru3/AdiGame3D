using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Engine.Core.Entities
{
    public class ObjModelData
    {
        // Каждая вершина состоит из: Position (3f), Normal (3f), TexCoords (2f)
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
    }

    public static class ObjLoader
    {
        private struct VertexKey : IEquatable<VertexKey>
        {
            public int PosIndex;
            public int TexIndex;
            public int NormIndex;

            public VertexKey(int pos, int tex, int norm)
            {
                PosIndex = pos;
                TexIndex = tex;
                NormIndex = norm;
            }

            public bool Equals(VertexKey other)
            {
                return PosIndex == other.PosIndex && TexIndex == other.TexIndex && NormIndex == other.NormIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is VertexKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + PosIndex;
                    hash = hash * 23 + TexIndex;
                    hash = hash * 23 + NormIndex;
                    return hash;
                }
            }
        }

        public static ObjModelData Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"OBJ file not found: {filePath}");

            var tempPositions = new List<Vec3>();
            var tempNormals = new List<Vec3>();
            var tempTexCoords = new List<Vec2>();

            var finalVertices = new List<float>();
            var finalIndices = new List<uint>();
            var vertexCache = new Dictionary<VertexKey, uint>();

            var lines = File.ReadAllLines(filePath);
            char[] splitChars = { ' ' };
            char[] faceSplitChars = { '/' };

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed))
                    continue;

                string[] tokens = trimmed.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0) continue;

                string type = tokens[0];

                if (type == "v")
                {
                    float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    tempPositions.Add(new Vec3(x, y, z));
                }
                else if (type == "vn")
                {
                    float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    tempNormals.Add(new Vec3(x, y, z));
                }
                else if (type == "vt")
                {
                    float u = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    float v = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    // OpenGL текстурные координаты начинаются снизу слева, часто переворачиваем Y
                    tempTexCoords.Add(new Vec2(u, 1f - v));
                }
                else if (type == "f")
                {
                    // Поддержка полигонов с более чем 3 вершинами через триангуляцию (Triangle Fan)
                    var faceKeys = new List<VertexKey>();
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        string[] parts = tokens[i].Split(faceSplitChars);
                        int posIdx = 0, texIdx = 0, normIdx = 0;

                        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                            posIdx = int.Parse(parts[0]) - 1;

                        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                            texIdx = int.Parse(parts[1]) - 1;

                        if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
                            normIdx = int.Parse(parts[2]) - 1;

                        faceKeys.Add(new VertexKey(posIdx, texIdx, normIdx));
                    }

                    // Триангуляция полигона
                    for (int i = 1; i < faceKeys.Count - 1; i++)
                    {
                        AddVertex(faceKeys[0], tempPositions, tempNormals, tempTexCoords, finalVertices, finalIndices, vertexCache);
                        AddVertex(faceKeys[i], tempPositions, tempNormals, tempTexCoords, finalVertices, finalIndices, vertexCache);
                        AddVertex(faceKeys[i + 1], tempPositions, tempNormals, tempTexCoords, finalVertices, finalIndices, vertexCache);
                    }
                }
            }

            return new ObjModelData
            {
                Vertices = finalVertices.ToArray(),
                Indices = finalIndices.ToArray()
            };
        }

        private static void AddVertex(
            VertexKey key,
            List<Vec3> tempPositions,
            List<Vec3> tempNormals,
            List<Vec2> tempTexCoords,
            List<float> finalVertices,
            List<uint> finalIndices,
            Dictionary<VertexKey, uint> vertexCache)
        {
            if (vertexCache.TryGetValue(key, out uint index))
            {
                finalIndices.Add(index);
                return;
            }

            uint newIndex = (uint)(finalVertices.Count / 8);
            vertexCache[key] = newIndex;
            finalIndices.Add(newIndex);

            // Позиция
            Vec3 pos = key.PosIndex >= 0 && key.PosIndex < tempPositions.Count ? tempPositions[key.PosIndex] : Vec3.Zero;
            finalVertices.Add(pos.X);
            finalVertices.Add(pos.Y);
            finalVertices.Add(pos.Z);

            // Нормаль
            Vec3 norm = key.NormIndex >= 0 && key.NormIndex < tempNormals.Count ? tempNormals[key.NormIndex] : Vec3.Up;
            finalVertices.Add(norm.X);
            finalVertices.Add(norm.Y);
            finalVertices.Add(norm.Z);

            // Текстурные координаты
            Vec2 tex = key.TexIndex >= 0 && key.TexIndex < tempTexCoords.Count ? tempTexCoords[key.TexIndex] : Vec2.Zero;
            finalVertices.Add(tex.X);
            finalVertices.Add(tex.Y);
        }
    }

    public class Vec2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2() { }
        public Vec2(float x, float y) { X = x; Y = y; }

        public static Vec2 Zero => new Vec2(0, 0);
    }
}
