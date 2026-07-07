using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Editor.Rendering
{
    /// <summary>
    /// Компилирует и хранит OpenGL шейдерную программу.
    /// </summary>
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; private set; }
        private bool _disposed;

        public ShaderProgram(string vertPath, string fragPath)
        {
            string vertSrc = File.ReadAllText(vertPath);
            string fragSrc = File.ReadAllText(fragPath);

            int vert = CompileShader(ShaderType.VertexShader,   vertSrc);
            int frag = CompileShader(ShaderType.FragmentShader, fragSrc);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vert);
            GL.AttachShader(Handle, frag);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linked);
            if (linked == 0)
            {
                string log = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Shader link error:\n{log}");
            }

            GL.DeleteShader(vert);
            GL.DeleteShader(frag);
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);
            if (compiled == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compile error ({type}):\n{log}");
            }
            return shader;
        }

        public void Use() => GL.UseProgram(Handle);

        // ─── Uniform helpers ───────────────────────────────────────────────

        private int Loc(string name) => GL.GetUniformLocation(Handle, name);

        public void SetInt(string name, int val)                           => GL.Uniform1(Loc(name), val);
        public void SetFloat(string name, float val)                       => GL.Uniform1(Loc(name), val);
        public void SetBool(string name, bool val)                         => GL.Uniform1(Loc(name), val ? 1 : 0);
        public void SetVec3(string name, float x, float y, float z)       => GL.Uniform3(Loc(name), x, y, z);
        public void SetVec4(string name, float x, float y, float z, float w) => GL.Uniform4(Loc(name), x, y, z, w);

        public void SetMatrix4(string name, ref OpenTK.Matrix4 matrix)
            => GL.UniformMatrix4(Loc(name), false, ref matrix);

        public void SetMatrix3(string name, ref OpenTK.Matrix3 matrix)
            => GL.UniformMatrix3(Loc(name), false, ref matrix);

        // ─── IDisposable ───────────────────────────────────────────────────

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteProgram(Handle);
                _disposed = true;
            }
        }
    }
}
