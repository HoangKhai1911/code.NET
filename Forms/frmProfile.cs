using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WinCook.Models;
using WinCook.Services;
using WinCook.Forms; // Namespace chứa frmEditProfile

namespace WinCook
{
    public partial class frmProfile : Form
    {
        // Khai báo Service
        private readonly UtilityService _utilityService;
        private readonly UserService _userService;
        private int _currentUserId;

        public frmProfile()
        {
            InitializeComponent();

            // Khởi tạo Service
            _utilityService = new UtilityService();
            _userService = new UserService();

            // 1. Kiểm tra đăng nhập
            if (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // Nếu chưa đăng nhập thì đóng hoặc hiện thông báo
                // _currentUserId = 1; // Bật dòng này nếu muốn test giao diện mà không cần login
                MessageBox.Show("Vui lòng đăng nhập để xem trang cá nhân.");
                this.Close();
                return;
            }

            // 2. Gán sự kiện thủ công cho các Label (Vì trong Designer chưa gán)
            // Link "My Recipes"
            guna2HtmlLabel4.Cursor = Cursors.Hand;
            guna2HtmlLabel4.Click += (s, e) => OpenForm(new frmMyRecipes());

            // Link "My Favorite Recipes"
            guna2HtmlLabel5.Cursor = Cursors.Hand;
            guna2HtmlLabel5.Click += (s, e) => OpenForm(new frmMyFavRecipes());

            // Button "Browse" (button1) - Gán chung logic với click ảnh
            button1.Click += pictureBox1_Click;

            // Xử lý Logout khi đóng form bằng nút X
            this.FormClosing += FrmProfile_FormClosing;
        }

        // ============================================================
        // PHẦN 1: CÁC SỰ KIỆN ĐƯỢC ĐỊNH NGHĨA TRONG DESIGNER
        // (Tên hàm phải KHỚP CHÍNH XÁC với Designer.cs)
        // ============================================================

        // Sự kiện Load Form (Designer đặt tên là frmProfile_Load_1)
        private void frmProfile_Load_1(object sender, EventArgs e)
        {
            RefreshProfileUI();
            LoadStatistics();
        }

        // Nút EDIT (Designer đặt tên là guna2Button1_Click_1)
        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            var currentUser = AuthManager.CurrentUser;
            if (currentUser == null) return;

            // Truyền FullName vào tham số thứ 2
            using (var frmEdit = new frmEditProfile(_currentUserId, currentUser.FullName, currentUser.Email))
            {
                if (frmEdit.ShowDialog() == DialogResult.OK)
                {
                    RefreshProfileUI();
                }
            }
        }

        // Click vào Ảnh Avatar (Designer đặt tên là pictureBox1_Click)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh đại diện mới";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string sourceFile = ofd.FileName;

                        // Tạo thư mục UserImages
                        string uploadFolder = Path.Combine(Application.StartupPath, "UserImages");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                        // Tạo tên file unique
                        string newFileName = $"Avatar_{_currentUserId}_{DateTime.Now.Ticks}{Path.GetExtension(sourceFile)}";
                        string destPath = Path.Combine(uploadFolder, newFileName);

                        // Copy ảnh vào thư mục
                        File.Copy(sourceFile, destPath, true);

