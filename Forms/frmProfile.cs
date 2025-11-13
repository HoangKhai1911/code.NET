//frmProfile.cs
using System;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmProfile : Form
    {
        public frmProfile()
        {
            InitializeComponent();
        }

        // Helper dùng chung
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // Nút icon nhỏ góc phải (guna2Button2) – nếu chưa cần, để trống
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Ví dụ: nếu muốn nút này là "Close" thì có thể:
            // this.Close();
        }

        // ===== Các nút trên thanh menu top trong frmProfile =====

        // Home
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            var f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmCollection();
            OpenForm(f);
        }

        // Profiles (đang ở Profile rồi -> không cần chuyển, nhưng vẫn có handler cho đủ)
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // Đã ở Profiles, không làm gì
        }
    }
}
