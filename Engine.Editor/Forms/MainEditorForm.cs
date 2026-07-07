using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Engine.Core.Entities;
using Engine.Core.Scene;
using Engine.Editor.Editor;
using Engine.Editor.Rendering;

namespace Engine.Editor.Forms
{
    public partial class MainEditorForm : Form
    {
        // в”Ђв”Ђв”Ђ OpenGL в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
        private OpenTK.GLControl    _glControl;
        private bool                _glReady;

        // в”Ђв”Ђв”Ђ Р РµРЅРґРµСЂРёРЅРі в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
        private GridRenderer        _grid;
        private BlockRenderer       _blockRenderer;
        private EditorCamera        _camera;

        // в”Ђв”Ђв”Ђ Р›РѕРіРёРєР° в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
        private SceneManager        _sceneManager;
        private EditorStateMachine  _editorSM;
        private string              _currentFilePath;
        private bool                _unsavedChanges;
        private Guid                _selectedId = Guid.Empty;

        // в”Ђв”Ђв”Ђ РРіСЂРѕРІРѕР№ С†РёРєР» в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
        private Timer               _renderTimer;
        private Stopwatch           _stopwatch = Stopwatch.StartNew();
        private double              _lastTime;

        // в”Ђв”Ђв”Ђ UI СЌР»РµРјРµРЅС‚С‹ в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
        private Panel               _leftPanel;
        private Panel               _rightPanel;
        private ListBox             _blockPalette;
        private PropertyGrid        _propertyGrid;
        private ToolStrip           _toolStrip;
        private StatusStrip         _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _statsLabel;
        private ToolStripButton     _btnPlay;

        public MainEditorForm()
        {
            InitializeComponent();
            SetupUI();
            SetupGL();
            SetupEngine();
            SetupTimer();

            Text    = "Adigame3D вЂ” Editor";
            MinimumSize = new Size(1024, 640);
            StartPosition = FormStartPosition.CenterScreen;
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  UI
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void SetupUI()
        {
            // в”Ђв”Ђ РњРµРЅСЋ в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
            var menuStrip = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New",          null, (s,e) => OnNew());
            fileMenu.DropDownItems.Add("Open...",      null, (s,e) => OnOpen());
            fileMenu.DropDownItems.Add("Save",         null, (s,e) => OnSave());
            fileMenu.DropDownItems.Add("Save As...",   null, (s,e) => OnSaveAs());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Export Game...", null, (s,e) => OnExport());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit",         null, (s,e) => Close());

            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.Add("Undo\tCtrl+Z", null, (s,e) => OnUndo());
            editMenu.DropDownItems.Add("Redo\tCtrl+Y", null, (s,e) => OnRedo());
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Clear Scene",  null, (s,e) => OnClearScene());

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            // в”Ђв”Ђ РўСѓР»Р±Р°СЂ в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
            _toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };

            var btnPlace  = ModeButton("вњЏ Place",  EditorMode.Place,  "Place blocks (1)");
            var btnSelect = ModeButton("в†– Select", EditorMode.Select, "Select & edit (2)");
            var btnDelete = ModeButton("вњ‚ Delete", EditorMode.Delete, "Delete blocks (3)");
            var btnUndo   = new ToolStripButton("в†© Undo")  { ToolTipText = "Ctrl+Z" };
            var btnRedo   = new ToolStripButton("в†Є Redo")  { ToolTipText = "Ctrl+Y" };
            btnUndo.Click += (s,e) => OnUndo();
            btnRedo.Click += (s,e) => OnRedo();

            _btnPlay = new ToolStripButton("в–¶  Play") {
                ToolTipText = "Launch Runtime (F5)",
                BackColor   = Color.FromArgb(40, 180, 80)
            };
            _btnPlay.Click += (s,e) => LaunchRuntime();

            var btnSave = new ToolStripButton("рџ’ѕ Save") { ToolTipText = "Ctrl+S" };
            btnSave.Click += (s,e) => OnSave();

