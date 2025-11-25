namespace WinCook.Forms
{
    partial class frmSelectCollection
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.cboCollections = new Guna.UI2.WinForms.Guna2ComboBox();
            this.btnOK = new Guna.UI2.WinForms.Guna2Button();
            this.btnCancel = new Guna.UI2.WinForms.Guna2Button();
            this.btnCreateNew = new Guna.UI2.WinForms.Guna2Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Chọn Bộ Sưu Tập để lưu:";
            // 
            // cboCollections
            // 
            this.cboCollections.BackColor = System.Drawing.Color.Transparent;
            this.cboCollections.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboCollections.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCollections.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboCollections.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.cboCollections.ItemHeight = 30;
            this.cboCollections.Location = new System.Drawing.Point(25, 60);
            this.cboCollections.Name = "cboCollections";
            this.cboCollections.Size = new System.Drawing.Size(330, 36);
            this.cboCollections.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.BorderRadius = 5;
            this.btnOK.FillColor = System.Drawing.Color.DarkSalmon;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(190, 160);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 35);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "Lưu";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BorderRadius = 5;
            this.btnCancel.FillColor = System.Drawing.Color.LightGray;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.Location = new System.Drawing.Point(275, 160);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.BorderRadius = 5;
            this.btnCreateNew.FillColor = System.Drawing.Color.Transparent;
            this.btnCreateNew.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
            this.btnCreateNew.ForeColor = System.Drawing.Color.Blue;
            this.btnCreateNew.Location = new System.Drawing.Point(25, 110);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(180, 30);
            this.btnCreateNew.TabIndex = 4;
            this.btnCreateNew.Text = "+ Tạo bộ sưu tập mới";
            this.btnCreateNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // frmSelectCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(380, 220);
            this.Controls.Add(this.btnCreateNew);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cboCollections);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectCollection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chọn Bộ Sưu Tập";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private Guna.UI2.WinForms.Guna2ComboBox cboCollections;
        private Guna.UI2.WinForms.Guna2Button btnOK;
        private Guna.UI2.WinForms.Guna2Button btnCancel;
        private Guna.UI2.WinForms.Guna2Button btnCreateNew;
    }
}