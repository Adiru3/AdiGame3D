using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Engine.Core.Entities;
using Engine.Core.Scene;
using Engine.Core.Rendering;
using Engine.Runtime.Physics;
using Engine.Runtime.Player;
using Engine.Runtime.Network;
using Engine.Runtime.Audio;
using Engine.Runtime.Forms;

namespace Engine.Runtime.GameLoop
{
    public class RuntimeWindow : GameWindow
    {
        public class ModelVAO
        {
            public int Vao;
            public int Vbo;
            public int Ebo;
            public int IndexCount;
        }

        // ─── Движок ───────────────────────────────────────────────────────
        private SceneManager     _scene;
        private PhysicsWorld     _physics;
        private PlayerController _player;
        private RuntimeCamera    _camera;
        private string           _levelPath;

        // ─── Рендер ───────────────────────────────────────────────────────
        private int          _blockVao, _blockVbo, _blockEbo;
        private int          _skyVao,   _skyVbo;
        private ShaderHelper _blockShader;
        private ShaderHelper _skyShader;
        private ShaderHelper _crosshairShader;
        private int          _crosshairVao, _crosshairVbo;

        private Dictionary<string, ModelVAO> _modelCache = new Dictionary<string, ModelVAO>();
        private Dictionary<string, int> _textureCache = new Dictionary<string, int>();

        // ─── Системы ──────────────────────────────────────────────────────
        private AudioManager     _audioManager;
        private CutsceneManager  _cutsceneManager;

        // ─── Настройки ────────────────────────────────────────────────────
        private float _masterVolume = 1.0f;
        private float _mouseSensitivity = 0.15f;

        // ─── Сеть ─────────────────────────────────────────────────────────
        private GameServer  _server;
        private GameClient  _netClient;
        private uint        _tick;

        // ─── Инпут ────────────────────────────────────────────────────────
        private bool    _mouseCaptured;
        private Vector2 _lastMouse;

        // ─── Геометрия куба ───────────────────────────────────────────────
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
             0, 1, 2,  2, 3, 0,
             4, 5, 6,  6, 7, 4,
             8, 9,10, 10,11, 8,
            12,13,14, 14,15,12,
            16,17,18, 18,19,16,
            20,21,22, 22,23,20,
        };

        // Скайбокс-куб (12 треугольников)
        private static readonly float[] SkyboxVerts =
        {
            -1,-1,-1,  1,-1,-1,  1, 1,-1,  1, 1,-1, -1, 1,-1, -1,-1,-1,
            -1,-1, 1,  1,-1, 1,  1, 1, 1,  1, 1, 1, -1, 1, 1, -1,-1, 1,
            -1, 1, 1, -1, 1,-1, -1,-1,-1, -1,-1,-1, -1,-1, 1, -1, 1, 1,
             1, 1, 1,  1, 1,-1,  1,-1,-1,  1,-1,-1,  1,-1, 1,  1, 1, 1,
            -1,-1,-1,  1,-1,-1,  1,-1, 1,  1,-1, 1, -1,-1, 1, -1,-1,-1,
            -1, 1,-1,  1, 1,-1,  1, 1, 1,  1, 1, 1, -1, 1, 1, -1, 1,-1,
        };

        public RuntimeWindow(string levelPath,
                             bool isHost   = false,
                             string hostIp = null,
                             int port      = 7777)
            : base(1280, 720, GraphicsMode.Default, "Adigame3D",
                   GameWindowFlags.Default,
                   DisplayDevice.Default, 3, 3,
                   GraphicsContextFlags.ForwardCompatible)
        {
            _levelPath = levelPath;
            VSync      = VSyncMode.On;
            CursorVisible = false;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Load
        // ═══════════════════════════════════════════════════════════════════

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0.35f, 0.55f, 0.85f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            string shaderDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "Shaders");

