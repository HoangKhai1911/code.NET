//frmCollection.cs
using System;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmCollection : Form
    {
        public frmCollection()
        {
            InitializeComponent();
        }

        private void frmCollection_Load(object sender, EventArgs e)
        {
            // Tạm thời chưa có logic load dữ liệu -> giữ nguyên
        }

        // Helper dùng chung
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // ===== Thanh menu top trong frmCollection =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
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
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections (đang ở Collections -> không cần chuyển)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Đã ở Collections, không làm gì
        }

        // Profiles
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmProfile();
            OpenForm(f);
        }

        // Các nút Search / Add / Delete hiện tại chưa cần logic -> bạn có thể bổ sung sau
        // Search
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // TODO: thêm logic tìm kiếm sau
        }

        // Add
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // TODO: thêm logic Add Collection sau
        }

        // Delete
        private void guna2Button8_Click(object sender, EventArgs e)
        {
            // TODO: thêm logic Delete Collection sau
        }
    }
}
