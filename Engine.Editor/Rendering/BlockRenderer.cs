using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Engine.Core.Entities;
using Engine.Core.Rendering;

namespace Engine.Editor.Rendering
{
    public class BlockRenderer : IDisposable
    {
        public class ModelVAO
        {
            public int Vao;
            public int Vbo;
            public int Ebo;
            public int IndexCount;
        }

        private int           _vao, _vbo, _ebo;
        private ShaderProgram _blockShader;
        private ShaderProgram _outlineShader;
        private bool          _disposed;

        private Dictionary<string, ModelVAO> _modelCache = new Dictionary<string, ModelVAO>();
        private Dictionary<string, int> _textureCache = new Dictionary<string, int>();

        // Юнит-куб: вершины (x,y,z) + нормали (nx,ny,nz) + текстурные координаты (u,v)
        private static readonly float[] CubeVertices =
        {
            // Position, Normal, TexCoords
            // Задняя грань (Z-)
            -0.5f,-0.5f,-0.5f,  0f, 0f,-1f,  0f, 0f,
             0.5f,-0.5f,-0.5f,  0f, 0f,-1f,  1f, 0f,
             0.5f, 0.5f,-0.5f,  0f, 0f,-1f,  1f, 1f,
            -0.5f, 0.5f,-0.5f,  0f, 0f,-1f,  0f, 1f,
            // Передняя грань (Z+)
            -0.5f,-0.5f, 0.5f,  0f, 0f, 1f,  0f, 0f,
             0.5f,-0.5f, 0.5f,  0f, 0f, 1f,  1f, 0f,
             0.5f, 0.5f, 0.5f,  0f, 0f, 1f,  1f, 1f,
            -0.5f, 0.5f, 0.5f,  0f, 0f, 1f,  0f, 1f,
            // Левая грань (X-)
            -0.5f, 0.5f, 0.5f, -1f, 0f, 0f,  1f, 0f,
            -0.5f, 0.5f,-0.5f, -1f, 0f, 0f,  1f, 1f,
            -0.5f,-0.5f,-0.5f, -1f, 0f, 0f,  0f, 1f,
            -0.5f,-0.5f, 0.5f, -1f, 0f, 0f,  0f, 0f,
            // Правая грань (X+)
             0.5f, 0.5f, 0.5f,  1f, 0f, 0f,  0f, 0f,
             0.5f, 0.5f,-0.5f,  1f, 0f, 0f,  0f, 1f,
             0.5f,-0.5f,-0.5f,  1f, 0f, 0f,  1f, 1f,
             0.5f,-0.5f, 0.5f,  1f, 0f, 0f,  1f, 0f,
            // Нижняя грань (Y-)
            -0.5f,-0.5f,-0.5f,  0f,-1f, 0f,  0f, 1f,
             0.5f,-0.5f,-0.5f,  0f,-1f, 0f,  1f, 1f,
             0.5f,-0.5f, 0.5f,  0f,-1f, 0f,  1f, 0f,
            -0.5f,-0.5f, 0.5f,  0f,-1f, 0f,  0f, 0f,
            // Верхняя грань (Y+)
            -0.5f, 0.5f,-0.5f,  0f, 1f, 0f,  0f, 0f,
             0.5f, 0.5f,-0.5f,  0f, 1f, 0f,  1f, 0f,
             0.5f, 0.5f, 0.5f,  0f, 1f, 0f,  1f, 1f,
            -0.5f, 0.5f, 0.5f,  0f, 1f, 0f,  0f, 1f,
        };

        private static readonly uint[] CubeIndices =
        {
             0, 1, 2,  2, 3, 0,   // Задняя
             4, 5, 6,  6, 7, 4,   // Передняя
             8, 9,10, 10,11, 8,   // Левая
            12,13,14, 14,15,12,   // Правая
            16,17,18, 18,19,16,   // Нижняя
            20,21,22, 22,23,20,   // Верхняя
        };

        private static readonly Vector3 LightDir  = new Vector3(-0.5f, -1f, -0.3f).Normalized();
        private static readonly Vector3 LightColor = Vector3.One;

        public BlockRenderer(string shaderDir)
        {
            _blockShader   = new ShaderProgram(
                Path.Combine(shaderDir, "basic.vert"),
                Path.Combine(shaderDir, "basic.frag"));

            _outlineShader = new ShaderProgram(
                Path.Combine(shaderDir, "outline.vert"),
                Path.Combine(shaderDir, "outline.frag"));

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                CubeVertices.Length * sizeof(float),
                CubeVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                CubeIndices.Length * sizeof(uint),
                CubeIndices, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        private int GetOrLoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            if (_textureCache.TryGetValue(path, out int texId)) return texId;

            texId = TextureManager.LoadTexture(path);
            _textureCache[path] = texId;
            return texId;
        }

        private ModelVAO GetOrLoadModel(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            if (_modelCache.TryGetValue(path, out var model)) return model;

            try
            {
                var data = ObjLoader.Load(path);
                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();
                int ebo = GL.GenBuffer();

                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, data.Vertices.Length * sizeof(float), data.Vertices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, data.Indices.Length * sizeof(uint), data.Indices, BufferUsageHint.StaticDraw);

                int stride = 8 * sizeof(float);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
                GL.EnableVertexAttribArray(2);

                GL.BindVertexArray(0);

                var newModel = new ModelVAO
                {
                    Vao = vao,
                    Vbo = vbo,
                    Ebo = ebo,
                    IndexCount = data.Indices.Length
                };
                _modelCache[path] = newModel;
                return newModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading model {path}: {ex.Message}");
                return null;
            }
        }