                        // Gọi Service cập nhật Database
                        if (_userService.UpdateUserAvatar(_currentUserId, destPath))
                        {
                            MessageBox.Show("Đổi ảnh đại diện thành công!", "Thông báo");
                            RefreshProfileUI(); // Load lại ảnh mới
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi lưu vào Database.", "Lỗi");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi upload ảnh: " + ex.Message);
                    }
                }
            }
        }

        // Nút Logout / Icon góc phải (Designer đặt tên là guna2Button2_Click)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            AuthManager.Logout();
            this.Close(); // Đóng form, sự kiện FormClosing sẽ lo việc mở lại Login
        }

        // --- THANH ĐIỀU HƯỚNG (MENU BAR) ---

        // Home
        private void guna2Button7_Click(object sender, EventArgs e) => OpenForm(new frmHomePage());

        // Recipes
        private void guna2Button3_Click(object sender, EventArgs e) => OpenForm(new frmRecipes());

        // Favorites
        private void guna2Button6_Click(object sender, EventArgs e) => OpenForm(new frmMyFavRecipes());

        // Collections
        private void guna2Button4_Click(object sender, EventArgs e) => OpenForm(new frmCollection());

        // Profiles (Trang hiện tại - Reload lại cho chắc)
        private void guna2Button5_Click(object sender, EventArgs e) => RefreshProfileUI();

        // Click vào Chart (Designer có tạo sự kiện này, để trống cũng được)
        private void chart1_Click(object sender, EventArgs e) { }


        // ============================================================
        // PHẦN 2: CÁC HÀM XỬ LÝ LOGIC (PRIVATE HELPER)
        // ============================================================

        private void RefreshProfileUI()
        {
            if (AuthManager.CurrentUser != null)
            {
                // 1. Hiển thị Tên
                if (guna2HtmlLabel2 != null)
                    guna2HtmlLabel2.Text = AuthManager.CurrentUser.FullName;

                // 2. Hiển thị Email (Đã che mờ bằng hàm MaskEmail bên dưới)
                if (guna2HtmlLabel1 != null)
                    guna2HtmlLabel1.Text = MaskEmail(AuthManager.CurrentUser.Email);

                // 3. Hiển thị Avatar
                string avatarPath = AuthManager.CurrentUser.AvatarUrl;

                if (pictureBox1 != null)
                {
                    // Reset thuộc tính ImageLocation trước để tránh xung đột
                    pictureBox1.ImageLocation = null;

                    // Kiểm tra: Nếu có đường dẫn trong DB VÀ file đó thực sự tồn tại trên máy
                    if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
                    {
                        pictureBox1.ImageLocation = avatarPath;
                    }
                    else
                    {
                        // --- Load ảnh mặc định từ Resources ---
                        // Vì file tên là "avatar-profile.jpg", C# sẽ đổi tên biến thành "avatar_profile"
                        pictureBox1.Image = Properties.Resources.avatar_profile;
                    }

                    // Chế độ co giãn ảnh đẹp (Zoom giữ tỉ lệ, Stretch lấp đầy)
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void LoadStatistics()
        {
            try
            {
                // Gọi Service lấy thống kê cá nhân
                var stats = _utilityService.GetUserStatistics(_currentUserId);

                // --- BIỂU ĐỒ 1: CỘT (Top Món Ăn) ---
                chart1.Series.Clear();
                chart1.Titles.Clear();
                chart1.Titles.Add("Top món ăn được yêu thích");

                Series s1 = chart1.Series.Add("Lượt thích");
                s1.ChartType = SeriesChartType.Column;
                s1.Color = Color.Goldenrod;

                if (stats.TopRecipes != null && stats.TopRecipes.Count > 0)
                {
                    foreach (var item in stats.TopRecipes)
                    {
                        // Lưu ý: Dùng StatChartData (Label, Value)
                        s1.Points.AddXY(item.Label, item.Value);
                    }
                }
                else
                {
                    s1.Points.AddXY("(Chưa có)", 0);
                }

                // --- BIỂU ĐỒ 2: TRÒN (Danh mục) ---
                chart2.Series.Clear();
                chart2.Titles.Clear();
                chart2.Titles.Add("Phân bố theo Danh mục");

                Series s2 = chart2.Series.Add("Danh mục");
                s2.ChartType = SeriesChartType.Pie;
                s2.IsValueShownAsLabel = true;

                if (stats.RecipesByCategory != null && stats.RecipesByCategory.Count > 0)
                {
                    foreach (var item in stats.RecipesByCategory)
                    {
                        s2.Points.AddXY(item.Label, item.Value);
                    }
                }
                else
                {
                    s2.Points.AddXY("(Trống)", 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart Error: " + ex.Message);
            }
        }

        // Hàm che email: abc@gmail.com -> a*****@gmail.com
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;
            var parts = email.Split('@');
            if (parts[0].Length <= 1) return email;
            return $"{parts[0][0]}*****@{parts[1]}";
        }

        // Hàm mở form mới và ẩn form cũ
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // Xử lý đóng form -> về Login
        private void FrmProfile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                AuthManager.Logout();
                var loginForm = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();
                if (loginForm != null) loginForm.Show();
                else new frmLogin().Show();
            }
        }
    }
}