using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using WinCook.Controls;

namespace WinCook
{
    public partial class frmMyRecipes : Form
    {
        // === KHAI BÁO SERVICE (NHÓM A) ===
        private readonly RecipeService _recipeService;
        private readonly int _currentUserId;
        private List<Recipe> _myRecipes; // Biến lưu trữ danh sách

        public frmMyRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _recipeService = new RecipeService();
            _myRecipes = new List<Recipe>();

            // Lấy ID người dùng (nếu đã đăng nhập)
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // Nếu chưa đăng nhập, đóng form
                MessageBox.Show("Vui lòng đăng nhập để xem 'Công thức của tôi'.");
                this.Close();
                return;
            }

            // === Gán sự kiện (Giả định tên control giống frmMyFavRecipes) ===
            this.Load += frmMyRecipes_Load;

            // Menu
            guna2Button1.Click += guna2Button1_Click; // Home
            guna2Button2.Click += guna2Button2_Click; // Recipes
            guna2Button5.Click += guna2Button5_Click; // Favorites
            guna2Button3.Click += guna2Button3_Click; // Collections
            guna2Button4.Click += guna2Button4_Click; // Profiles

            // Chức năng
            guna2Button6.Click += guna2Button6_Click; // Search

            // === SỬA LỖI ĐIỀU HƯỚNG ===
            this.FormClosing += FrmMyRecipes_FormClosing;
        }

        private void frmMyRecipes_Load(object sender, EventArgs e)
        {
            // Tải danh sách công thức
            LoadMyRecipes();
        }

        #region === Logic Chính (Nhóm A) ===

        /// <summary>
        /// (Nhóm A) Tải tất cả công thức CỦA TÔI từ Service
        /// </summary>
        private void LoadMyRecipes(string keyword = null)
        {
            try
            {
                // 1. Gọi Service (Tải lại dữ liệu mới nhất mỗi lần load)
                _myRecipes = _recipeService.GetRecipesByAuthor(_currentUserId); // <-- GỌI HÀM MỚI

                // 2. Lọc (nếu có)
                List<Recipe> recipesToShow;
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    recipesToShow = _myRecipes; // Hiển thị tất cả
                }
                else
                {
                    // Lọc danh sách đã tải
                    string searchTerm = keyword.ToLower();
                    recipesToShow = _myRecipes
                        .Where(r => r.Title.ToLower().Contains(searchTerm))
                        .ToList();
                }

                // 3. Hiển thị lên
                PopulateRecipeList(recipesToShow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi tải 'Công thức của tôi': " + ex.Message);
            }
        }

        /// <summary>
        /// (Nhóm A) Đổ danh sách Recipe vào FlowLayoutPanel
        /// </summary>
        private void PopulateRecipeList(List<Recipe> recipes)
        {
            // Giả định bạn có FlowLayoutPanel tên là 'flowLayoutPanel1'
            flowLayoutPanel1.Controls.Clear(); // Xóa các thẻ cũ

            if (recipes == null || recipes.Count == 0)
            {
                Label lblEmpty = new Label();
                lblEmpty.Text = "Bạn chưa đăng công thức nào.";
                lblEmpty.Font = new System.Drawing.Font("Segoe UI", 12F);
                lblEmpty.ForeColor = System.Drawing.Color.Gray;
                lblEmpty.AutoSize = false;
                lblEmpty.Width = flowLayoutPanel1.Width;
                lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                flowLayoutPanel1.Controls.Add(lblEmpty);
                return;
            }

            foreach (var recipe in recipes)
            {
                ucRecipeCard card = new ucRecipeCard(recipe);
                card.CardClicked += OnRecipeCardClicked;
                flowLayoutPanel1.Controls.Add(card);
            }
        }

        /// <summary>
        /// (Nhóm A) Được gọi khi bấm vào bất kỳ thẻ 'ucRecipeCard' nào
        /// </summary>
        private void OnRecipeCardClicked(object sender, EventArgs e)
        {
            if (sender is ucRecipeCard card)
            {
                int recipeId = card.GetRecipeId();

                // Mở form Chi tiết
                frmRecipeDetails frmDetail = new frmRecipeDetails(recipeId);
                frmDetail.ShowDialog();

                // Khi form Chi tiết đóng, tải lại danh sách
                // (Phòng trường hợp người dùng Sửa/Xóa công thức của họ)
                _myRecipes.Clear();
                LoadMyRecipes(guna2TextBox1.Text.Trim()); // Tải lại
            }
        }

        #endregion

        #region === Sự kiện Nút bấm Chức năng (Nhóm A) ===

        // Search (Nút 'guna2Button6')
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // Lấy keyword từ ô search (Giả định tên là 'guna2TextBox1')
            string keyword = guna2TextBox1.Text.Trim();

            // Chỉ cần gọi lại LoadMyRecipes với keyword
            LoadMyRecipes(keyword);
        }

        #endregion

        #region === SỬA LỖI ĐIỀU HƯỚNG ===

        // Helper dùng chung: Đóng form hiện tại
        private void OpenForm(Form f)
        {
            f.Show();
            this.Close();
        }

        // ===== Thanh menu trên cùng của frmMyRecipes =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
            if (f == null) f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmRecipes>().FirstOrDefault();
            if (f == null) f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmMyFavRecipes>().FirstOrDefault();
            if (f == null) f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmCollection>().FirstOrDefault();
            if (f == null) f = new frmCollection();
            OpenForm(f);
        }

        // Profiles
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmProfile>().FirstOrDefault();
            if (f == null) f = new frmProfile();
            OpenForm(f);
        }

        // === HÀM MỚI: XỬ LÝ KHI BẤM NÚT 'X' ===
        private void FrmMyRecipes_FormClosing(object sender, FormClosingEventArgs e)
        {
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
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void guna2CircleButton1_Click(object sender, EventArgs e) { }
        private void label45_Click(object sender, EventArgs e) { }
        private void pictureBox6_Click(object sender, EventArgs e) { }
        private void panel12_Paint(object sender, PaintEventArgs e) { }

        // --- Bổ sung từ lỗi 'image_0dfd25.png' ---
        private void guna2Button8_Click(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void guna2Button9_Click(object sender, EventArgs e) { }
        private void guna2Button10_Click(object sender, EventArgs e) { }
        private void guna2Button11_Click(object sender, EventArgs e) { }
        private void guna2Button12_Click(object sender, EventArgs e) { }
        private void guna2Button13_Click(object sender, EventArgs e) { }
        private void guna2Button1_Click_1(object sender, EventArgs e) { }
        private void guna2Button7_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}