            _toolStrip.Items.Add(btnPlace);
            _toolStrip.Items.Add(btnSelect);
            _toolStrip.Items.Add(btnDelete);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(btnUndo);
            _toolStrip.Items.Add(btnRedo);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(btnSave);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnPlay);

            Controls.Add(_toolStrip);

            // в”Ђв”Ђ Р›РµРІР°СЏ РїР°РЅРµР»СЊ (РїР°Р»РёС‚СЂР° Р±Р»РѕРєРѕРІ) в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
            _leftPanel = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 180,
                BackColor = Color.FromArgb(35, 35, 40)
            };

            var paletteLabel = new Label
            {
                Text      = "  Block Palette",
                Dock      = DockStyle.Top,
                Height    = 28,
                ForeColor = Color.FromArgb(180, 180, 200),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(28, 28, 33)
            };

            _blockPalette = new ListBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.FromArgb(35, 35, 40),
                ForeColor   = Color.FromArgb(200, 200, 210),
                BorderStyle = BorderStyle.None,
                Font        = new Font("Segoe UI", 9f),
                ItemHeight  = 24
            };

            // Р—Р°РїРѕР»РЅСЏРµРј РїР°Р»РёС‚СЂСѓ
            foreach (EntityType t in Enum.GetValues(typeof(EntityType)))
            {
                string icon = t == EntityType.PlayerSpawn ? "вљ‘" :
                              t == EntityType.Light       ? "вЂ" :
                              t == EntityType.Trigger     ? "вљЎ" :
                              t == EntityType.KillZone    ? "в " :
                              t == EntityType.Checkpoint  ? "в­ђ" : "в– ";
                _blockPalette.Items.Add($"  {icon}  {t}");
            }
            _blockPalette.SelectedIndex = 0;
            _blockPalette.SelectedIndexChanged += OnPaletteSelectionChanged;

            _leftPanel.Controls.Add(_blockPalette);
            _leftPanel.Controls.Add(paletteLabel);
            Controls.Add(_leftPanel);

            // в”Ђв”Ђ РџСЂР°РІР°СЏ РїР°РЅРµР»СЊ (СЃРІРѕР№СЃС‚РІР°) в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
            _rightPanel = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 230,
                BackColor = Color.FromArgb(35, 35, 40)
            };

            var propsLabel = new Label
            {
                Text      = "  Properties",
                Dock      = DockStyle.Top,
                Height    = 28,
                ForeColor = Color.FromArgb(180, 180, 200),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(28, 28, 33)
            };

            _propertyGrid = new PropertyGrid
            {
                Dock             = DockStyle.Fill,
                BackColor        = Color.FromArgb(35, 35, 40),
                LineColor        = Color.FromArgb(50, 50, 60),
                CategoryForeColor= Color.FromArgb(130, 160, 210),
                ViewBackColor    = Color.FromArgb(38, 38, 45),
                ViewForeColor    = Color.FromArgb(210, 210, 220),
                HelpVisible      = false,
                ToolbarVisible   = false
            };
            _propertyGrid.PropertyValueChanged += OnPropertyChanged;

            _rightPanel.Controls.Add(_propertyGrid);
            _rightPanel.Controls.Add(propsLabel);
            Controls.Add(_rightPanel);

            // в”Ђв”Ђ РЎС‚Р°С‚СѓСЃРЅР°СЏ СЃС‚СЂРѕРєР° в”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђв”Ђ
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready") { Spring = false };
            _statsLabel  = new ToolStripStatusLabel("0 entities")
            {
                Alignment = ToolStripItemAlignment.Right
            };
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(_statsLabel);
            Controls.Add(_statusStrip);
        }

        private ToolStripButton ModeButton(string text, EditorMode mode, string tip)
        {
            var btn = new ToolStripButton(text) { ToolTipText = tip };
            btn.Click += (s, e) =>
            {
                _editorSM?.SetMode(mode);
                SetStatus($"Mode: {mode}");
            };
            return btn;
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  OpenGL Control
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void SetupGL()
        {
            var gfxMode = new GraphicsMode(
                color:   new ColorFormat(8, 8, 8, 8),
                depth:   24,
                stencil: 8,
                samples: 4   // MSAA 4x
            );

            _glControl = new OpenTK.GLControl(gfxMode, 3, 3, GraphicsContextFlags.Default)
            {
                Dock = DockStyle.Fill
            };

            _glControl.Load        += OnGLLoad;
            _glControl.Paint       += OnGLPaint;
            _glControl.Resize      += OnGLResize;
            _glControl.MouseDown   += OnGLMouseDown;
            _glControl.MouseUp     += OnGLMouseUp;
            _glControl.MouseMove   += OnGLMouseMove;
            _glControl.MouseWheel  += OnGLMouseWheel;
            _glControl.KeyDown     += OnGLKeyDown;
            _glControl.KeyUp       += OnGLKeyUp;
            _glControl.KeyPress    += OnGLKeyPress;

            // GLControl РїРѕРјРµС‰Р°РµС‚СЃСЏ РІ С†РµРЅС‚СЂ РјРµР¶РґСѓ Р»РµРІРѕР№ Рё РїСЂР°РІРѕР№ РїР°РЅРµР»СЏРјРё
            var centerPanel = new Panel { Dock = DockStyle.Fill };
            centerPanel.Controls.Add(_glControl);
            Controls.Add(centerPanel);

            // РџРѕСЂСЏРґРѕРє РґРѕР±Р°РІР»РµРЅРёСЏ Controls РІР°Р¶РµРЅ РґР»СЏ Dock
            Controls.SetChildIndex(centerPanel, 0);
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Р”РІРёР¶РѕРє
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void SetupEngine()
        {
            _sceneManager = new SceneManager();
            _camera       = new EditorCamera();
            _editorSM     = new EditorStateMachine(_sceneManager, _camera);

            _editorSM.EntitySelected   += e =>
            {
                _selectedId = e.Id;
                _propertyGrid.SelectedObject = new EntityProxy(e, () => {
                    _unsavedChanges = true;
                    UpdateTitle();
                });
            };
            _editorSM.SelectionCleared += () =>
            {
                _selectedId = Guid.Empty;
                _propertyGrid.SelectedObject = null;
            };
            _editorSM.SceneModified    += () =>
            {
                _unsavedChanges = true;
                UpdateTitle();
                UpdateStats();
            };

            // Р”РѕР±Р°РІР»СЏРµРј СЃРїР°РІРЅ РёРіСЂРѕРєР° РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ
            _sceneManager.AddEntity(EntityType.PlayerSpawn, new Vec3(0, 0, 0));
        }

        private void SetupTimer()
        {
            _renderTimer = new Timer { Interval = 16 }; // ~60 FPS
            _renderTimer.Tick += (s, e) =>
            {
                double now  = _stopwatch.Elapsed.TotalSeconds;
                float  dt   = (float)(now - _lastTime);
                _lastTime   = now;

                if (_glReady && _camera != null)
                {
                    _camera.Update(dt);
                    _glControl.Invalidate();
                }
            };
            _renderTimer.Start();
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  OpenGL СЃРѕР±С‹С‚РёСЏ
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void OnGLLoad(object sender, EventArgs e)
        {
            _glControl.MakeCurrent();

            GL.ClearColor(0.14f, 0.14f, 0.18f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.Multisample);

            string shaderDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "Shaders");

            try
            {
                _grid          = new GridRenderer(shaderDir);
                _blockRenderer = new BlockRenderer(shaderDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Shader error:\n{ex.Message}", "GL Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            _camera.Aspect = (float)_glControl.Width / Math.Max(1, _glControl.Height);
            _glReady = true;
        }

        private void OnGLPaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!_glReady) return;
            _glControl.MakeCurrent();

            var sky = _sceneManager.CurrentScene.SkyColor;
            GL.ClearColor(sky.R * 0.15f, sky.G * 0.15f, sky.B * 0.2f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // РЎРµС‚РєР° (СЂРёСЃСѓРµС‚СЃСЏ РїРµСЂРІРѕР№, СЃ depth write off)
            _grid.Render(_camera);

            // Р‘Р»РѕРєРё СЃС†РµРЅС‹
            _blockRenderer.RenderEntities(
                _sceneManager.CurrentScene.Entities,
                _camera,
                _selectedId);

            // Outline РІС‹РґРµР»РµРЅРЅРѕРіРѕ Р±Р»РѕРєР°
            if (_selectedId != Guid.Empty)
            {
                var sel = _sceneManager.FindById(_selectedId);
                _blockRenderer.RenderOutline(sel, _camera);
            }

            // РџСЂРµРІСЊСЋ Р±Р»РѕРєР° РІ СЂРµР¶РёРјРµ Place
            if (_editorSM?.CurrentMode == EditorMode.Place &&
                _editorSM.PreviewPosition != null)
            {
                var col = EntityTypeColors.GetColor(_editorSM.SelectedBlockType);
                _blockRenderer.RenderPreview(_editorSM.PreviewPosition, col, _camera, null, null);
            }

            _glControl.SwapBuffers();
        }

        private void OnGLResize(object sender, EventArgs e)
        {
            if (!_glReady) return;
            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
            _camera.Aspect = (float)_glControl.Width / Math.Max(1, _glControl.Height);
            if (_editorSM != null)
            {
                _editorSM.ViewWidth  = _glControl.Width;
                _editorSM.ViewHeight = _glControl.Height;
            }
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Р’РІРѕРґ
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void OnGLMouseDown(object sender, MouseEventArgs e)
        {
            _glControl.Focus();

            if (e.Button == MouseButtons.Right)
            {
                _camera.BeginMouseLook(e.X, e.Y);
                Cursor.Hide();
            }
            else if (e.Button == MouseButtons.Left && !_camera.IsMouseLooking)
            {
                _editorSM?.OnLeftClick(e.X, e.Y);
            }
        }

        private void OnGLMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _camera.EndMouseLook();
                Cursor.Show();
            }
        }

        private void OnGLMouseMove(object sender, MouseEventArgs e)
        {
            _camera.OnMouseMove(e.X, e.Y);
            _editorSM?.OnMouseMove(e.X, e.Y);
        }

        private void OnGLMouseWheel(object sender, MouseEventArgs e)
        {
            _camera.OnScroll(e.Delta / 120f);
        }

        private void OnGLKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { OnUndo(); return; }
            if (e.Control && e.KeyCode == Keys.Y) { OnRedo(); return; }
            if (e.Control && e.KeyCode == Keys.S) { OnSave(); return; }
            switch (e.KeyCode)
            {
                case Keys.W:      _camera.KeyW     = true;  break;
                case Keys.S:      _camera.KeyS     = true;  break;
                case Keys.A:      _camera.KeyA     = true;  break;
                case Keys.D:      _camera.KeyD     = true;  break;
                case Keys.Q:      _camera.KeyQ     = true;  break;
                case Keys.E:      _camera.KeyE     = true;  break;
                case Keys.ShiftKey:_camera.KeyShift= true;  break;

                // Р“РѕСЂСЏС‡РёРµ РєР»Р°РІРёС€Рё СЂРµР¶РёРјРѕРІ
                case Keys.D1: _editorSM?.SetMode(EditorMode.Place);  SetStatus("Mode: Place");  break;
                case Keys.D2: _editorSM?.SetMode(EditorMode.Select); SetStatus("Mode: Select"); break;
                case Keys.D3: _editorSM?.SetMode(EditorMode.Delete); SetStatus("Mode: Delete"); break;


                case Keys.Delete:
                    if (_selectedId != Guid.Empty)
                    {
                        _sceneManager.RemoveEntity(_selectedId);
                        _selectedId = Guid.Empty;
                        _propertyGrid.SelectedObject = null;
                        _unsavedChanges = true;
                        UpdateTitle();
                        UpdateStats();
                    }
                    break;
            }
        }

        private void OnGLKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:      _camera.KeyW     = false; break;
                case Keys.S:      _camera.KeyS     = false; break;
                case Keys.A:      _camera.KeyA     = false; break;
                case Keys.D:      _camera.KeyD     = false; break;
                case Keys.Q:      _camera.KeyQ     = false; break;
                case Keys.E:      _camera.KeyE     = false; break;
                case Keys.ShiftKey:_camera.KeyShift= false; break;
            }
        }

        private void OnGLKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) { }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  РџР°Р»РёС‚СЂР° Рё СЃРІРѕР№СЃС‚РІР°
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void OnPaletteSelectionChanged(object sender, EventArgs e)
        {
            if (_blockPalette.SelectedIndex < 0) return;
            var types = (EntityType[])Enum.GetValues(typeof(EntityType));
            if (_blockPalette.SelectedIndex < types.Length)
                _editorSM.SelectedBlockType = types[_blockPalette.SelectedIndex];
        }

        private void OnPropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            _unsavedChanges = true;
            UpdateTitle();
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  РљРѕРјР°РЅРґС‹ File/Edit
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void OnNew()
        {
            if (!ConfirmUnsaved()) return;
            _sceneManager.NewScene();
            _sceneManager.AddEntity(EntityType.PlayerSpawn, new Vec3(0, 0, 0));
            _currentFilePath = null;
            _unsavedChanges  = false;
            _selectedId      = Guid.Empty;
            _propertyGrid.SelectedObject = null;
            UpdateTitle();
            UpdateStats();
            SetStatus("New scene created.");
        }

        private void OnOpen()
        {
            if (!ConfirmUnsaved()) return;
            using (var dlg = new OpenFileDialog
            {
                Title  = "Open Level",
                Filter = "Level files (*.json)|*.json|All files (*.*)|*.*"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    _sceneManager.LoadScene(dlg.FileName);
                    _currentFilePath = dlg.FileName;
                    _unsavedChanges  = false;
                    _selectedId      = Guid.Empty;
                    _propertyGrid.SelectedObject = null;
                    UpdateTitle();
                    UpdateStats();
                    SetStatus($"Loaded: {Path.GetFileName(dlg.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnSave()
        {
            if (_currentFilePath == null) { OnSaveAs(); return; }
            try
            {
                _sceneManager.SaveScene(_currentFilePath);
                _unsavedChanges = false;
                UpdateTitle();
                SetStatus($"Saved: {Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSaveAs()
        {
            using (var dlg = new SaveFileDialog
            {
                Title      = "Save Level As",
                Filter     = "Level files (*.json)|*.json",
                DefaultExt = "json",
                FileName   = _sceneManager.CurrentScene.Name
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                _currentFilePath = dlg.FileName;
                OnSave();
            }
        }

        private void OnUndo()
        {
            _editorSM?.Undo();
            UpdateStats();
        }

        private void OnRedo()
        {
            _editorSM?.Redo();
            UpdateStats();
        }

        private void OnClearScene()
        {
            if (MessageBox.Show("Clear all entities?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            _sceneManager.NewScene();
            _sceneManager.AddEntity(EntityType.PlayerSpawn, new Vec3(0, 0, 0));
            _selectedId = Guid.Empty;
            _propertyGrid.SelectedObject = null;
            _unsavedChanges = true;
            UpdateTitle();
            UpdateStats();
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Р—Р°РїСѓСЃРє Runtime
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private string FindSolutionDir()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, "Engine.Runtime")) &&
                    Directory.Exists(Path.Combine(dir, "Engine.Editor")))
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        private void LaunchRuntime()
        {
            string tmpDir   = Path.Combine(Path.GetTempPath(), "Adigame3d_Runtime");
            string levelFile = Path.Combine(tmpDir, "level.json");
            Directory.CreateDirectory(tmpDir);
            _sceneManager.SaveScene(levelFile);

            string runtimeExe = null;
            string solDir = FindSolutionDir();
            if (solDir != null)
            {
                string[] paths = new[]
                {
                    Path.Combine(solDir, "Engine.Runtime", "bin", "Debug", "net461", "Engine.Runtime.exe"),
                    Path.Combine(solDir, "Engine.Runtime", "bin", "Release", "net461", "Engine.Runtime.exe")
                };
                foreach (var p in paths)
                {
                    if (File.Exists(p)) { runtimeExe = p; break; }
                }
            }

            if (runtimeExe == null)
            {
                runtimeExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Engine.Runtime.exe");
            }

            if (!File.Exists(runtimeExe))
            {
                MessageBox.Show(
                    "Engine.Runtime.exe not found.\nBuild the Runtime project first.",
                    "Runtime Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName  = runtimeExe,
                    Arguments = $"\"{levelFile}\"",
                    UseShellExecute = true
                });
                SetStatus("Runtime launched.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch runtime:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Р­РєСЃРїРѕСЂС‚ РёРіСЂС‹
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private void OnExport()
        {
            using (var dlg = new FolderBrowserDialog
            {
                Description = "Choose export folder"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                ExportGame(dlg.SelectedPath);
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
            foreach (string folder in Directory.GetDirectories(sourceDir))
            {
                string dest = Path.Combine(destinationDir, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        private void ExportGame(string outDir)
        {
            string editorDir = AppDomain.CurrentDomain.BaseDirectory;
            string runtimeExePath = null;

            string solDir = FindSolutionDir();
            if (solDir != null)
            {
                string[] paths = new[]
                {
                    Path.Combine(solDir, "Engine.Runtime", "bin", "Release", "net461", "Engine.Runtime.exe"),
                    Path.Combine(solDir, "Engine.Runtime", "bin", "Debug", "net461", "Engine.Runtime.exe")
                };
                foreach (var p in paths)
                {
                    if (File.Exists(p)) { runtimeExePath = p; break; }
                }
            }

            if (runtimeExePath == null)
            {
                runtimeExePath = Path.Combine(editorDir, "Engine.Runtime.exe");
            }

            if (!File.Exists(runtimeExePath) && solDir != null)
            {
                string runtimeProj = Path.Combine(solDir, "Engine.Runtime", "Engine.Runtime.csproj");
                if (File.Exists(runtimeProj))
                {
                    SetStatus("Compiling Runtime... please wait.");
                    Application.DoEvents();
                    try
                    {
                        var psiBuild = new ProcessStartInfo("dotnet")
                        {
                            Arguments = $"build \"{runtimeProj}\" -c Release",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };
                        var procBuild = Process.Start(psiBuild);
                        procBuild.WaitForExit();
                        
                        string[] paths = new[]
                        {
                            Path.Combine(solDir, "Engine.Runtime", "bin", "Release", "net461", "Engine.Runtime.exe"),
                            Path.Combine(solDir, "Engine.Runtime", "bin", "Debug", "net461", "Engine.Runtime.exe")
                        };
                        foreach (var p in paths)
                        {
                            if (File.Exists(p)) { runtimeExePath = p; break; }
                        }
                    }
                    catch { }
                }
            }

            if (!File.Exists(runtimeExePath))
            {
                MessageBox.Show(
                    "Engine.Runtime.exe not found and could not be compiled.\n" +
                    "Make sure the Runtime is built or Engine.Runtime.exe is present next to the Editor.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sourceDir = Path.GetDirectoryName(runtimeExePath);
            string[] filesToCopy = new[]
            {
                "Engine.Runtime.exe",
                "Engine.Runtime.exe.config",
                "Engine.Core.dll",
                "OpenTK.dll",
                "Newtonsoft.Json.dll",
                "LiteNetLib.dll"
            };

            SetStatus("Exporting... please wait.");
            Application.DoEvents();

            try
            {
                Directory.CreateDirectory(outDir);

                foreach (var fileName in filesToCopy)
                {
                    string srcPath = Path.Combine(sourceDir, fileName);
                    if (!File.Exists(srcPath))
                    {
                        srcPath = Path.Combine(editorDir, fileName);
                    }

                    if (File.Exists(srcPath))
                    {
                        string destPath = Path.Combine(outDir, fileName);
                        File.Copy(srcPath, destPath, true);
                    }
                }

                // Copy Shaders
                string destShaderDir = Path.Combine(outDir, "Resources", "Shaders");
                Directory.CreateDirectory(destShaderDir);

                string[] shaders = new[]
                {
                    "runtime.vert",
                    "runtime.frag",
                    "sky.vert",
                    "sky.frag"
                };

                string shaderSourceDir = null;
                string[] possibleShaderDirs = new[]
                {
                    Path.Combine(sourceDir, "Resources", "Shaders"),
                    Path.Combine(editorDir, "Resources", "Shaders"),
                    solDir != null ? Path.Combine(solDir, "Engine.Runtime", "Resources", "Shaders") : null
                };

                foreach (var sDir in possibleShaderDirs)
                {
                    if (sDir != null && Directory.Exists(sDir))
                    {
                        if (File.Exists(Path.Combine(sDir, "runtime.vert")))
                        {
                            shaderSourceDir = sDir;
                            break;
                        }
                    }
                }

                if (shaderSourceDir != null)
                {
                    foreach (var shader in shaders)
                    {
                        string srcShader = Path.Combine(shaderSourceDir, shader);
                        if (File.Exists(srcShader))
                        {
                            File.Copy(srcShader, Path.Combine(destShaderDir, shader), true);
                        }
                    }
                }

                // Copy Assets folder if exists
                string assetsSource = null;
                string[] possibleAssetsPaths = new[]
                {
                    Path.Combine(editorDir, "Assets"),
                    solDir != null ? Path.Combine(solDir, "Assets") : null
                };

                foreach (var aPath in possibleAssetsPaths)
                {
                    if (aPath != null && Directory.Exists(aPath))
                    {
                        assetsSource = aPath;
                        break;
                    }
                }

                if (assetsSource != null)
                {
                    CopyDirectory(assetsSource, Path.Combine(outDir, "Assets"));
                }

                string levelFile = Path.Combine(outDir, "level.json");
                _sceneManager.SaveScene(levelFile);

                SetStatus("Export complete!");
                MessageBox.Show(
                    $"Game successfully exported to:\n{outDir}\n\nTo play, run Engine.Runtime.exe.",
                    "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                try
                {
                    Process.Start("explorer.exe", $"\"{outDir}\"");
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Helpers
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        private bool ConfirmUnsaved()
        {
            if (!_unsavedChanges) return true;
            var r = MessageBox.Show("Unsaved changes. Save before continuing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)  { OnSave(); return true; }
            if (r == DialogResult.No)   return true;
            return false;
        }

        private void UpdateTitle()
        {
            string name  = _currentFilePath != null
                ? Path.GetFileNameWithoutExtension(_currentFilePath)
                : "Untitled";
            string dirty = _unsavedChanges ? " *" : "";
            Text = $"Adigame3D Editor вЂ” {name}{dirty}";
        }

        private void UpdateStats()
        {
            int count = _sceneManager.CurrentScene?.Entities?.Count ?? 0;
            _statsLabel.Text = $"{count} entities";
        }

        private void SetStatus(string msg)
        {
            _statusLabel.Text = msg;
        }

        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
        //  Р—Р°РєСЂС‹С‚РёРµ
        // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmUnsaved()) { e.Cancel = true; return; }
            _renderTimer?.Stop();
            _glControl?.MakeCurrent();
            _grid?.Dispose();
            _blockRenderer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ
    //  Proxy РґР»СЏ PropertyGrid
    // в•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђв•ђ

    /// <summary>
    /// РћР±С‘СЂС‚РєР° РІРѕРєСЂСѓРі Entity РґР»СЏ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ РІ PropertyGrid СЂРµРґР°РєС‚РѕСЂР°.
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
    public class EntityProxy
    {
        private Entity _entity;
        private Action _onChange;

        public EntityProxy(Entity e, Action onChange) { _entity = e; _onChange = onChange; }

        [System.ComponentModel.Category("Identity")]
        public string Name
        {
            get => _entity.Name;
            set { _entity.Name = value; _onChange(); }
        }

        [System.ComponentModel.Category("Identity")]
        public EntityType Type => _entity.Type;

        [System.ComponentModel.Category("Transform")]
        public string Position
        {
            get => $"{_entity.Position.X:F1}, {_entity.Position.Y:F1}, {_entity.Position.Z:F1}";
        }

        [System.ComponentModel.Category("Transform")]
        public float PosX
        {
            get => _entity.Position.X;
            set { _entity.Position.X = value; _onChange(); }
        }

        [System.ComponentModel.Category("Transform")]
        public float PosY
        {
            get => _entity.Position.Y;
            set { _entity.Position.Y = value; _onChange(); }
        }

        [System.ComponentModel.Category("Transform")]
        public float PosZ
        {
            get => _entity.Position.Z;
            set { _entity.Position.Z = value; _onChange(); }
        }

        [System.ComponentModel.Category("Appearance")]
        public float ColorR
        {
            get => _entity.Color.R;
            set { _entity.Color.R = Math.Max(0f, Math.Min(1f, value)); _onChange(); }
        }

        [System.ComponentModel.Category("Appearance")]
        public float ColorG
        {
            get => _entity.Color.G;
            set { _entity.Color.G = Math.Max(0f, Math.Min(1f, value)); _onChange(); }
        }

        [System.ComponentModel.Category("Appearance")]
        public float ColorB
        {
            get => _entity.Color.B;
            set { _entity.Color.B = Math.Max(0f, Math.Min(1f, value)); _onChange(); }
        }

        [System.ComponentModel.Category("Identity")]
        public string Id => _entity.Id.ToString("D");

        [System.ComponentModel.Category("Custom Properties")]
        public string TexturePath
        {
            get => _entity.Properties.TryGetValue("texture_path", out string v) ? v : "";
            set { _entity.Properties["texture_path"] = value; _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public string ModelPath
        {
            get => _entity.Properties.TryGetValue("model_path", out string v) ? v : "";
            set { _entity.Properties["model_path"] = value; _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public string SoundPath
        {
            get => _entity.Properties.TryGetValue("sound_path", out string v) ? v : "";
            set { _entity.Properties["sound_path"] = value; _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public float SoundRadius
        {
            get => _entity.Properties.TryGetValue("radius", out string v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float r) ? r : 15.0f;
            set { _entity.Properties["radius"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture); _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public float SoundVolume
        {
            get => _entity.Properties.TryGetValue("volume", out string v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vol) ? vol : 1.0f;
            set { _entity.Properties["volume"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture); _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public bool SoundLooping
        {
            get => !_entity.Properties.TryGetValue("looping", out string v) || !bool.TryParse(v, out bool loop) || loop;
            set { _entity.Properties["looping"] = value.ToString(); _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public int WaypointSequence
        {
            get => _entity.Properties.TryGetValue("sequence", out string v) && int.TryParse(v, out int s) ? s : 0;
            set { _entity.Properties["sequence"] = value.ToString(); _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public float WaypointDuration
        {
            get => _entity.Properties.TryGetValue("duration", out string v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float d) ? d : 3.0f;
            set { _entity.Properties["duration"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture); _onChange(); }
        }

        [System.ComponentModel.Category("Custom Properties")]
        public float WaypointFov
        {
            get => _entity.Properties.TryGetValue("fov", out string v) && float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f) ? f : 70.0f;
            set { _entity.Properties["fov"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture); _onChange(); }
        }
    }
}

