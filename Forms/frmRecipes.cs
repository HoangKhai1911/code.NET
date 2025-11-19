using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;   // <-- THÊM
using WinCook.Services; // <-- THÊM
using WinCook.Controls; // <-- THÊM THƯ MỤC CONTROLS

namespace WinCook
{
    public partial class frmRecipes : Form
    {
        // === KHAI BÁO SERVICE (NHÓM A & B) ===
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService; // Dùng cho nút Yêu thích
        private readonly int _currentUserId;
        private List<Recipe> _allRecipes; // Biến lưu trữ danh sách

        public frmRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
            _allRecipes = new List<Recipe>();

            // Lấy ID người dùng (nếu đã đăng nhập)
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // (Tùy chọn) Có thể đóng nếu bắt buộc đăng nhập
                // MessageBox.Show("Vui lòng đăng nhập.");
                // this.Close();
                // return;
            }

            // === Gán sự kiện (khớp với file Designer của bạn) ===
            this.Load += frmRecipes_Load;

            // Menu
            guna2Button1.Click += guna2Button1_Click; // Home
            guna2Button2.Click += guna2Button2_Click; // Recipes
            guna2Button5.Click += guna2Button5_Click; // Favorites
            guna2Button3.Click += guna2Button3_Click; // Collections
            guna2Button4.Click += guna2Button4_Click; // Profiles

            // Chức năng
            guna2Button6.Click += guna2Button6_Click; // Search
            guna2Button7.Click += guna2Button7_Click; // Add (Nút '+')

            // === SỬA LỖI ĐIỀU HƯỚNG ===
            // Gán sự kiện FormClosing (khi bấm nút 'X')
            this.FormClosing += FrmRecipes_FormClosing;
        }

        private void frmRecipes_Load(object sender, EventArgs e)
        {
            // Tải danh sách công thức
            LoadAllRecipes();
        }

        #region === Logic Chính (Nhóm A) ===

        /// <summary>
        /// (Nhóm A - Ktuoi) Tải tất cả công thức từ Service
        /// </summary>
        private void LoadAllRecipes()
        {
            try
            {
                // 1. Gọi Service
                _allRecipes = _recipeService.GetAllRecipes();

                // 2. Hiển thị lên
                PopulateRecipeList(_allRecipes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi tải công thức: " + ex.Message);
            }
        }

        /// <summary>
        /// (Nhóm A) Đổ danh sách Recipe vào FlowLayoutPanel
        /// (Thay thế hoàn toàn hàm 'BuildRecipeCard' cũ)
        /// </summary>
        private void PopulateRecipeList(List<Recipe> recipes)
        {
            // Giả định bạn có FlowLayoutPanel tên là 'flowLayoutPanel1' (theo code cũ)
            flowLayoutPanel1.Controls.Clear(); // Xóa các thẻ cũ

            if (recipes == null || recipes.Count == 0)
            {
                // TODO: Hiển thị 1 Label thông báo "Không tìm thấy công thức"
                return;
            }

            // Lặp qua 30 công thức
            foreach (var recipe in recipes)
            {
                // 1. Tạo "Khuôn" (ucRecipeCard)
                // (Đảm bảo 'ucRecipeCard.cs' nằm trong namespace 'WinCook.Controls')
                ucRecipeCard card = new ucRecipeCard(recipe);

                // 2. Gán sự kiện Click cho thẻ
                // (Khi bấm vào thẻ, nó sẽ kích hoạt sự kiện CardClicked)
                card.CardClicked += OnRecipeCardClicked;

                // 3. Thêm "Khuôn" vào "Khay" (flowLayoutPanel1)
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
                // Lấy ID từ thẻ được bấm
                int recipeId = card.GetRecipeId();

                // Mở form Chi tiết (Nhóm B)
                frmRecipeDetails frmDetail = new frmRecipeDetails(recipeId);

                // Dùng ShowDialog() để nó chặn form này
                // Khi form Chi tiết đóng, nó sẽ quay lại đây...
                frmDetail.ShowDialog();

                // ...và chúng ta tải lại danh sách
                // (Để cập nhật nếu người dùng vừa bấm Yêu thích)
                LoadAllRecipes();
            }
        }

        #endregion

        #region === Sự kiện Nút bấm Chức năng (Nhóm A) ===

        // Search (Nút 'guna2Button6')
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // Lấy keyword từ ô search (Giả định tên là 'guna2TextBox1' - theo code cũ)
            string keyword = guna2TextBox1.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Không nhập gì -> load toàn bộ
                PopulateRecipeList(_allRecipes);
            }
            else
            {
                // Có từ khoá -> Lọc danh sách đã tải (rất nhanh)
                var filteredList = _allRecipes
                    .Where(r => r.Title.ToLower().Contains(keyword) ||
                                r.AuthorName.ToLower().Contains(keyword))
                    .ToList();

                PopulateRecipeList(filteredList);
            }
        }

        // Add (Nút '+' - 'guna2Button7')
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // Mở form thêm / sửa công thức

            // === SỬA LỖI CS0246 ===
            // Đổi tên form từ 'frmAdd_EditRecipie' thành 'frmAddRecipie'
            // (Khớp với tên form bạn dùng trong frmRecipeDetails.cs)
            using (var f = new frmAddRecipie())
            {
                f.ShowDialog(); // mở modal, nhập xong bấm Add sẽ đóng form
            }

            // Sau khi form đóng, load lại danh sách để thấy recipe mới
            LoadAllRecipes();
        }

        #endregion

        #region === SỬA LỖI ĐIỀU HƯỚNG ===

        // Helper dùng chung: Đóng form hiện tại
        private void OpenForm(Form f)
        {
            f.Show();

            // === SỬA LỖI CS1061 ===
            // XÓA DÒNG NÀY:
            // this.CloseReason = CloseReason.None; 

            // Chỉ cần gọi Close(). Logic trong FrmRecipes_FormClosing
            // sẽ tự động kiểm tra và không logout 2 lần.
            this.Hide();
        }

        // ===== Thanh menu trên cùng của frmRecipes =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
            if (f == null) f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes (đang ở Recipes rồi -> không làm gì)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Đang ở Recipes, không cần chuyển
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
        private void FrmRecipes_FormClosing(object sender, FormClosingEventArgs e)
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

        #region === Các hàm Designer trống (để tránh lỗi Build) ===
        // (Đây là các hàm rỗng mà code cũ của bạn có,
        // chúng ta giữ lại để làm hài lòng file Designer)
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void guna2CircleButton1_Click(object sender, EventArgs e) { }
        private void label45_Click(object sender, EventArgs e) { }
        private void pictureBox6_Click(object sender, EventArgs e) { }
        private void panel12_Paint(object sender, PaintEventArgs e) { }
        #endregion

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}