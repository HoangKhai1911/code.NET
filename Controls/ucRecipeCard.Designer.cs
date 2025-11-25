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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucRecipeCard));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            picImage = new Guna.UI2.WinForms.Guna2PictureBox();
            lblTitle = new Label();
            lblAuthor = new Label();
            btnFavoriteSmall = new Label();
            lblRating = new Label();
            ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
            SuspendLayout();
            // 
            // picImage
            // 
            picImage.BorderRadius = 10;
            picImage.CustomizableEdges = customizableEdges1;
            picImage.Dock = DockStyle.Top;
            picImage.Image = (Image)resources.GetObject("picImage.Image");
            picImage.ImageLocation = "";
            picImage.ImageRotate = 0F;
            picImage.Location = new Point(10, 10);
            picImage.Name = "picImage";
            picImage.ShadowDecoration.CustomizableEdges = customizableEdges2;
            picImage.Size = new Size(200, 150);
            picImage.SizeMode = PictureBoxSizeMode.StretchImage;
            picImage.TabIndex = 0;
            picImage.TabStop = false;
            picImage.Click += picImage_Click;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(10, 165);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 48);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Tên món ăn";
            // 
            // lblAuthor
            // 
            lblAuthor.AutoSize = true;
            lblAuthor.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblAuthor.ForeColor = SystemColors.ControlDarkDark;
            lblAuthor.Location = new Point(10, 215);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(52, 20);
            lblAuthor.TabIndex = 2;
            lblAuthor.Text = "Author";
            // 
            // btnFavoriteSmall
            // 
            btnFavoriteSmall.AutoSize = true;
            btnFavoriteSmall.Cursor = Cursors.Hand;
            btnFavoriteSmall.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnFavoriteSmall.ForeColor = Color.LightGray;
            btnFavoriteSmall.Location = new Point(185, 165);
            btnFavoriteSmall.Name = "btnFavoriteSmall";
            btnFavoriteSmall.Size = new Size(30, 32);
            btnFavoriteSmall.TabIndex = 3;
            btnFavoriteSmall.Text = "♡";
            // 
            // lblRating
            // 
            lblRating.AutoSize = true;
            lblRating.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRating.ForeColor = Color.Goldenrod;
            lblRating.Location = new Point(150, 215);
            lblRating.Name = "lblRating";
            lblRating.Size = new Size(49, 20);
            lblRating.TabIndex = 4;
            lblRating.Text = "0.0 ★";
            // 
            // ucRecipeCard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(lblRating);
            Controls.Add(btnFavoriteSmall);
            Controls.Add(lblAuthor);
            Controls.Add(lblTitle);
            Controls.Add(picImage);
            Cursor = Cursors.Hand;
            Margin = new Padding(10);
            Name = "ucRecipeCard";
            Padding = new Padding(10);
            Size = new Size(220, 250);
            ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox picImage;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAuthor;
        public System.Windows.Forms.Label btnFavoriteSmall; // Public để form cha truy cập nếu cần
        private System.Windows.Forms.Label lblRating;
    }
}