using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using WinCook.Controls; // Đảm bảo đã có namespace này
using System.Drawing;
using Guna.UI2.AnimatorNS;

namespace WinCook
{
    public partial class frmRecipes : Form
    {
        // === KHAI BÁO SERVICE (NHÓM A & B) ===
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService;
        private readonly int _currentUserId;
        private List<Recipe> _allRecipes;
        private readonly RecipeController _controller;

        public frmRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
            _allRecipes = new List<Recipe>();
            _controller = new RecipeController();

            // Lấy ID người dùng
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }

            // Gán sự kiện FormClosing
            this.FormClosing += FrmRecipes_FormClosing;
        }

        private void frmRecipes_Load(object sender, EventArgs e)
        {
            LoadAllRecipes();
            LoadCategories();
        }

        #region === Logic Chính (Nhóm A) ===

        private void LoadAllRecipes()
        {
            try
            {
                _allRecipes = _controller.GetAllRecipes();
                PopulateRecipeList(_allRecipes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi tải công thức: " + ex.Message);
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _recipeService.GetCategories() ?? new List<Category>();
                categories.Insert(0, new Category { CategoryId = 0, Name = "All" });

                guna2ComboBox1.DataSource = categories;
                guna2ComboBox1.DisplayMember = "Name";
                guna2ComboBox1.ValueMember = "CategoryId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load Categories: " + ex.Message);
            }
        }

        /// <summary>
        /// (Nhóm A) Đổ danh sách Recipe vào FlowLayoutPanel
        /// ĐÃ CẬP NHẬT: Sử dụng ucRecipeCard
        /// </summary>
        private void PopulateRecipeList(List<Recipe> recipes)
        {
            // 1. Xóa cũ
            flowLayoutPanel1.Controls.Clear();

            // 2. Kiểm tra rỗng
            if (recipes == null || recipes.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Chưa có công thức nào.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = Color.DimGray,
                    Margin = new Padding(20),
                };
                flowLayoutPanel1.Controls.Add(lblEmpty);
                return;
            }

            // 3. Tạo thẻ ucRecipeCard cho từng công thức
            foreach (var r in recipes)
            {
                // Khởi tạo Card với dữ liệu Recipe
                ucRecipeCard card = new ucRecipeCard(r);

                // Căn chỉnh lề
                card.Margin = new Padding(10);

                // Gán sự kiện Click: Khi bấm vào card thì mở chi tiết
                // (Sử dụng event CardClicked mà bạn đã định nghĩa trong ucRecipeCard)
                card.CardClicked += (s, e) => OnRecipeCardClicked(card, e);

                // Gán sự kiện Chấm điểm xong: Load lại list để cập nhật điểm TB
                card.RatingSubmitted += (s, e) => LoadAllRecipes();

                // Thêm vào danh sách hiển thị
                flowLayoutPanel1.Controls.Add(card);
            }
        }

        /// <summary>
        /// Xử lý khi bấm vào Card -> Mở chi tiết
        /// </summary>
        private void OnRecipeCardClicked(object sender, EventArgs e)
        {
            if (sender is ucRecipeCard card)
            {
                int recipeId = card.GetRecipeId();

                // Kiểm tra tồn tại
                var recipe = _controller.GetRecipeDetails(recipeId);
                if (recipe == null)
                {
                    MessageBox.Show("Không tìm thấy chi tiết món ăn.");
                    return;
                }

                // Mở Form Chi tiết
                using (var f = new frmRecipeDetails(recipeId))
                {
                    f.ShowDialog();
                }

                // Load lại danh sách sau khi đóng chi tiết (để cập nhật view, like...)
                LoadAllRecipes();
            }
        }

        #endregion

        #region === Sự kiện Nút bấm Chức năng (Nhóm A) ===

        // Search
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // Lưu ý: Kiểm tra tên control nhập liệu trong Designer (ví dụ: guna2TextBox1)
            if (guna2TextBox1 == null) return;

            string keyword = guna2TextBox1.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                PopulateRecipeList(_allRecipes);
            }
            else
            {
                var filteredList = _allRecipes
                    .Where(r => r.Title.ToLower().Contains(keyword) ||
                                r.AuthorName.ToLower().Contains(keyword))
                    .ToList();

                PopulateRecipeList(filteredList);
            }
        }

        // Add Recipe
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // Mở form thêm mới
            using (var f = new frmAddRecipie()) // Đảm bảo tên class form đúng
            {
                f.ShowDialog();
            }
            LoadAllRecipes();
        }

        #endregion

        #region === Điều hướng & Logic Form (Giữ nguyên) ===

        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
            if (f == null) f = new frmHomePage();
            OpenForm(f);
        }

        private void guna2Button2_Click(object sender, EventArgs e) { } // Đang ở Recipes

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmCollection>().FirstOrDefault();
            if (f == null) f = new frmCollection();
            OpenForm(f);
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmProfile>().FirstOrDefault();
            if (f == null) f = new frmProfile();
            OpenForm(f);
        }

        private void FrmRecipes_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var homePage = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
                if (homePage != null) homePage.Show();
                else
                {
                    var loginForm = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();
                    if (loginForm != null) loginForm.Show();
                    else new frmLogin().Show();
                }
            }
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_allRecipes == null || _allRecipes.Count == 0) return;

            if (guna2ComboBox1.SelectedValue is int selectedCategoryId)
            {
                if (selectedCategoryId == 0)
                {
                    PopulateRecipeList(_allRecipes);
                }
                else
                {
                    // Lọc danh sách hiện có (Nhanh hơn gọi DB)
                    var filtered = _allRecipes.Where(r => r.CategoryName == guna2ComboBox1.Text).ToList();
                    // Hoặc gọi Service nếu muốn chính xác theo ID:
                    // var filtered = _recipeService.FilterRecipesByCategory(selectedCategoryId);

                    PopulateRecipeList(filtered);
                }
            }
        }

        #endregion

        // Các event giữ lại để Designer không lỗi
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void guna2CircleButton1_Click(object sender, EventArgs e) { }
        private void label45_Click(object sender, EventArgs e) { }
        private void pictureBox6_Click(object sender, EventArgs e) { }
        private void panel12_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
    }
}