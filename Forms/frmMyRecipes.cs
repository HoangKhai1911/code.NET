using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using WinCook.Controls; // Đảm bảo đã có ucMyRecipeCard ở đây

namespace WinCook
{
    public partial class frmMyRecipes : Form
    {
        // === KHAI BÁO SERVICE ===
        private readonly RecipeService _recipeService;
        private readonly int _currentUserId;
        private List<Recipe> _myRecipes; // Biến lưu trữ danh sách gốc

        public frmMyRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _recipeService = new RecipeService();
            _myRecipes = new List<Recipe>();

            // Lấy ID người dùng
            if (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // Nếu chưa đăng nhập
                MessageBox.Show("Vui lòng đăng nhập để xem 'Công thức của tôi'.");
                this.Close();
                return;
            }

            // Gán sự kiện Load và Đóng form
            this.Load += frmMyRecipes_Load;
            this.FormClosing += FrmMyRecipes_FormClosing;

            // Gán sự kiện cho các nút Menu (Đã có trong Designer thì giữ nguyên logic)
            // guna2Button1... (Home, Recipes, Favorites...)
        }

        private void frmMyRecipes_Load(object sender, EventArgs e)
        {
            LoadMyRecipes();
        }

        #region === Logic Chính (QUẢN LÝ MÓN ĂN) ===

        /// <summary>
        /// Tải danh sách món ăn CỦA TÔI từ Database
        /// </summary>
        private void LoadMyRecipes(string keyword = null)
        {
            try
            {
                // 1. Gọi Service lấy danh sách theo AuthorID
                _myRecipes = _recipeService.GetRecipesByAuthor(_currentUserId);

                // 2. Lọc (nếu có từ khóa tìm kiếm)
                List<Recipe> recipesToShow;
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    recipesToShow = _myRecipes;
                }
                else
                {
                    string searchTerm = keyword.ToLower();
                    recipesToShow = _myRecipes
                        .Where(r => r.Title.ToLower().Contains(searchTerm))
                        .ToList();
                }

                // 3. Hiển thị lên giao diện
                PopulateRecipeList(recipesToShow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message);
            }
        }

        /// <summary>
        /// Đổ danh sách vào FlowLayoutPanel sử dụng ucMyRecipeCard
        /// </summary>
        private void PopulateRecipeList(List<Recipe> recipes)
        {
            // Xóa danh sách cũ
            flowLayoutPanel1.Controls.Clear();

            if (recipes == null || recipes.Count == 0)
            {
                Label lbl = new Label { Text = "Chưa có món ăn nào.", AutoSize = true, ForeColor = Color.Gray };
                flowLayoutPanel1.Controls.Add(lbl);
                return;
            }

            foreach (var r in recipes)
            {
                // Tạo thẻ
                ucMyRecipeCard card = new ucMyRecipeCard(r);
                card.Margin = new Padding(10);

                // --- 1. XỬ LÝ SỰ KIỆN SỬA ---
                card.EditClicked += (s, e) =>
                {
                    try
                    {
                        // Mở form AddRecipe ở chế độ Sửa (Truyền ID vào)
                        // Đảm bảo tên class form là frmAddRecipie (hoặc frmAddRecipe tùy file của bạn)
                        using (var f = new frmAddRecipie(r.RecipeId))
                        {
                            if (f.ShowDialog() == DialogResult.OK)
                            {
                                // Sửa xong -> Load lại danh sách
                                LoadMyRecipes(guna2TextBox1?.Text.Trim());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi mở form sửa: " + ex.Message);
                    }
                };

                // --- 2. XỬ LÝ SỰ KIỆN XÓA ---
                card.DeleteClicked += (s, e) =>
                {
                    try
                    {
                        var result = MessageBox.Show($"Bạn chắc chắn muốn xóa '{r.Title}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            // Gọi Service xóa
                            bool isDeleted = _recipeService.DeleteRecipe(r.RecipeId);

                            if (isDeleted)
                            {
                                MessageBox.Show("Đã xóa thành công!");
                                LoadMyRecipes(guna2TextBox1?.Text.Trim()); // Load lại
                            }
                            else
                            {
                                MessageBox.Show("Không thể xóa món ăn này. Vui lòng thử lại.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                    }
                };

                // --- 3. XỬ LÝ SỰ KIỆN XEM CHI TIẾT ---
                card.CardClicked += (s, e) =>
                {
                    using (var f = new frmRecipeDetails(r.RecipeId))
                    {
                        f.ShowDialog();
                    }
                };

                flowLayoutPanel1.Controls.Add(card);
            }
        }

        #endregion

        #region === Các Chức năng khác (Search, Menu...) ===

        // Nút Tìm kiếm (Search)
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // Lấy từ khóa từ textbox (kiểm tra null)
            string keyword = guna2TextBox1 != null ? guna2TextBox1.Text.Trim() : "";
            LoadMyRecipes(keyword);
        }

        // Nút Đóng (Nút X) -> Điều hướng về Home hoặc Login
        private void FrmMyRecipes_FormClosing(object sender, FormClosingEventArgs e)
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

        // Helper chuyển form
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // Menu Navigation
        private void guna2Button1_Click(object sender, EventArgs e) => OpenForm(new frmHomePage()); // Home
        private void guna2Button2_Click(object sender, EventArgs e) => OpenForm(new frmRecipes());  // Recipes
        private void guna2Button5_Click(object sender, EventArgs e) => OpenForm(new frmMyFavRecipes()); // Favorites
        private void guna2Button3_Click(object sender, EventArgs e) => OpenForm(new frmCollection()); // Collections
        private void guna2Button4_Click(object sender, EventArgs e) => OpenForm(new frmProfile()); // Profiles

        // Nút Thoát/Back nhỏ
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region === Các hàm rỗng giữ lại cho Designer ===
        // Giữ nguyên để tránh lỗi file .Designer.cs
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void guna2CircleButton1_Click(object sender, EventArgs e) { }
        private void label45_Click(object sender, EventArgs e) { }
        private void pictureBox6_Click(object sender, EventArgs e) { }
        private void panel12_Paint(object sender, PaintEventArgs e) { }
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
        #endregion
    }
}