using System;
using System.Windows.Forms;
using WinCook.Services; // <-- Thêm Service
using WinCook.Models;   // <-- Thêm Model
using System.Windows.Forms.DataVisualization.Charting; // <-- Thêm thư viện Chart
using System.Linq; // <-- THÊM THƯ VIỆN NÀY

namespace WinCook
{
    public partial class frmProfile : Form
    {
        // Khai báo Service
        private readonly UtilityService _utilityService;
        private int _currentUserId;

        public frmProfile()
        {
            InitializeComponent();

            _utilityService = new UtilityService();

            // Lấy ID người dùng (nếu đã đăng nhập)
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // Nếu chưa đăng nhập (ví dụ: đang test), đóng form
                MessageBox.Show("Vui lòng đăng nhập để xem Trang cá nhân.");
                this.Close();
                return;
            }

            // Gán sự kiện Load
            this.Load += FrmProfile_Load;

            // Gán sự kiện cho các nút/link còn thiếu (theo file Designer)
            guna2Button1.Click += Guna2Button1_Click; // Nút "Edit"
            guna2HtmlLabel4.Click += Guna2HtmlLabel4_Click; // Link "My Recipes"
            guna2HtmlLabel5.Click += Guna2HtmlLabel5_Click; // Link "My Favorite Recipes"

            // === THÊM SỰ KIỆN LOGOUT KHI ĐÓNG FORM ===
            // Gán sự kiện FormClosing (khi bấm nút 'X')
            this.FormClosing += FrmProfile_FormClosing;
        }

        /// <summary>
        /// Hàm chạy khi Form được tải
        /// </summary>
        private void FrmProfile_Load(object sender, EventArgs e)
        {
            LoadUserProfile();
            LoadStatistics();
        }

        /// <summary>
        /// (Nhóm B) Tải thông tin người dùng lên các Label
        /// </summary>
        private void LoadUserProfile()
        {
            if (AuthManager.IsLoggedIn)
            {
                guna2HtmlLabel2.Text = AuthManager.CurrentUser.Username; // Tên người dùng
                guna2HtmlLabel1.Text = AuthManager.CurrentUser.Email;    // Email
                // guna2HtmlLabel3.Text = "0814501476"; // (Bạn có thể thêm SĐT vào CSDL nếu muốn)
            }
        }

        /// <summary>
        /// (Nhóm D - Tổng tài audio) Tải thống kê và vẽ biểu đồ
        /// </summary>
        private void LoadStatistics()
        {
            BasicStatistics stats = _utilityService.GetBasicStatistics();

            // 1. Biểu đồ 1 (chart1 - Bar Chart: Top 5 Yêu thích)
            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.Titles.Add("Top 5 Món ăn được Yêu thích nhất");

            Series series1 = chart1.Series.Add("Lượt yêu thích");
            series1.ChartType = SeriesChartType.Bar;

            if (stats.Top5Favorites != null && stats.Top5Favorites.Count > 0)
            {
                foreach (var item in stats.Top5Favorites)
                {
                    series1.Points.AddXY(item.Title, item.TotalFavorites);
                }
            }
            else
            {
                series1.Points.AddXY("Không có dữ liệu", 0);
            }

            // 2. Biểu đồ 2 (chart2 - Pie Chart: Tổng quan)
            chart2.Series.Clear();
            chart2.Titles.Clear();
            chart2.Titles.Add("Tổng quan Hệ thống");

            Series series2 = chart2.Series.Add("Tổng quan");
            series2.ChartType = SeriesChartType.Pie;
            series2.IsValueShownAsLabel = true; // Hiển thị số liệu trên miếng bánh

            series2.Points.AddXY($"Tổng Công thức ({stats.TotalRecipes})", stats.TotalRecipes);
            series2.Points.AddXY($"Tổng Người dùng ({stats.TotalUsers})", stats.TotalUsers);

            chart2.Legends[0].Enabled = true; // Bật chú thích cho Pie chart
        }


        // Helper dùng chung (Bạn đã có)
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // ===== Các sự kiện cho nút/link trên Form =====

        // Nút "Edit"
        private void Guna2Button1_Click(object sender, EventArgs e)
        {
            // TODO: (Nhóm B) Mở form "Edit Profile" (nếu có)
            MessageBox.Show("Chức năng Edit Profile đang được phát triển!");
        }

        // Link "My Recipes"
        private void Guna2HtmlLabel4_Click(object sender, EventArgs e)
        {
            OpenForm(new frmMyRecipes()); // Mở form "Công thức của tôi"
        }

        // Link "My Favorite Recipes"
        private void Guna2HtmlLabel5_Click(object sender, EventArgs e)
        {
            OpenForm(new frmMyFavRecipes()); // Mở form "Yêu thích"
        }

        // Nút icon nhỏ góc phải (guna2Button2) – (Bạn đã có)
        // (NÚT NÀY BÂY GIỜ LÀ NÚT LOGOUT)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Bằng cách gọi Close(), chúng ta sẽ kích hoạt
            // sự kiện 'FrmProfile_FormClosing' đã được thêm ở trên.
            this.Close();
        }

        // ===== Các nút trên thanh menu top (Code của bạn đã có - Giữ nguyên) =====

        // Home
        private void guna2Button7_Click(object sender, EventArgs e)
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
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmCollection();
            OpenForm(f);
        }

        // Profiles (đang ở Profile rồi -> không cần chuyển, nhưng vẫn có handler cho đủ)
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // Đã ở Profiles, không làm gì
        }

        // === HÀM MỚI: XỬ LÝ LOGOUT KHI BẤM NÚT 'X' ===
        /// <summary>
        /// (Nhóm 0 - Khải) Xử lý Đăng xuất khi đóng Form.
        /// </summary>
        private void FrmProfile_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kiểm tra xem người dùng có đang chủ động điều hướng (Hide)
            // hay đang thực sự đóng (Close)
            // Nếu chúng ta chỉ Hide() (như trong OpenForm), e.CloseReason sẽ là None.
            // Chúng ta chỉ Logout khi người dùng bấm nút 'X' (UserClosing) hoặc gọi this.Close().

            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 1. Đăng xuất người dùng
                AuthManager.Logout();

                // 2. Tìm và hiển thị lại form Login đã bị ẩn
                // (Dùng Linq, cần 'using System.Linq;')
                frmLogin loginForm = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();

                if (loginForm != null)
                {
                    // Chế độ Production: Mở lại form Login đã ẩn
                    loginForm.Show();
                }
                else
                {
                    new frmLogin().Show();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}