using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Core.Rendering
{
    public static class TextureManager
    {
        public static int LoadTexture(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Если файл не найден, возвращаем 0 или кидаем ошибку
                return 0;
            }

            try
            {
                int textureId = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, textureId);

                using (var bitmap = new Bitmap(filePath))
                {
                    var data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    // Загружаем текстуру в GPU
                    GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        bitmap.Width,
                        bitmap.Height,
                        0,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, // GDI+ хранит как BGRA
                        PixelType.UnsignedByte,
                        data.Scan0);

                    bitmap.UnlockBits(data);
                }

                // Генерация Mipmaps для сглаживания при отдалении
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                // Настройки фильтрации текстур
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                return textureId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading texture {filePath}: {ex.Message}");
                return 0;
            }
        }
    }
}
