namespace WinCook.Controls
{
    partial class ucCollection
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            // Khai báo các control
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.btnDelete = new Guna.UI2.WinForms.Guna2Button();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2ShadowPanel1 = new Guna.UI2.WinForms.Guna2ShadowPanel();

            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.guna2ShadowPanel1.SuspendLayout();
            this.SuspendLayout();

            // 
            // guna2ShadowPanel1 (Khung viền có bóng đổ cho đẹp)
            // 
            this.guna2ShadowPanel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2ShadowPanel1.FillColor = System.Drawing.Color.White;
            this.guna2ShadowPanel1.Controls.Add(this.btnDelete);
            this.guna2ShadowPanel1.Controls.Add(this.lblCount);
            this.guna2ShadowPanel1.Controls.Add(this.lblName);
            this.guna2ShadowPanel1.Controls.Add(this.picIcon);
            this.guna2ShadowPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2ShadowPanel1.Radius = 10;
            this.guna2ShadowPanel1.ShadowColor = System.Drawing.Color.Black;
            this.guna2ShadowPanel1.ShadowDepth = 50;
            this.guna2ShadowPanel1.ShadowShift = 2;
            this.guna2ShadowPanel1.Location = new System.Drawing.Point(5, 5);
            this.guna2ShadowPanel1.Name = "guna2ShadowPanel1";
            this.guna2ShadowPanel1.Size = new System.Drawing.Size(190, 210);
            this.guna2ShadowPanel1.TabIndex = 0;

            // 
            // picIcon (Icon Folder - Bạn có thể thay bằng Image từ Resource)
            // 
            this.picIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picIcon.BackColor = System.Drawing.Color.Transparent;
            // Nếu bạn có ảnh folder trong Resource thì bỏ comment dòng dưới và sửa tên
            // this.picIcon.Image = global::WinCook.Properties.Resources.folder_icon; 
            this.picIcon.Location = new System.Drawing.Point(55, 30);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(80, 80);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            // Tạm thời tô màu cam làm icon nếu chưa có ảnh
            this.picIcon.BackColor = System.Drawing.Color.PeachPuff;
            this.picIcon.TabIndex = 0;
            this.picIcon.TabStop = false;

            // 
            // lblName (Tên Bộ sưu tập)
            // 
            this.lblName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.lblName.Location = new System.Drawing.Point(10, 120);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(170, 25);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "Tên Bộ Sưu Tập";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblCount (Số lượng công thức)
            // 
            this.lblCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCount.ForeColor = System.Drawing.Color.Gray;
            this.lblCount.Location = new System.Drawing.Point(10, 145);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(170, 20);
            this.lblCount.TabIndex = 2;
            this.lblCount.Text = "0 công thức";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // btnDelete (Nút Xóa nhỏ)
            // 
            this.btnDelete.BorderRadius = 10;
            this.btnDelete.FillColor = System.Drawing.Color.Salmon;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(60, 170);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 25);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Xóa";
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;

            // 
            // guna2Elipse1 (Bo góc cho UserControl)
            // 
            this.guna2Elipse1.BorderRadius = 15;
            this.guna2Elipse1.TargetControl = this;

            // 
            // ucCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.guna2ShadowPanel1);
            this.Name = "ucCollection";
            this.Size = new System.Drawing.Size(200, 220);
            this.Padding = new System.Windows.Forms.Padding(5); // Padding để hiện bóng đổ
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.guna2ShadowPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblCount;
        private Guna.UI2.WinForms.Guna2Button btnDelete;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private Guna.UI2.WinForms.Guna2ShadowPanel guna2ShadowPanel1;
    }
}