using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;   // <-- THÊM
using WinCook.Services; // <-- THÊM
using WinCook.Controls; // <-- THÊM THƯ MỤC CONTROLS

namespace WinCook
{
    public partial class frmMyFavRecipes : Form
    {
        // === KHAI BÁO SERVICE (NHÓM B) ===
        private readonly InteractionService _interactionService;
        private readonly RecipeController _controller;
        private readonly int _currentUserId;
        private List<Recipe> _allFavoriteRecipes; // Biến lưu trữ danh sách

        public frmMyFavRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _interactionService = new InteractionService();
            _controller = new RecipeController();
            _allFavoriteRecipes = new List<Recipe>();

            // Lấy ID người dùng (nếu đã đăng nhập)
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // Nếu chưa đăng nhập, đóng form
                MessageBox.Show("Vui lòng đăng nhập để xem Yêu thích.");
                this.Close();
                return;
            }

            // === Gán sự kiện (khớp với file Designer của bạn) ===
            this.Load += frmMyFavRecipes_Load;

            // Menu
            guna2Button1.Click += guna2Button1_Click; // Home
            guna2Button2.Click += guna2Button2_Click; // Recipes
            guna2Button5.Click += guna2Button5_Click; // Favorites
            guna2Button3.Click += guna2Button3_Click; // Collections
            guna2Button4.Click += guna2Button4_Click; // Profiles

            // Chức năng
            guna2Button6.Click += guna2Button6_Click; // Search

