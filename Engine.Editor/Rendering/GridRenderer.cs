using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace Engine.Editor.Rendering
{
    /// <summary>
    /// Рисует бесконечную координатную сетку на плоскости Y=0.
    /// Большой flat quad (2000×2000) следует за камерой по XZ,
    /// сетка рисуется через fract() в фрагментном шейдере.
    /// </summary>
    public class GridRenderer : IDisposable
    {
        private int           _vao, _vbo;
        private ShaderProgram _shader;
        private bool          _disposed;

        private const float S = 1000f; // Половина размера quad

        // Большой плоский quad (Y=0), вершины в мировых координатах
        // Будут сдвинуты в шейдере на позицию камеры
        private static readonly float[] QuadVertices =
        {
            -S, 0f, -S,
             S, 0f, -S,
             S, 0f,  S,
            -S, 0f,  S,
        };

        private static readonly uint[] QuadIndices =
        {
            0, 1, 2,  2, 3, 0
        };

        private int _ebo;

        public GridRenderer(string shaderDir)
        {
            _shader = new ShaderProgram(
                System.IO.Path.Combine(shaderDir, "grid.vert"),
                System.IO.Path.Combine(shaderDir, "grid.frag"));

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                QuadVertices.Length * sizeof(float),
                QuadVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                QuadIndices.Length * sizeof(uint),
                QuadIndices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public void Render(EditorCamera camera)
        {
            var view = camera.GetViewMatrix();
            var proj = camera.GetProjectionMatrix();
            var eye  = camera.Position;

            _shader.Use();
            _shader.SetMatrix4("view",       ref view);
            _shader.SetMatrix4("projection", ref proj);
            _shader.SetVec3("cameraPos", eye.X, eye.Y, eye.Z);
            _shader.SetFloat("far", camera.Far);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length,
                DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteVertexArray(_vao);
                GL.DeleteBuffer(_vbo);
                GL.DeleteBuffer(_ebo);
                _shader?.Dispose();
                _disposed = true;
            }
        }
    }
}
