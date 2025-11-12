using System;
using System.IO; // (using này không cần nữa, nhưng có thể giữ lại)
using System.Windows.Forms;
using WinCook.Models;   // <-- THÊM DÒNG NÀY
using WinCook.Services; // <-- THÊM DÒNG NÀY

namespace WinCook
{
    public partial class frmLogin : Form
    {
        // Xóa dòng 'userFile' và thay bằng 'UserService'
        private readonly UserService _userService;

        public frmLogin()
        {
            InitializeComponent();

            // Khởi tạo UserService
            _userService = new UserService();

            // Attach event handlers (Rất tốt khi giữ lại phần này)
            guna2Button1.Click += BtnLogin_Click;   // "Log in" button
            linkLabel1.LinkClicked += LinkLabel1_LinkClicked; // "Register" link
        }
        // Designer gắn: Load += frmLogin_Load;
        private void frmLogin_Load(object? sender, EventArgs e)
        {
            // TODO: khởi tạo form nếu cần
        }

        // Designer gắn: guna2TextBox1.TextChanged += guna2TextBox1_TextChanged;
        private void guna2TextBox1_TextChanged(object? sender, EventArgs e)
        {
            // TODO: xử lý khi text thay đổi (nếu cần)
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Lấy tên control từ code của bạn
            string username = guna2TextBox1.Text.Trim(); // username input
            string password = guna2TextBox2.Text.Trim(); // password input

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- THAY THẾ LOGIC VALIDATEUSER BẰNG USER SERVICE ---

            // 2. Gọi hàm Login từ UserService
            User loggedInUser = _userService.Login(username, password);

            // 3. Xử lý kết quả
            if (loggedInUser != null)
            {
                // Đăng nhập thành công!
                MessageBox.Show("Đăng nhập thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 3.1. Lưu thông tin người dùng vào AuthManager
                AuthManager.CurrentUser = loggedInUser;

                // 3.2. Mở Form chính (sử dụng tên form của bạn)
                frmHomePage home = new frmHomePage();
                home.Show();

                // Ẩn form đăng nhập
                this.Hide();
            }
            else
            {
                // Đăng nhập thất bại
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- XÓA HÀM VALIDATEUSER CŨ ---
        // private bool ValidateUser(string username, string password) { ... }
        // (Hàm này không còn cần thiết vì đã có UserService.Login)

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Mở form signup (sử dụng tên form của bạn)
            frmSignup signup = new frmSignup();
            signup.Show();
            this.Hide();
        }

        // --- CÁC HÀM KHÁC MÀ BẠN CÓ (NẾU CÓ) ---
        // private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e) { }
        // private void frmLogin_Load(object sender, EventArgs e) { }
        // private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }

    }
}