using System;
using System.Drawing;
using System.Windows.Forms;

namespace Engine.Runtime.Forms
{
    public class PauseMenuForm : Form
    {
        public float MasterVolume { get; private set; }
        public float MouseSensitivity { get; private set; }
        public int SelectedWidth { get; private set; }
        public int SelectedHeight { get; private set; }
        public bool ExitRequest { get; private set; }

        private TrackBar _volumeTrack;
        private TrackBar _sensitivityTrack;
        private ComboBox _resolutionCombo;
        private Label _lblVolumeVal;
        private Label _lblSensVal;

        public PauseMenuForm(float currentVolume, float currentSens, int currentWidth, int currentHeight)
        {
            MasterVolume = currentVolume;
            MouseSensitivity = currentSens;
            SelectedWidth = currentWidth;
            SelectedHeight = currentHeight;
            ExitRequest = false;

            InitializeStyles();
            CreateControls();
        }

        private void InitializeStyles()
        {
            Text = "Pause / Settings";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(380, 480);
            BackColor = Color.FromArgb(32, 32, 38);
            ShowInTaskbar = false;

            // Добавляем красивую тонкую рамку вокруг формы
            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(60, 60, 75), 2))
                {
                    e.Graphics.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
                }
            };
        }

        private void CreateControls()
        {
            // Title
            var lblTitle = new Label
            {
                Text = "GAME PAUSED",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 160, 240),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            Controls.Add(lblTitle);

            // Container Panel
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(30, 20, 30, 20),
                WrapContents = false
            };
            Controls.Add(panel);

            // Volume Label
            var lblVolume = new Label
            {
                Text = "Master Volume",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 190),
                Width = 320,
                Height = 20
            };
            panel.Controls.Add(lblVolume);

            // Volume TrackBar
            _volumeTrack = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = (int)(MasterVolume * 100f),
                Width = 310,
                Height = 35,
                TickStyle = TickStyle.None
            };
            _lblVolumeVal = new Label
            {
                Text = $"{_volumeTrack.Value}%",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(140, 140, 150),
                Width = 320,
                Height = 15
            };
            _volumeTrack.Scroll += (s, e) =>
            {
                MasterVolume = _volumeTrack.Value / 100f;
                _lblVolumeVal.Text = $"{_volumeTrack.Value}%";
            };
            panel.Controls.Add(_volumeTrack);
            panel.Controls.Add(_lblVolumeVal);

            // Spacer
            panel.Controls.Add(new Panel { Height = 10, Width = 320 });

            // Sensitivity Label
            var lblSens = new Label
            {
                Text = "Mouse Sensitivity",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 190),
                Width = 320,
                Height = 20
            };
            panel.Controls.Add(lblSens);

            // Sensitivity TrackBar
            _sensitivityTrack = new TrackBar
            {
                Minimum = 5,
                Maximum = 50,
                Value = (int)(MouseSensitivity * 100f),
                Width = 310,
                Height = 35,
                TickStyle = TickStyle.None
            };
            _lblSensVal = new Label
            {
                Text = $"{MouseSensitivity:F2}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(140, 140, 150),
                Width = 320,
                Height = 15
            };
            _sensitivityTrack.Scroll += (s, e) =>
            {
                MouseSensitivity = _sensitivityTrack.Value / 100f;
                _lblSensVal.Text = $"{MouseSensitivity:F2}";
            };
            panel.Controls.Add(_sensitivityTrack);
            panel.Controls.Add(_lblSensVal);

            // Spacer
            panel.Controls.Add(new Panel { Height = 10, Width = 320 });

            // Resolution Label
            var lblRes = new Label
            {
                Text = "Screen Resolution",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 190),
                Width = 320,
                Height = 20
            };
            panel.Controls.Add(lblRes);

            // Resolution ComboBox
            _resolutionCombo = new ComboBox
            {
                Width = 310,
                Height = 26,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            _resolutionCombo.Items.AddRange(new object[] {
                "1024 x 576",
                "1280 x 720",
                "1366 x 768",
                "1600 x 900",
                "1920 x 1080"
            });

            // Set current
            string currentStr = $"{SelectedWidth} x {SelectedHeight}";
            int idx = _resolutionCombo.FindString(currentStr);
            if (idx >= 0)
                _resolutionCombo.SelectedIndex = idx;
            else
            {
                _resolutionCombo.Items.Add(currentStr);
                _resolutionCombo.SelectedIndex = _resolutionCombo.Items.Count - 1;
            }

            _resolutionCombo.SelectedIndexChanged += (s, e) =>
            {
                string[] parts = _resolutionCombo.SelectedItem.ToString().Split('x');
                if (parts.Length == 2)
                {
                    SelectedWidth = int.Parse(parts[0].Trim());
                    SelectedHeight = int.Parse(parts[1].Trim());
                }
            };
            panel.Controls.Add(_resolutionCombo);

            // Spacers for button section
            panel.Controls.Add(new Panel { Height = 30, Width = 320 });

            // Resume Button
            var btnResume = new Button
            {
                Text = "RESUME GAME",
                Width = 310,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 150, 250),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResume.FlatAppearance.BorderSize = 0;
            btnResume.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            panel.Controls.Add(btnResume);

            panel.Controls.Add(new Panel { Height = 8, Width = 320 });

            // Exit Button
            var btnExit = new Button
            {
                Text = "QUIT TO EDITOR",
                Width = 310,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.FromArgb(220, 100, 100),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) =>
            {
                ExitRequest = true;
                DialogResult = DialogResult.Cancel;
                Close();
            };
            panel.Controls.Add(btnExit);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Закрываем меню и возобновляем игру при повторном нажатии Escape
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
