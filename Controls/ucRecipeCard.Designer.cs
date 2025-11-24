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
            this.picImage = new Guna.UI2.WinForms.Guna2PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.btnFavoriteSmall = new System.Windows.Forms.Label(); // Nút tim nhỏ
            this.lblRating = new System.Windows.Forms.Label(); // Điểm số (ví dụ: 4.5 ★)
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.SuspendLayout();
            // 
            // picImage
            // 
            this.picImage.BorderRadius = 10;
            this.picImage.CustomizableEdges = customizableEdges1;
            this.picImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.picImage.ImageLocation = ""; // Code sẽ set
            this.picImage.ImageRotate = 0F;
            this.picImage.Location = new System.Drawing.Point(10, 10);
            this.picImage.Name = "picImage";
            this.picImage.ShadowDecoration.CustomizableEdges = customizableEdges2;
            this.picImage.Size = new System.Drawing.Size(200, 150);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picImage.TabIndex = 0;
            this.picImage.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(10, 165);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 48);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Tên món ăn";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblAuthor.Location = new System.Drawing.Point(10, 215);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(63, 20);
            this.lblAuthor.TabIndex = 2;
            this.lblAuthor.Text = "Author";
            // 
            // btnFavoriteSmall
            // 
            this.btnFavoriteSmall.AutoSize = true;
            this.btnFavoriteSmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFavoriteSmall.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnFavoriteSmall.ForeColor = System.Drawing.Color.LightGray; // Mặc định chưa tim
            this.btnFavoriteSmall.Location = new System.Drawing.Point(185, 165); // Góc phải của tên món
            this.btnFavoriteSmall.Name = "btnFavoriteSmall";
            this.btnFavoriteSmall.Size = new System.Drawing.Size(38, 32);
            this.btnFavoriteSmall.TabIndex = 3;
            this.btnFavoriteSmall.Text = "♡"; // Tim rỗng
            // 
            // lblRating
            // 
            this.lblRating.AutoSize = true;
            this.lblRating.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRating.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblRating.Location = new System.Drawing.Point(150, 215); // Góc phải dưới
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(44, 20);
            this.lblRating.TabIndex = 4;
            this.lblRating.Text = "0.0 ★";
            // 
            // ucRecipeCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblRating);
            this.Controls.Add(this.btnFavoriteSmall);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.picImage);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(10);
            this.Name = "ucRecipeCard";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(220, 250);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox picImage;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAuthor;
        public System.Windows.Forms.Label btnFavoriteSmall; // Public để form cha truy cập nếu cần
        private System.Windows.Forms.Label lblRating;
    }
}