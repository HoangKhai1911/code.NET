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
    public partial class frmHomePage : Form
    {
        public frmHomePage()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Nút Home – hiện tại chưa làm gì thêm
        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        //  Nút "Recipes" (guna2Button2) → mở form danh sách công thức
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            f.Show();
            // Nếu muốn ẩn màn hình home khi mở Recipes:
            // this.Hide();
        }

        // Nút "Go!" (guna2Button6) → cũng mở danh sách công thức
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            f.Show();
            // this.Hide();
        }
    }
}