            try
            {
                _blockShader = new ShaderHelper(
                    Path.Combine(shaderDir, "runtime.vert"),
                    Path.Combine(shaderDir, "runtime.frag"));

                _skyShader = new ShaderHelper(
                    Path.Combine(shaderDir, "sky.vert"),
                    Path.Combine(shaderDir, "sky.frag"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Shader load error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            SetupBlockGeometry();
            SetupSkyGeometry();
            SetupCrosshair();

            // Загружаем сцену
            _scene = new SceneManager();
            if (File.Exists(_levelPath))
            {
                _scene.LoadScene(_levelPath);
                Title = "Adigame3D — " + _scene.CurrentScene.Name;
            }
            else
            {
                MessageBox.Show($"Level file not found:\n{_levelPath}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _physics = new PhysicsWorld(_scene);
            _camera  = new RuntimeCamera { Aspect = (float)Width / Height };
            _player  = new PlayerController(_physics);

            // Спавн игрока
            var spawn = _scene.CurrentScene.PlayerSpawn;
            _player.Respawn(spawn?.Position ?? new Vec3(0, 2, 0));

            var ms = Mouse.GetState();
            _lastMouse = new Vector2(ms.X, ms.Y);

            // Инициализация Аудио и Кат-сцен
            _audioManager = new AudioManager();
            _cutsceneManager = new CutsceneManager();
            _cutsceneManager.OnFinished += () =>
            {
                _camera.OverrideEyePosition = null;
                _camera.Fov = 70f;
                _mouseCaptured = true;
                CursorVisible = false;
            };

            // Запуск звука
            InitializeAudioSources();

            // Запуск кат-сцены, если есть waypoint-точки
            bool hasWaypoints = false;
            foreach (var ent in _scene.CurrentScene.Entities)
            {
                if (ent.Type == EntityType.CameraWaypoint)
                {
                    hasWaypoints = true;
                    break;
                }
            }

            if (hasWaypoints)
            {
                _mouseCaptured = false;
                CursorVisible = true;
                _cutsceneManager.Start(_scene.CurrentScene.Entities);
            }
        }

        private string GetAssetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            if (Path.IsPathRooted(relativePath)) return relativePath;

            string path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (File.Exists(path1)) return path1;

            string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", relativePath);
            if (File.Exists(path2)) return Path.GetFullPath(path2);

            return path1; // fallback
        }

        private void InitializeAudioSources()
        {
            if (_audioManager == null || !_audioManager.IsActive || _scene?.CurrentScene == null) return;

            _audioManager.ClearSources();

            foreach (var entity in _scene.CurrentScene.Entities)
            {
                if (entity.Type == EntityType.SoundPoint)
                {
                    string soundPath;
                    entity.Properties.TryGetValue("sound_path", out soundPath);
                    if (string.IsNullOrEmpty(soundPath)) continue;

                    string absPath = GetAssetAbsolutePath(soundPath);

                    float radius = 15f;
                    if (entity.Properties.TryGetValue("radius", out string rStr))
                        float.TryParse(rStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out radius);

                    bool looping = true;
                    if (entity.Properties.TryGetValue("looping", out string lStr))
                        bool.TryParse(lStr, out looping);

                    float volume = 1f;
                    if (entity.Properties.TryGetValue("volume", out string vStr))
                        float.TryParse(vStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out volume);

                    _audioManager.Play3DSound(absPath, entity.Position, looping, volume, radius);
                }
            }
        }

        private int GetOrLoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            if (_textureCache.TryGetValue(path, out int texId)) return texId;

            string absPath = GetAssetAbsolutePath(path);
            texId = TextureManager.LoadTexture(absPath);
            _textureCache[path] = texId;
            return texId;
        }

        private ModelVAO GetOrLoadModel(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_modelCache.TryGetValue(path, out var model)) return model;

            string absPath = GetAssetAbsolutePath(path);
            if (!File.Exists(absPath)) return null;

            try
            {
                var data = ObjLoader.Load(absPath);
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

        // ═══════════════════════════════════════════════════════════════════
        //  Update
        // ═══════════════════════════════════════════════════════════════════

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float dt = (float)Math.Min(e.Time, 0.05);
            _tick++;

            var kb = Keyboard.GetState();

            // Кнопка Escape открывает меню настроек (паузы)
            if (kb.IsKeyDown(Key.Escape))
            {
                _mouseCaptured = false;
                CursorVisible = true;

                using (var pauseForm = new PauseMenuForm(_masterVolume, _mouseSensitivity, Width, Height))
                {
                    if (pauseForm.ShowDialog() == DialogResult.Cancel && pauseForm.ExitRequest)
                    {
                        Close();
                        return;
                    }
                    else
                    {
                        // Применяем настройки
                        _masterVolume = pauseForm.MasterVolume;
                        _mouseSensitivity = pauseForm.MouseSensitivity;
                        _audioManager.SetMasterVolume(_masterVolume);

                        if (Width != pauseForm.SelectedWidth || Height != pauseForm.SelectedHeight)
                        {
                            Width = pauseForm.SelectedWidth;
                            Height = pauseForm.SelectedHeight;
                        }
                    }
                }

                if (!_cutsceneManager.IsActive)
                {
                    _mouseCaptured = true;
                    CursorVisible = false;
                }
                _lastMouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                return;
            }

            // Захват мыши по клику
            if (!_mouseCaptured && Mouse.GetState().IsButtonDown(MouseButton.Left) && !_cutsceneManager.IsActive)
            {
                _mouseCaptured = true;
                CursorVisible  = false;
            }

            // Повороты камеры
            if (_mouseCaptured && !_cutsceneManager.IsActive)
            {
                var ms    = Mouse.GetState();
                float dx  = ms.X - _lastMouse.X;
                float dy  = ms.Y - _lastMouse.Y;

                _camera.ApplyMouseDelta(dx, dy, _mouseSensitivity);

                var center = new System.Drawing.Point(
                    Bounds.Left + Width / 2, Bounds.Top + Height / 2);
                System.Windows.Forms.Cursor.Position = center;
                var newMs = Mouse.GetState();
                _lastMouse = new Vector2(newMs.X, newMs.Y);
            }

            // Кат-сцена
            if (_cutsceneManager.IsActive)
            {
                _cutsceneManager.Update(dt);
                _camera.OverrideEyePosition = new Vector3(_cutsceneManager.CurrentPos.X, _cutsceneManager.CurrentPos.Y, _cutsceneManager.CurrentPos.Z);
                _camera.Yaw = _cutsceneManager.CurrentYaw;
                _camera.Pitch = _cutsceneManager.CurrentPitch;
                _camera.Fov = _cutsceneManager.CurrentFov;

                // Блокируем движение игрока во время кат-сцены
                _player.InputForward = false;
                _player.InputBack    = false;
                _player.InputLeft    = false;
                _player.InputRight   = false;
                _player.InputJump    = false;
                _player.InputSprint  = false;
            }
            else
            {
                // Обычное управление игроком
                _player.InputForward = kb.IsKeyDown(Key.W);
                _player.InputBack    = kb.IsKeyDown(Key.S);
                _player.InputLeft    = kb.IsKeyDown(Key.A);
                _player.InputRight   = kb.IsKeyDown(Key.D);
                _player.InputJump    = kb.IsKeyDown(Key.Space);
                _player.InputSprint  = kb.IsKeyDown(Key.ShiftLeft);
                _player.ViewYaw      = _camera.Yaw;
            }

            _player.Update(dt);

            // Обновление Audio Listener позиций
            if (_audioManager != null && _audioManager.IsActive)
            {
                var eye = _camera.GetEyePosition(_player.Position);
                _audioManager.UpdateListener(new Vec3(eye.X, eye.Y, eye.Z), _camera.Front, Vector3.UnitY);
            }

            _server?.Update();
            _netClient?.Update();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Render
        // ═══════════════════════════════════════════════════════════════════

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (_scene == null || _player == null) { SwapBuffers(); return; }

            var sky = _scene.CurrentScene.SkyColor;
            GL.ClearColor(sky.R * 0.8f, sky.G * 0.85f, sky.B, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var view = _camera.GetViewMatrix(_player.Position);
            var proj = _camera.GetProjectionMatrix();

            RenderSky(view, proj, sky);
            RenderBlocks(view, proj);

            if (!_cutsceneManager.IsActive)
            {
                RenderCrosshair();
            }

            SwapBuffers();
        }

        private void RenderBlocks(Matrix4 view, Matrix4 proj)
        {
            _blockShader.Use();
            _blockShader.SetMatrix4("view",       ref view);
            _blockShader.SetMatrix4("projection", ref proj);
            _blockShader.SetVec3("lightDir",      -0.5f, -1f, -0.3f);
            _blockShader.SetVec3("lightColor",     1f, 0.97f, 0.9f);
            _blockShader.SetFloat("ambientStrength", 0.3f);

            var skyCol = _scene.CurrentScene.SkyColor;
            _blockShader.SetVec3("fogColor", skyCol.R * 0.85f, skyCol.G * 0.9f, skyCol.B);
            _blockShader.SetFloat("fogStart", 60f);
            _blockShader.SetFloat("fogEnd",   150f);

            var camPos = _camera.GetEyePosition(_player.Position);
            _blockShader.SetVec3("cameraPos", camPos.X, camPos.Y, camPos.Z);

            foreach (var entity in _scene.CurrentScene.Entities)
            {
                if ((int)entity.Type >= 100) continue; // Не рендерим вспомогательные хелперы

                var model = Matrix4.CreateTranslation(entity.Position.X, entity.Position.Y, entity.Position.Z);
                var normalMat = new Matrix3(Matrix4.Transpose(Matrix4.Invert(model)));
                _blockShader.SetMatrix4("model", ref model);
                _blockShader.SetMatrix3("normalMatrix", ref normalMat);
                _blockShader.SetVec3("objectColor", entity.Color.R, entity.Color.G, entity.Color.B);

                // Загрузка/биндинг текстур
                string texturePath;
                entity.Properties.TryGetValue("texture_path", out texturePath);
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

                // Отрисовка 3D-модели или обычного куба
                string modelPath = null;
                if (entity.Type == EntityType.Model3D)
                {
                    entity.Properties.TryGetValue("model_path", out modelPath);
                }

                ModelVAO customModel = GetOrLoadModel(modelPath);
                if (customModel != null)
                {
                    GL.BindVertexArray(customModel.Vao);
                    GL.DrawElements(PrimitiveType.Triangles, customModel.IndexCount, DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    GL.BindVertexArray(_blockVao);
                    GL.DrawElements(PrimitiveType.Triangles, CubeIndices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        private void RenderSky(Matrix4 view, Matrix4 proj, ColorRGB skyCol)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Disable(EnableCap.CullFace);
            _skyShader.Use();
            _skyShader.SetMatrix4("view",       ref view);
            _skyShader.SetMatrix4("projection", ref proj);
            _skyShader.SetVec3("skyColorTop",     skyCol.R * 0.55f, skyCol.G * 0.72f, skyCol.B);
            _skyShader.SetVec3("skyColorHorizon", 0.95f, 0.88f, 0.78f);
            _skyShader.SetVec3("sunDir",          -0.5f, -1f, -0.3f);

            GL.BindVertexArray(_skyVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Less);
        }

        private void RenderCrosshair()
        {
            GL.Disable(EnableCap.DepthTest);
            _crosshairShader.Use();
            GL.BindVertexArray(_crosshairVao);
            GL.LineWidth(2.0f);
            GL.DrawArrays(PrimitiveType.Lines, 0, 4);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Геометрия
        // ═══════════════════════════════════════════════════════════════════

        private void SetupBlockGeometry()
        {
            _blockVao = GL.GenVertexArray();
            _blockVbo = GL.GenBuffer();
            _blockEbo = GL.GenBuffer();

            GL.BindVertexArray(_blockVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _blockVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                CubeVertices.Length * sizeof(float),
                CubeVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _blockEbo);
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

        private void SetupSkyGeometry()
        {
            _skyVao = GL.GenVertexArray();
            _skyVbo = GL.GenBuffer();
            GL.BindVertexArray(_skyVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _skyVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                SkyboxVerts.Length * sizeof(float),
                SkyboxVerts, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
        }

        private void SetupCrosshair()
        {
            float s = 0.018f;
            float ar = (float)Height / Math.Max(1, Width);
            float[] verts = { -s, 0f, 0f,  s, 0f, 0f,  0f, -s*ar, 0f,  0f, s*ar, 0f };

            _crosshairVao = GL.GenVertexArray();
            _crosshairVbo = GL.GenBuffer();
            GL.BindVertexArray(_crosshairVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _crosshairVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float),
                verts, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);

            string vs = "#version 330 core\nlayout(location=0) in vec3 aPos;\nvoid main(){ gl_Position=vec4(aPos,1.0); }";
            string fs = "#version 330 core\nout vec4 c;\nvoid main(){ c=vec4(1.0,1.0,1.0,0.85); }";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs); GL.CompileShader(v);
            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs); GL.CompileShader(f);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v); GL.AttachShader(prog, f);
            GL.LinkProgram(prog);
            GL.DeleteShader(v); GL.DeleteShader(f);
            _crosshairShader = new ShaderHelper(prog);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Resize
        // ═══════════════════════════════════════════════════════════════════

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            if (_camera != null)
                _camera.Aspect = (float)Width / Math.Max(1, Height);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  Cleanup
        // ═══════════════════════════════════════════════════════════════════

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (_blockVao > 0) { GL.DeleteVertexArray(_blockVao); GL.DeleteBuffer(_blockVbo); GL.DeleteBuffer(_blockEbo); }
            if (_skyVao   > 0) { GL.DeleteVertexArray(_skyVao);   GL.DeleteBuffer(_skyVbo); }
            if (_crosshairVao > 0) { GL.DeleteVertexArray(_crosshairVao); GL.DeleteBuffer(_crosshairVbo); }

            foreach (var model in _modelCache.Values)
            {
                GL.DeleteVertexArray(model.Vao);
                GL.DeleteBuffer(model.Vbo);
                GL.DeleteBuffer(model.Ebo);
            }
            _modelCache.Clear();

            foreach (var texId in _textureCache.Values)
            {
                GL.DeleteTexture(texId);
            }
            _textureCache.Clear();

            _blockShader?.Dispose();
            _skyShader?.Dispose();
            _crosshairShader?.Dispose();
            _audioManager?.Dispose();

            _server?.Dispose();
            _netClient?.Dispose();
        }
    }

    internal class ShaderHelper : IDisposable
    {
        public int Handle { get; }
        private bool _disposed;

        public ShaderHelper(string vertPath, string fragPath)
        {
            string vs = File.ReadAllText(vertPath);
            string fs = File.ReadAllText(fragPath);
            Handle = BuildProgram(vs, fs);
        }

        public ShaderHelper(int existingProgram)
        {
            Handle = existingProgram;
        }

        private static int BuildProgram(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs); GL.CompileShader(v);
            int ok;
            GL.GetShader(v, ShaderParameter.CompileStatus, out ok);
            if (ok == 0) throw new Exception("VS error: " + GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs); GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out ok);
            if (ok == 0) throw new Exception("FS error: " + GL.GetShaderInfoLog(f));

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v); GL.AttachShader(prog, f);
            GL.LinkProgram(prog);
            GL.DeleteShader(v); GL.DeleteShader(f);
            return prog;
        }

        public void Use() => GL.UseProgram(Handle);

        private int L(string n) => GL.GetUniformLocation(Handle, n);
        public void SetFloat(string n, float v)            => GL.Uniform1(L(n), v);
        public void SetBool(string n, bool v)              => GL.Uniform1(L(n), v ? 1 : 0);
        public void SetVec3(string n, float x, float y, float z) => GL.Uniform3(L(n), x, y, z);
        public void SetMatrix4(string n, ref Matrix4 m)    => GL.UniformMatrix4(L(n), false, ref m);
        public void SetMatrix3(string n, ref Matrix3 m)    => GL.UniformMatrix3(L(n), false, ref m);

        public void Dispose()
        {
            if (!_disposed) { GL.DeleteProgram(Handle); _disposed = true; }
        }
    }
}
