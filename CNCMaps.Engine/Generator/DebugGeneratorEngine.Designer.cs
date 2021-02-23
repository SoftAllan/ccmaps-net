
namespace CNCMaps.Engine.Generator {
	partial class DebugGeneratorEngine {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.Canvas = new CNCMaps.Engine.Rendering.ZoomableCanvas();
            this.SuspendLayout();
            // 
            // Canvas
            // 
            this.Canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Canvas.Image = null;
            this.Canvas.Location = new System.Drawing.Point(36, 27);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(716, 380);
            this.Canvas.TabIndex = 0;
            this.Canvas.Text = "zoomableCanvas1";
            this.Canvas.VirtualMode = false;
            this.Canvas.VirtualSize = new System.Drawing.Size(0, 0);
            // 
            // DebugGeneratorEngine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Canvas);
            this.Name = "DebugGeneratorEngine";
            this.Text = "DebugGeneratorEngine";
            this.Load += new System.EventHandler(this.DebugGeneratorEngine_Load);
            this.ResumeLayout(false);

		}

		#endregion

		public Rendering.ZoomableCanvas Canvas;
	}
}
