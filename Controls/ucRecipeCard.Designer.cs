namespace WinCook.Controls // <-- ĐÃ CẬP NHẬT NAMESPACE
{
    partial class ucRecipeCard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            picImage = new Guna.UI2.WinForms.Guna2PictureBox();
            lblTitle = new Label();
            lblAuthor = new Label();
            ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
            SuspendLayout();
            // 
            // picImage
            // 
            picImage.BorderRadius = 10;
            picImage.CustomizableEdges = customizableEdges1;
            picImage.Dock = DockStyle.Top;
            picImage.ImageLocation = "https://placehold.co/200x150/E8AA8B/ffffff?text=Loading...";
            picImage.ImageRotate = 0F;
            picImage.Location = new System.Drawing.Point(10, 10);
            picImage.Name = "picImage";
            picImage.ShadowDecoration.CustomizableEdges = customizableEdges2;
            picImage.Size = new System.Drawing.Size(200, 150);
            picImage.SizeMode = PictureBoxSizeMode.StretchImage;
            picImage.TabIndex = 0;
            picImage.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new System.Drawing.Point(10, 163);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(200, 52);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Cánh gà chiên nước mắm";
            // 
            // lblAuthor
            // 
            lblAuthor.AutoSize = true;
            lblAuthor.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblAuthor.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            lblAuthor.Location = new System.Drawing.Point(10, 215);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new System.Drawing.Size(89, 20);
            lblAuthor.TabIndex = 2;
            lblAuthor.Text = "by tester1";
            // 
            // ucRecipeCard
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Margin = new Padding(10);
            Name = "ucRecipeCard";
            Padding = new Padding(10);
            Size = new System.Drawing.Size(220, 245);
            ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox picImage;
        private Label lblTitle;
        private Label lblAuthor;
    }
}