            // === SỬA LỖI ĐIỀU HƯỚNG ===
            // Gán sự kiện FormClosing (khi bấm nút 'X')
            this.FormClosing += FrmMyFavRecipes_FormClosing;
        }

        private void frmMyFavRecipes_Load(object? sender, EventArgs e)
        {
            // Tải danh sách công thức
            LoadFavorites();
        }

        #region === Logic Chính (Nhóm B - Tổng tài audio) ===

        /// <summary>
        /// (Nhóm B) Tải tất cả công thức YÊU THÍCH từ Service
        /// </summary>
        private void LoadFavorites(string? keyword = null)
        {
            try
            {
                // 1. Lấy list favorites qua CONTROLLER
                _allFavoriteRecipes = _controller.GetFavoriteRecipes(_currentUserId);

                // 2. Lọc (nếu có)
                List<Recipe> recipesToShow;
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    recipesToShow = _allFavoriteRecipes;
                }
                else
                {
                    string searchTerm = keyword.ToLower();
                    recipesToShow = _allFavoriteRecipes
                        .Where(r =>
                            (r.Title ?? "").ToLower().Contains(searchTerm) ||
                            (r.AuthorName ?? "").ToLower().Contains(searchTerm))
                        .ToList();
                }

                // 3. Bind ra UI
                PopulateFavoriteList(recipesToShow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi tải công thức yêu thích: " + ex.Message);
            }
        }


        /// <summary>
        /// (Nhóm A/B) Đổ danh sách Recipe vào FlowLayoutPanel
        /// (Sử dụng 'ucRecipeCard')
        /// </summary>
        private void PopulateFavoriteList(List<Recipe> recipes)
        {
            // Giả định bạn có FlowLayoutPanel tên là 'flowLayoutPanel1' (theo code cũ)
            flowLayoutPanel1.Controls.Clear(); // Xóa các thẻ cũ

            if (recipes == null || recipes.Count == 0)
            {
                // Hiển thị 1 Label thông báo "Bạn chưa yêu thích công thức nào"
                Label lblEmpty = new Label();
                lblEmpty.Text = "Bạn chưa có công thức yêu thích nào.";
                lblEmpty.Font = new System.Drawing.Font("Segoe UI", 12F);
                lblEmpty.ForeColor = System.Drawing.Color.Gray;
                lblEmpty.AutoSize = false;
                lblEmpty.Width = flowLayoutPanel1.Width - 50; // Trừ 50 margin
                lblEmpty.Height = 100;
                lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                flowLayoutPanel1.Controls.Add(lblEmpty);
                return;
            }

            // Lặp qua các công thức yêu thích
            foreach (var r in recipes)
            {
                var card = new Panel
                {
                    Width = 255,
                    Height = 323,
                    Margin = new Padding(10),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                };

                var pic = new PictureBox
                {
                    Width = 275,
                    Height = 200,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.White
                };

                try
                {
                    if (!string.IsNullOrWhiteSpace(r.ImageUrl) && File.Exists(r.ImageUrl))
                        pic.Image = Image.FromFile(r.ImageUrl);
                }
                catch { }

                card.Controls.Add(pic);

                var info = new Panel
                {
                    Width = 275,
                    Height = 120,
                    Top = 200,
                    BackColor = Color.White
                };

                var lblTitle = new Label
                {
                    Text = r.Title ?? "(No title)",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    AutoSize = false,
                    Left = 18,
                    Top = 0,
                    Width = 240,
                };

                var lblAuthorCaption = new Label
                {
                    Text = "Author:",
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 18,
                    Top = 25
                };

                var lblAuthor = new Label
                {
                    Text = r.AuthorName ?? "",
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 84,
                    Top = 25
                };

                var lblTimeCaption = new Label
                {
                    Text = "Time:",
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 18,
                    Top = 47
                };

                var lblTime = new Label
                {
                    Text = string.IsNullOrWhiteSpace(r.TimeNeeded) ? "N/A" : r.TimeNeeded,
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 84,
                    Top = 47
                };

                var lblCateCaption = new Label
                {
                    Text = "Cate:",
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 18,
                    Top = 69
                };

                var lblCate = new Label
                {
                    Text = r.CategoryName ?? "N/A",
                    Font = new Font("Segoe UI Semibold", 10),
                    AutoSize = true,
                    Left = 84,
                    Top = 69
                };

                info.Controls.Add(lblTitle);
                info.Controls.Add(lblAuthorCaption);
                info.Controls.Add(lblAuthor);
                info.Controls.Add(lblTimeCaption);
                info.Controls.Add(lblTime);
                info.Controls.Add(lblCateCaption);
                info.Controls.Add(lblCate);

                card.Controls.Add(info);

                // ========== CLICK mở chi tiết ==========
                int recipeId = r.RecipeId;

                void openDetail(object? s, EventArgs e)
                {
                    var recipe = _controller.GetRecipeDetails(recipeId);
                    using (var f = new frmRecipeDetails(recipeId))
                        f.ShowDialog();

                    LoadFavorites();
                }

                card.Click += openDetail;
                pic.Click += openDetail;
                info.Click += openDetail;
                lblTitle.Click += openDetail;
                lblAuthor.Click += openDetail;
                lblTime.Click += openDetail;
                lblCate.Click += openDetail;

                flowLayoutPanel1.Controls.Add(card);
            }

        }

        /// <summary>
        /// (Nhóm A/B) Được gọi khi bấm vào bất kỳ thẻ 'ucRecipeCard' nào
        /// </summary>
        private void OnRecipeCardClicked(object? sender, EventArgs e)
        {
            if (sender is ucRecipeCard card)
            {
                int recipeId = card.GetRecipeId();

                // ✅ Lấy FULL Recipe detail từ controller
                var recipe = _controller.GetRecipeDetails(recipeId);
                if (recipe == null)
                {
                    MessageBox.Show("Không tìm thấy chi tiết món ăn.");
                    return;
                }

                // ✅ Truyền ID Recipe vào form Details (constructor expects int)
                using (var frmDetail = new frmRecipeDetails(recipeId))
                {
                    frmDetail.ShowDialog();
                }

                // Sau khi đóng Details, reload lại favorites (vì user có thể bỏ tim)
                _allFavoriteRecipes.Clear();
                LoadFavorites(guna2TextBox1.Text.Trim());
            }
        }


        #endregion

        #region === Sự kiện Nút bấm Chức năng (Nhóm B) ===

        // Search (Nút 'guna2Button6')
        private void guna2Button6_Click(object? sender, EventArgs e)
        {
            // Lấy keyword từ ô search (Giả định tên là 'guna2TextBox1' - theo code cũ)
            string keyword = guna2TextBox1.Text.Trim();

            // Chỉ cần gọi lại LoadFavorites với keyword
            LoadFavorites(keyword);
        }

        #endregion

        #region === SỬA LỖI ĐIỀU HƯỚNG ===

        // Helper dùng chung: Đóng form hiện tại
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide(); // Sửa từ 'Hide()' thành 'Close()' (comment cũ của bạn)
        }

        // ===== Thanh menu trên cùng của frmMyFavRecipes =====

        // Home
        private void guna2Button1_Click(object? sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
            if (f == null) f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button2_Click(object? sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmRecipes>().FirstOrDefault();
            if (f == null) f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites (đang ở đây nên không làm gì)
        private void guna2Button5_Click(object? sender, EventArgs e)
        {
            // Đang ở Favorites, không cần chuyển
        }

        // Collections
        private void guna2Button3_Click(object? sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmCollection>().FirstOrDefault();
            if (f == null) f = new frmCollection();
            OpenForm(f);
        }

        // Profiles
        private void guna2Button4_Click(object? sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmProfile>().FirstOrDefault();
            if (f == null) f = new frmProfile();
            OpenForm(f);
        }

        // === HÀM MỚI: XỬ LÝ KHI BẤM NÚT 'X' ===
        private void FrmMyFavRecipes_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Chỉ xử lý khi người dùng bấm 'X'
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Tìm frmHomePage CŨ (đang bị ẩn) và hiển thị lại
                var homePage = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
                if (homePage != null)
                {
                    homePage.Show();
                }
                else
                {
                    // Fallback: Nếu không tìm thấy (ví dụ: đang test)
                    var loginForm = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();
                    if (loginForm != null)
                    {
                        loginForm.Show();
                    }
                    else
                    {
                        new frmLogin().Show();
                    }
                }
            }
        }

        #endregion

        // (Đây là các hàm rỗng từ file v1 của bạn, giữ lại để tránh lỗi Designer)
        private void flowLayoutPanel1_Paint(object? sender, PaintEventArgs e)
        {
        }

        private void guna2Button15_Click(object? sender, EventArgs e)
        {
        }
    }
}