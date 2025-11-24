using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using WinCook.Controls;
using Guna.UI2.WinForms;

namespace WinCook
{
    public partial class frmRecipeDetails : Form
    {
        // Service
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService;
        private readonly UtilityService _utilityService;

        // Trạng thái
        private readonly int _recipeId;
        private readonly int _currentUserId;
        private Recipe _currentRecipe;
        private bool _isCurrentlyFavorite;
        private int _commentLimit = 10;

        public frmRecipeDetails(int id)
        {
            InitializeComponent();

            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
            _utilityService = new UtilityService();
            _recipeId = id;

            // Kiểm tra đăng nhập an toàn
            if (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                _currentUserId = 0; // Chế độ khách (hoặc xử lý tùy ý)
            }

            this.Load += (s, e) => LoadDetails();

            // Gán sự kiện nút (Kiểm tra null để tránh lỗi nếu control chưa được khởi tạo)
            if (guna2Button1 != null) guna2Button1.Click += btnAddToCollection_Click;
            if (guna2Button2 != null) guna2Button2.Click += btnFavorite_Click;

            // Tìm và gán sự kiện cho nút Đăng bình luận
            var btnPost = this.Controls.Find("button2", true).FirstOrDefault() as Guna2Button;
            if (btnPost != null) btnPost.Click += btnPostComment_Click;

            // Nút Quay lại (button1) - Xử lý cho cả trường hợp nút nằm trực tiếp trên Form hoặc trong Panel
            if (button1 != null)
            {
                button1.Click += button1_Click;
            }
            else
            {
                var btnBack = this.Controls.Find("button1", true).FirstOrDefault() as Button;
                if (btnBack != null) btnBack.Click += button1_Click;
            }
        }

        private void LoadDetails()
        {
            try
            {
                // 1. Tải thông tin từ Service nếu chưa có
                if (_currentRecipe == null)
                {
                    _currentRecipe = _recipeService.GetRecipeDetails(_recipeId);
                }

                // Kiểm tra nếu không tìm thấy recipe (có thể do ID sai hoặc DB lỗi)
                if (_currentRecipe == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu công thức.", "Lỗi");
                    this.Close();
                    return;
                }

                // 2. Gán dữ liệu lên UI (Kiểm tra null cho từng control để tránh lỗi hiển thị)
                if (guna2HtmlLabel3 != null) guna2HtmlLabel3.Text = _currentRecipe.Title;
                if (guna2HtmlLabel5 != null) guna2HtmlLabel5.Text = _currentRecipe.AuthorName;
                if (guna2HtmlLabel7 != null) guna2HtmlLabel7.Text = _currentRecipe.CategoryName;

                if (guna2HtmlLabel11 != null)
                    guna2HtmlLabel11.Text = !string.IsNullOrEmpty(_currentRecipe.TimeNeeded) ? _currentRecipe.TimeNeeded : "N/A";

                if (guna2HtmlLabel9 != null)
                    guna2HtmlLabel9.Text = !string.IsNullOrEmpty(_currentRecipe.Difficulty) ? _currentRecipe.Difficulty : "Medium";

                // Tải ảnh (Xử lý ngoại lệ riêng cho ảnh để không ảnh hưởng luồng chính)
                if (pictureBox1 != null && !string.IsNullOrEmpty(_currentRecipe.ImageUrl))
                {
                    try
                    {
                        if (System.IO.File.Exists(_currentRecipe.ImageUrl))
                            pictureBox1.ImageLocation = _currentRecipe.ImageUrl;
                        // Nếu là URL web thì có thể cần dùng pictureBox1.LoadAsync(_currentRecipe.ImageUrl);
                    }
                    catch { }
                }

                // 3. Tải nội dung Nguyên liệu và Các bước
                // Quan trọng: Sử dụng toán tử ?? "" để đảm bảo không truyền null vào hàm xử lý chuỗi
                PopulateIngredients(_currentRecipe.Ingredients ?? "");
                PopulateSteps(_currentRecipe.Steps ?? "");

                // 4. Tải thông tin tương tác (Yêu thích, Bình luận)
                LoadInteractionData();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi bất ngờ, hiện thông báo chi tiết để debug
                MessageBox.Show("Lỗi hiển thị chi tiết: " + ex.Message);
            }
        }

        private void PopulateIngredients(string ingredientsText)
        {
            if (guna2HtmlLabel12 == null) return;

            if (!string.IsNullOrEmpty(ingredientsText))
            {
                // Thay xuống dòng bằng <br> cho Guna Label hiển thị đẹp (nếu dùng HTML render)
                // Hoặc giữ nguyên \n nếu GunaLabel của bạn không bật HTML formatting
                guna2HtmlLabel12.Text = ingredientsText.Replace("\n", "<br>");
            }
            else
            {
                guna2HtmlLabel12.Text = "(Không có nguyên liệu)";
            }
        }

        private void PopulateSteps(string stepsText)
        {
            if (guna2HtmlLabel15 == null) return;

            if (!string.IsNullOrEmpty(stepsText))
            {
                // Thay xuống dòng bằng <br><br> để thoáng mắt
                guna2HtmlLabel15.Text = stepsText.Replace("\n", "<br><br>");
            }
            else
            {
                guna2HtmlLabel15.Text = "(Chưa có bước làm)";
            }
        }

