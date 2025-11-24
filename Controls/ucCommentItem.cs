using System;
using System.Drawing;
using System.Windows.Forms;
using WinCook.Models;

namespace WinCook.Controls
{
    public partial class ucCommentItem : UserControl
    {
        public ucCommentItem()
        {
            InitializeComponent();
        }

        public ucCommentItem(Rating rating) : this()
        {
            // 1. Gán thông tin cơ bản
            lblUsername.Text = rating.Username;
            lblDate.Text = rating.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            lblContent.Text = rating.Comment;

            // 2. Xử lý chiều cao tự động (Auto-size height)
            // Đo chiều cao cần thiết cho nội dung
            Size textSize = TextRenderer.MeasureText(rating.Comment, lblContent.Font, new Size(lblContent.Width, 0), TextFormatFlags.WordBreak);

            // Cập nhật chiều cao label
            lblContent.Height = Math.Max(30, textSize.Height + 10);

            // Cập nhật chiều cao tổng thể của UserControl
            // (Khoảng cách trên (60) + chiều cao text + khoảng cách dưới (10))
            this.Height = 60 + lblContent.Height + 10;

            // 3. (Tùy chọn) Hiển thị số sao nếu cần
            // Bạn có thể thêm 1 label nhỏ để hiện: "★ 5" bên cạnh tên
        }
    }
}