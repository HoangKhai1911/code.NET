using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WinCook
{
    public partial class frmHomePage : Form
    {
        public frmHomePage()
        {
            InitializeComponent();
        }

        // Designer gắn: guna2Button1.Click += guna2Button1_Click;
        private void guna2Button1_Click(object? sender, EventArgs e)
        {
            // Nút "Home" – hiện tại chưa làm gì thêm
        }

        // Designer gắn: guna2HtmlLabel3.Click += guna2HtmlLabel3_Click;
        private void guna2HtmlLabel3_Click(object? sender, EventArgs e)
        {
            // Xử lý khi click label nếu cần
        }

        // Designer gắn: guna2Button6.Click += this.guna2Button6_Click_1;
        private void guna2Button6_Click_1(object? sender, EventArgs e)
        {
            // Ví dụ mở form Recipes
            var f = new frmRecipes();
            f.Show();
            // this.Hide();
        }

        // (Tùy chọn) Nếu bạn có gán sự kiện cho Recipes button ở Designer,
        // hãy bảo đảm Designer có dòng "+= guna2Button2_Click;"
        private void guna2Button2_Click(object? sender, EventArgs e)
        {
            var f = new frmRecipes();
            f.Show();
        }
    }
}
