//frmRecipes.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;   // <-- THÊM
using WinCook.Services; // <-- THÊM
using WinCook.Controls; // <-- THÊM THƯ MỤC CONTROLS
using System.Drawing;
using Guna.UI2.AnimatorNS;
namespace WinCook
{
    public partial class frmRecipes : Form
    {
        // === KHAI BÁO SERVICE (NHÓM A & B) ===
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService; // Dùng cho nút Yêu thích
        private readonly int _currentUserId;
        private List<Recipe> _allRecipes; // Biến lưu trữ danh sách
        private readonly RecipeController _controller;
        public frmRecipes()
        {
            InitializeComponent();

            // Khởi tạo
            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
            _allRecipes = new List<Recipe>();
            _controller = new RecipeController();
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
            

           

            // === SỬA LỖI ĐIỀU HƯỚNG ===
            // Gán sự kiện FormClosing (khi bấm nút 'X')
            this.FormClosing += FrmRecipes_FormClosing;
        }

        private void frmRecipes_Load(object sender, EventArgs e)
        {
            // Tải danh sách công thức
            LoadAllRecipes();
            // Tải danh mục cho ComboBox
            LoadCategories();
        }

        #region === Logic Chính (Nhóm A) ===

        /// <summary>
        /// (Nhóm A - Ktuoi) Tải tất cả công thức từ Service
        /// </summary>
        private void LoadAllRecipes()
        {
            try
            {
                _allRecipes = _controller.GetAllRecipes();  // ⬅️ gọi controller
                PopulateRecipeList(_allRecipes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng khi tải công thức: " + ex.Message);
            }
        }

        /// <summary>
        /// (Nhóm A) Load danh sách Category vào combobox Category by
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                // Lấy list Category từ Service (đọc DB)
                var categories = _recipeService.GetCategories() ?? new List<Category>();

                // Thêm dòng "All" ở đầu để xem tất cả
                categories.Insert(0, new Category
                {
                    CategoryId = 0,
                    Name = "All"
                });

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
        /// (Thay thế hoàn toàn hàm 'BuildRecipeCard' cũ)
        /// </summary>
        private void PopulateRecipeList(List<Recipe> recipes)
        {
            // Xóa hết mấy panel mẫu trong Designer
            flowLayoutPanel1.Controls.Clear();

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

            foreach (var r in recipes)
            {
                // === CARD CHA ===
                var card = new Panel
                {
                    Width = 275,
                    Height = 323,
                    Margin = new Padding(20),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                };

                // === HÌNH ẢNH ===
                var pic = new PictureBox
                {
                    Width = 275,
                    Height = 200,
                    Left = 0,
                    Top = 0,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.White
                };

                // cố gắng load ảnh từ đường dẫn, nếu lỗi thì để trống
                try
                {
                    if (!string.IsNullOrWhiteSpace(r.ImageUrl) && System.IO.File.Exists(r.ImageUrl))
                    {
                        pic.Image = Image.FromFile(r.ImageUrl);
                    }
                }
                catch
                {
                    // nếu lỗi thì cho qua, giữ ảnh trắng
                }

                card.Controls.Add(pic);

                // === PANEL THÔNG TIN ===
                var info = new Panel
                {
                    Width = 275,
                    Height = 120,
                    Left = 0,
                    Top = 200,
                    BackColor = Color.White
                };

                // Tiêu đề
                var lblTitle = new Label
                {
                    Text = r.Title ?? "(Không có tiêu đề)",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    AutoSize = false,
                    Left = 18,
                    Top = 0,
                    Width = 240,
                };

                // Author
                var lblAuthorCaption = new Label
                {
                    Text = "Author :",
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

                // Time
                var lblTimeCaption = new Label
                {
                    Text = "Time :",
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

                // Category
                var lblCateCaption = new Label
                {
                    Text = "Cate :",
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

                // === CLICK MỞ CHI TIẾT ===
                int recipeId = r.RecipeId;    // capture local
                void openDetail(object s, EventArgs e)
                {
                    // Lấy full Recipe từ controller
                    var recipe = _controller.GetRecipeDetails(recipeId);
                    if (recipe == null)
                    {
                        MessageBox.Show("Không tìm thấy chi tiết món ăn.");
                        return;
                    }

                    using (var f = new frmRecipeDetails(recipe))  // ⬅️ truyền MODEL
                    {
                        f.ShowDialog();
                    }

                    LoadAllRecipes();
                }


                card.Click += openDetail;
                pic.Click += openDetail;
                info.Click += openDetail;
                lblTitle.Click += openDetail;
                lblAuthor.Click += openDetail;
                lblTime.Click += openDetail;
                lblCate.Click += openDetail;

                // === THÊM VÀO FLOWLAYOUT ===
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
        // Favorites
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // LUÔN tạo form Favorites mới để nó load lại danh sách từ Service
            var f = new frmMyFavRecipes();
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
            // Nếu chưa có dữ liệu recipe thì thôi
            if (_allRecipes == null || _allRecipes.Count == 0)
                return;

            // Lấy category_id đang chọn
            if (guna2ComboBox1.SelectedValue is int selectedCategoryId)
            {
                // 0 = All -> hiện tất cả
                if (selectedCategoryId == 0)
                {
                    PopulateRecipeList(_allRecipes);
                }
                else
                {
                    // Gọi Service lọc theo CategoryID (logic DB ở Services)
                    var filtered = _recipeService.FilterRecipesByCategory(selectedCategoryId);

                    // Nếu muốn tiết kiệm query, có thể thay bằng lọc _allRecipes theo CategoryName
                    // nhưng mình dùng đúng Service cho clear
                    PopulateRecipeList(filtered);
                }
            }
        }

    }
}