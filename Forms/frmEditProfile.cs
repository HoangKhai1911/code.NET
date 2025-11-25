using System;
using System.Drawing; // Cần thiết cho Point, Size, Font
using System.Windows.Forms;
using WinCook.Services;
using Guna.UI2.WinForms;

namespace WinCook.Forms
{
    public partial class frmEditProfile : Form
    {
        private int _userId;
        private UserService _userService;

        // Các control giao diện
        private Guna2TextBox txtFullName; // Đổi từ txtUsername thành txtFullName
        private Guna2TextBox txtEmail;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;
        private Label lblTitle;
        private Label lblSubHint;

        /// <summary>
        /// Khởi tạo form sửa thông tin
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="currentFullName">Tên hiển thị hiện tại (FullName)</param>
        /// <param name="currentEmail">Email hiện tại</param>
        public frmEditProfile(int userId, string currentFullName, string currentEmail)
        {
            InitializeComponent(); // Hàm của Designer
            InitializeCustomControls(); // Hàm tự vẽ giao diện

            _userId = userId;
            _userService = new UserService();

            // Gán dữ liệu cũ vào ô nhập
            txtFullName.Text = currentFullName;
            txtEmail.Text = currentEmail;

            // Gán sự kiện click
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu
            string newName = txtFullName.Text.Trim();
            string newEmail = txtEmail.Text.Trim();

            // 2. Kiểm tra dữ liệu (Validate)
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Tên hiển thị không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            // 3. Gọi Service cập nhật
            // Lưu ý: Tham số thứ 4 là số điện thoại, tạm thời để trống ""
            bool isUpdated = _userService.UpdateUserProfile(_userId, newName, newEmail);

            if (isUpdated)
            {
                MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Báo cho form cha biết là OK
                this.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm tạo giao diện nhanh bằng Code (Không cần kéo thả Designer)
        private void InitializeCustomControls()
        {
            // Cài đặt Form
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Chỉnh sửa hồ sơ";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // 1. Tiêu đề
            lblTitle = new Label
            {
                Text = "Cập Nhật Hồ Sơ",
                Location = new Point(0, 20), // Căn giữa sau
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkSalmon
            };

            // 2. Ô nhập Tên hiển thị
            txtFullName = new Guna2TextBox
            {
                Location = new Point(50, 80),
                Size = new Size(300, 40),
                PlaceholderText = "Tên hiển thị (Full Name)",
                BorderRadius = 10
            };

            // Gợi ý nhỏ dưới ô tên
            lblSubHint = new Label
            {
                Text = "* Tên này sẽ hiển thị trên trang cá nhân của bạn.",
                Location = new Point(55, 122),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // 3. Ô nhập Email
            txtEmail = new Guna2TextBox
            {
                Location = new Point(50, 150),
                Size = new Size(300, 40),
                PlaceholderText = "Địa chỉ Email",
                BorderRadius = 10
            };

            // 4. Nút Lưu
            btnSave = new Guna2Button
            {
                Text = "Lưu Thay Đổi",
                Location = new Point(50, 230),
                Size = new Size(140, 45),
                FillColor = Color.DarkSalmon,
                ForeColor = Color.White,
                BorderRadius = 20,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // 5. Nút Hủy
            btnCancel = new Guna2Button
            {
                Text = "Hủy Bỏ",
                Location = new Point(210, 230),
                Size = new Size(140, 45),
                FillColor = Color.LightGray,
                ForeColor = Color.DimGray,
                BorderRadius = 20,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Thêm controls vào Form
            this.Controls.Add(lblTitle);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblSubHint);
            this.Controls.Add(txtEmail);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        // Sự kiện load mặc định (để trống cũng được)
        private void frmEditProfile_Load(object sender, EventArgs e) { }
    }
}