        private void LoadInteractionData()
        {
            if (_currentUserId == 0) return;

            _isCurrentlyFavorite = _interactionService.IsRecipeFavorited(_currentUserId, _recipeId);
            UpdateFavoriteButtonVisuals();
            LoadAllRatings();
        }

        private void LoadAllRatings()
        {
            try
            {
                List<Rating> ratings = _interactionService.GetRatingsForRecipe(_recipeId);

                // Tìm FlowLayoutPanel chứa comment
                Control container = this.Controls.Find("flpComments", true).FirstOrDefault();
                if (container == null) return;

                FlowLayoutPanel flp = (FlowLayoutPanel)container;
                flp.Controls.Clear();

                // Header số lượng
                Label lblHeader = new Label();
                lblHeader.Text = $"Rating Comment ({ratings.Count})";
                lblHeader.Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold);
                lblHeader.AutoSize = true;
                lblHeader.Margin = new Padding(0, 0, 0, 10);
                flp.Controls.Add(lblHeader);

                if (ratings.Count == 0)
                {
                    Label lblEmpty = new Label();
                    lblEmpty.Text = "Chưa có đánh giá nào.";
                    lblEmpty.AutoSize = true;
                    lblEmpty.ForeColor = Color.Gray;
                    flp.Controls.Add(lblEmpty);
                    return;
                }

                // Hiển thị comment (có giới hạn)
                int count = 0;
                foreach (var rating in ratings)
                {
                    if (count >= _commentLimit) break;

                    // Đảm bảo ucCommentItem đã được tạo đúng
                    ucCommentItem item = new ucCommentItem(rating);
                    // Chỉnh width trừ đi thanh cuộn (khoảng 25px)
                    item.Width = flp.ClientSize.Width > 25 ? flp.ClientSize.Width - 25 : 300;
                    flp.Controls.Add(item);
                    count++;
                }

                // Nút Xem thêm
                if (ratings.Count > _commentLimit)
                {
                    Button btnMore = new Button();
                    btnMore.Text = "Xem thêm...";
                    btnMore.Click += (s, e) => { _commentLimit += 10; LoadAllRatings(); };
                    flp.Controls.Add(btnMore);
                }
            }
            catch { /* Bỏ qua lỗi load comment để không ảnh hưởng UI chính */ }
        }

        // --- CÁC SỰ KIỆN ---

        private void btnFavorite_Click(object sender, EventArgs e)
        {
            if (_currentUserId == 0) { MessageBox.Show("Cần đăng nhập!"); return; }

            if (_isCurrentlyFavorite) _interactionService.RemoveFavorite(_currentUserId, _recipeId);
            else _interactionService.AddFavorite(_currentUserId, _recipeId);

            _isCurrentlyFavorite = !_isCurrentlyFavorite;
            UpdateFavoriteButtonVisuals();
        }

        private void UpdateFavoriteButtonVisuals()
        {
            if (guna2Button2 == null) return;
            if (_isCurrentlyFavorite)
                guna2Button2.FillColor = Color.FromArgb(255, 128, 128); // Màu đỏ nhạt (Đã thích)
            else
                guna2Button2.FillColor = Color.LightSalmon; // Màu gốc (Chưa thích)
        }

        private void btnPostComment_Click(object sender, EventArgs e)
        {
            if (_currentUserId == 0) { MessageBox.Show("Cần đăng nhập!"); return; }

            float score = 0;
            // Tìm control rating
            var ratingControl = this.Controls.Find("rsMyRating", true).FirstOrDefault() as Guna2RatingStar;
            if (ratingControl == null) ratingControl = this.Controls.Find("guna2RatingStar1", true).FirstOrDefault() as Guna2RatingStar;
            if (ratingControl != null) score = ratingControl.Value;

            // Tìm control comment
            string comment = "";
            var txtControl = this.Controls.Find("txtMyComment", true).FirstOrDefault() as Guna2TextBox;
            if (txtControl == null) txtControl = this.Controls.Find("guna2TextBox1", true).FirstOrDefault() as Guna2TextBox;

            if (txtControl != null) comment = txtControl.Text.Trim();

            if (score <= 0)
            {
                MessageBox.Show("Vui lòng chọn số sao!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Rating newRating = new Rating
            {
                UserId = _currentUserId,
                RecipeId = _recipeId,
                Score = (int)Math.Round(score),
                Comment = comment
            };

            if (_interactionService.AddOrUpdateRating(newRating))
            {
                MessageBox.Show("Đánh giá thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (txtControl != null) txtControl.Clear();
                if (ratingControl != null) ratingControl.Value = 0;
                _commentLimit = 10;
                LoadAllRatings(); // Reload lại comment ngay lập tức
            }
            else
            {
                MessageBox.Show("Lỗi khi lưu đánh giá.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddToCollection_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tính năng đang phát triển.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Các hàm placeholder để tránh lỗi Designer (giữ nguyên)
        private void label1_Click(object sender, EventArgs e) { }
        private void guna2RatingStar1_ValueChanged(object sender, EventArgs e) { }
        private void BtnDelete_Click(object sender, EventArgs e) { }
        private void nudServingsFactor_ValueChanged(object sender, EventArgs e) { }
        private void btnCookingMode_Click(object sender, EventArgs e) { }
        private void btnSaveRating_Click(object sender, EventArgs e) { }
        private void btnSaveNote_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
    }
}