        public void RenderEntities(
            IEnumerable<Entity> entities,
            EditorCamera camera,
            Guid selectedId)
        {
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 proj = camera.GetProjectionMatrix();

            _blockShader.Use();
            _blockShader.SetMatrix4("view",        ref view);
            _blockShader.SetMatrix4("projection",  ref proj);
            _blockShader.SetVec3("lightDir",   LightDir.X,   LightDir.Y,   LightDir.Z);
            _blockShader.SetVec3("lightColor", LightColor.X, LightColor.Y, LightColor.Z);
            _blockShader.SetFloat("ambientStrength", 0.28f);
            _blockShader.SetBool("isPreview", false);
            _blockShader.SetFloat("alpha", 1.0f);

            foreach (var e in entities)
            {
                bool isSelected = (e.Id == selectedId);
                DrawEntity(e, isSelected);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        public void RenderPreview(Vec3 pos, ColorRGB color, EditorCamera camera, string texturePath, string modelPath)
        {
            if (pos == null) return;

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 proj = camera.GetProjectionMatrix();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _blockShader.Use();
            _blockShader.SetMatrix4("view",       ref view);
            _blockShader.SetMatrix4("projection", ref proj);
            _blockShader.SetVec3("lightDir",   LightDir.X,   LightDir.Y,   LightDir.Z);
            _blockShader.SetVec3("lightColor", LightColor.X, LightColor.Y, LightColor.Z);
            _blockShader.SetFloat("ambientStrength", 0.4f);
            _blockShader.SetBool("isSelected", false);
            _blockShader.SetBool("isPreview",  true);
            _blockShader.SetFloat("alpha", 0.5f);
            _blockShader.SetVec3("objectColor", color.R, color.G, color.B);

            var model = Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z);
            var normalMat = Matrix3.Identity;
            _blockShader.SetMatrix4("model",        ref model);
            _blockShader.SetMatrix3("normalMatrix", ref normalMat);

            int texId = GetOrLoadTexture(texturePath);
            if (texId > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texId);
                _blockShader.SetBool("useTexture", true);
            }
            else
            {
                _blockShader.SetBool("useTexture", false);
            }

            ModelVAO customModel = GetOrLoadModel(modelPath);
            if (customModel != null)
            {
                GL.BindVertexArray(customModel.Vao);
                GL.DrawElements(PrimitiveType.Triangles, customModel.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, CubeIndices.Length, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.Disable(EnableCap.Blend);
        }

        public void RenderOutline(Entity e, EditorCamera camera)
        {
            if (e == null) return;

            Matrix4 view  = camera.GetViewMatrix();
            Matrix4 proj  = camera.GetProjectionMatrix();
            
            var model = Matrix4.CreateScale(1.03f) *
                        Matrix4.CreateTranslation(e.Position.X, e.Position.Y, e.Position.Z);

            _outlineShader.Use();
            _outlineShader.SetMatrix4("model",      ref model);
            _outlineShader.SetMatrix4("view",       ref view);
            _outlineShader.SetMatrix4("projection", ref proj);
            _outlineShader.SetVec4("outlineColor", 1f, 0.85f, 0.1f, 1f);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(2.0f);
            GL.Disable(EnableCap.CullFace);

            string modelPath;
            e.Properties.TryGetValue("model_path", out modelPath);
            ModelVAO customModel = GetOrLoadModel(modelPath);

            if (customModel != null)
            {
                GL.BindVertexArray(customModel.Vao);
                GL.DrawElements(PrimitiveType.Triangles, customModel.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, CubeIndices.Length, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindVertexArray(0);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.CullFace);
        }

        private void DrawEntity(Entity e, bool selected)
        {
            var model = Matrix4.CreateTranslation(e.Position.X, e.Position.Y, e.Position.Z);
            var normalMat = new Matrix3(Matrix4.Transpose(Matrix4.Invert(model)));
            _blockShader.SetMatrix4("model",        ref model);
            _blockShader.SetMatrix3("normalMatrix", ref normalMat);
            _blockShader.SetVec3("objectColor",     e.Color.R, e.Color.G, e.Color.B);
            _blockShader.SetBool("isSelected",      selected);

            // Bind Texture if specified
            string texturePath;
            e.Properties.TryGetValue("texture_path", out texturePath);
            int texId = GetOrLoadTexture(texturePath);

            if (texId > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texId);
                _blockShader.SetBool("useTexture", true);
            }
            else
            {
                _blockShader.SetBool("useTexture", false);
            }

            // Bind Mesh (custom Model3D or standard Block)
            string modelPath = null;
            if (e.Type == EntityType.Model3D)
            {
                e.Properties.TryGetValue("model_path", out modelPath);
            }
            
            ModelVAO customModel = GetOrLoadModel(modelPath);
            if (customModel != null)
            {
                GL.BindVertexArray(customModel.Vao);
                GL.DrawElements(PrimitiveType.Triangles, customModel.IndexCount, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, CubeIndices.Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteVertexArray(_vao);
                GL.DeleteBuffer(_vbo);
                GL.DeleteBuffer(_ebo);
                
                foreach (var model in _modelCache.Values)
                {
                    GL.DeleteVertexArray(model.Vao);
                    GL.DeleteBuffer(model.Vbo);
                    GL.DeleteBuffer(model.Ebo);
                }

                foreach (var texId in _textureCache.Values)
                {
                    GL.DeleteTexture(texId);
                }

                _blockShader?.Dispose();
                _outlineShader?.Dispose();
                _disposed = true;
            }
        }
    }
}
