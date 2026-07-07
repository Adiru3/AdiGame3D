namespace Engine.Editor.Forms
{
    partial class MainEditorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize    = new System.Drawing.Size(1280, 768);
            this.Text          = "Adigame3D Editor";
            this.Icon          = null;
            // Минимальная инициализация — вся реальная UI настройка в SetupUI()
        }
    }
}
