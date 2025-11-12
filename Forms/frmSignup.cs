using System;
using System.Windows.Forms;
using WinCook.Services; // THÊM DÒNG NÀY

namespace WinCook
{
    public partial class frmSignup : Form
    {
        // Xóa 'userFile', thay bằng 'UserService'
        private readonly UserService _userService;

        public frmSignup()
        {
            InitializeComponent();

            // Khởi tạo UserService
            _userService = new UserService();

            // Các control này đã được định nghĩa trong file Designer của bạn
            guna2Button1.Click += BtnSignUp_Click;       // "Sign up" button
            linkLabel1.LinkClicked += linkLabel1_LinkClicked; // "Go to log in" link
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // (Bạn có thể giữ hoặc xóa sự kiện trống này)
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Sự kiện này trống, có thể sự kiện chính là 'BtnSignUp_Click'
            // Nếu bạn không dùng, hãy xóa nó khỏi Designer.
        }

        private void BtnSignUp_Click(object sender, EventArgs e)
        {
            // 1. Lấy thông tin từ các control (Khớp với file Designer)
            string username = guna2TextBox1.Text.Trim(); // username field
            string email = guna2TextBox2.Text.Trim();    // Email
            string password = guna2TextBox3.Text.Trim(); // password field

            // 2. Kiểm tra thông tin đầu vào
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập, Email và Mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ĐÃ XÓA BƯỚC KIỂM TRA CONFIRM PASSWORD VÌ GIAO DIỆN KHÔNG CÓ

            // 3. Gọi hàm Register từ UserService (thay vì ghi file txt)
            try
            {
                bool isSuccess = _userService.Register(username, password, email);

                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký tài khoản thành công! \nVui lòng quay lại trang đăng nhập.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Đăng ký xong, mở lại form Login
                    GoToLogin();
                }
                else
                {
                    // Lỗi này thường là do Tên đăng nhập đã tồn tại
                    MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Bắt các lỗi khác (như không kết nối được DB)
                MessageBox.Show("Đã xảy ra lỗi trong quá trình đăng ký. \nChi tiết: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Xóa hàm UserExists, vì UserService đã tự xử lý
        // private bool UserExists(string username) { ... }


        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            // (Giữ lại nếu có)
        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {
            // (Giữ lại nếu có)
        }

        // Bạn có 2 hàm LinkLabel1_LinkClicked, tôi sẽ gộp lại thành 1
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoToLogin();
        }

        // Tạo hàm dùng chung để quay về Login
        private void GoToLogin()
        {
            // Open login form
            frmLogin login = new frmLogin();
            login.Show();
            this.Close();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}