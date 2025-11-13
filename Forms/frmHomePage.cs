//frmHomePage.cs
using System;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmHomePage : Form
    {
        public frmHomePage()
        {
            InitializeComponent();
        }

        // Helper dùng chung để mở form khác và ẩn form hiện tại
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // Nút "Home" trên chính frmHomePage -> không cần làm gì
        private void guna2Button1_Click(object? sender, EventArgs e)
        {
            // Đang ở Home rồi, không cần chuyển
        }

        // Label dòng 2 (nếu cần xử lý gì thêm sau này)
        private void guna2HtmlLabel3_Click(object? sender, EventArgs e)
        {
            // Tạm thời để trống
        }

        // Nút "Go!" – mở màn hình Recipes chính
        private void guna2Button6_Click_1(object? sender, EventArgs e)
        {
            var f = new frmRecipes();
            OpenForm(f);
        }

        // Nút "Recipes" trên thanh menu (trên cùng)
        // (Tùy chọn) Nếu bạn có gán sự kiện cho Recipes button ở Designer,
        // hãy bảo đảm Designer có dòng "+= guna2Button2_Click;"
        private void guna2Button2_Click(object? sender, EventArgs e)
        {
            // Nếu bạn muốn Recipes trên menu mở frmRecipes:
            var f = new frmRecipes();
            OpenForm(f);

            // Nếu bạn muốn mở frmMyRecipes thay vì frmRecipes thì đổi thành:
            // var f = new frmMyRecipes();
            // OpenForm(f);
        }

        // Nút "Favorites" trên thanh menu
        private void guna2Button5_Click(object? sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Nút "Collections" trên thanh menu
        private void guna2Button3_Click(object? sender, EventArgs e)
        {
            var f = new frmCollection();
            OpenForm(f);
        }

        // Nút "Profiles" trên thanh menu
        private void guna2Button4_Click(object? sender, EventArgs e)
        {
            var f = new frmProfile();
            OpenForm(f);
        }
    }
}
