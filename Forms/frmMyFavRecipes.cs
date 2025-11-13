//frmMyFavRecipes.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmMyFavRecipes : Form
    {
        public frmMyFavRecipes()
        {
            InitializeComponent();
        }
        // ===== Helper dùng chung để mở form khác và ẩn form hiện tại =====
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

      

        // ===== THANH MENU TRÊN CÙNG CỦA TRANG FAVORITES =====
        // Lưu ý: các handler này giả sử bạn dùng lại 5 nút giống các form khác:
        // guna2Button1 = Home
        // guna2Button2 = Recipes
        // guna2Button5 = Favorites
        // guna2Button3 = Collections
        // guna2Button4 = Profiles

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites (đang ở Favorites rồi -> không cần chuyển)
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // Đã ở Favorites, không làm gì
        }

        // Collections
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = new frmCollection();
            OpenForm(f);
        }

        // Profiles
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmProfile();
            OpenForm(f);
        }
        private void panel12_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
