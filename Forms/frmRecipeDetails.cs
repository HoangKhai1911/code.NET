using WinCook.Controls;
using WinCook.Models;
using WinCook.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmRecipeDetails : Form
    {
        // Services
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

            _currentUserId = (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
                                ? AuthManager.CurrentUser.UserId
                                : 0;

            this.Load += (s, e) => LoadDetails();

            guna2Button1.Click += btnAddToCollection_Click;
            guna2Button2.Click += btnFavorite_Click;
            button1.Click += button1_Click;
            button2.Click += btnPostComment_Click;

            DebugServices();
        }

        /* GIỮ LẠI VÀ IMPLEMENT PHƯƠNG THỨC flpComments_Paint
        private void flpComments_Paint(object sender, PaintEventArgs e)
        {
            // Custom painting cho FlowLayoutPanel
            try
            {
                // Vẽ border cho flpComments
                using (Pen borderPen = new Pen(Color.LightGray, 1))
                {
                    e.Graphics.DrawRectangle(borderPen,
                        new Rectangle(0, 0, flpComments.Width - 1, flpComments.Height - 1));
                }

                // Nếu không có comments, có thể vẽ thông báo
                if (flpComments.Controls.Count == 0)
                {
                    string message = "Chưa có đánh giá nào";
                    using (Font messageFont = new Font("Segoe UI", 9, FontStyle.Italic))
                    using (SolidBrush messageBrush = new SolidBrush(Color.Gray))
                    {
                        SizeF textSize = e.Graphics.MeasureString(message, messageFont);
                        float x = (flpComments.Width - textSize.Width) / 2;
                        float y = (flpComments.Height - textSize.Height) / 2;

                        e.Graphics.DrawString(message, messageFont, messageBrush, x, y);
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi painting nếu có
                Console.WriteLine($"Lỗi trong flpComments_Paint: {ex.Message}");
            }
        }
        */


        private void DebugServices()
        {
            try
            {
                Console.WriteLine($"RecipeService: {_recipeService != null}");
                Console.WriteLine($"InteractionService: {_interactionService != null}");
                Console.WriteLine($"Current User ID: {_currentUserId}");
                Console.WriteLine($"Recipe ID: {_recipeId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug Error: {ex.Message}");
            }
        }

        private void LoadDetails()
        {
            try
            {
                _currentRecipe ??= _recipeService.GetRecipeDetails(_recipeId);

                if (_currentRecipe == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu công thức.");
                    this.Close();
                    return;
                }

                guna2HtmlLabel3.Text = _currentRecipe.Title;
                guna2HtmlLabel5.Text = _currentRecipe.AuthorName;
                guna2HtmlLabel7.Text = _currentRecipe.CategoryName;
                guna2HtmlLabel11.Text = _currentRecipe.TimeNeeded ?? "N/A";
                guna2HtmlLabel9.Text = _currentRecipe.Difficulty ?? "Medium";

                if (!string.IsNullOrEmpty(_currentRecipe.ImageUrl) && System.IO.File.Exists(_currentRecipe.ImageUrl))
                    pictureBox1.ImageLocation = _currentRecipe.ImageUrl;

                PopulateIngredients(_currentRecipe.Ingredients ?? "");
                PopulateSteps(_currentRecipe.Steps ?? "");

                LoadInteractionData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void PopulateIngredients(string text)
        {
            guna2HtmlLabel12.Text =
                string.IsNullOrEmpty(text)
                ? "(Không có nguyên liệu)"
                : text.Replace("\n", "<br>");
        }

        private void PopulateSteps(string text)
        {
            guna2HtmlLabel15.Text =
                string.IsNullOrEmpty(text)
                ? "(Chưa có bước làm)"
                : text.Replace("\n", "<br><br>");
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
                var ratings = _interactionService.GetRatingsForRecipe(_recipeId);

                flpComments.Controls.Clear();

                // Tiêu đề
                var titleLabel = new Label
                {
                    Text = $"Đánh giá & Bình luận ({ratings.Count})",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 10),
                    ForeColor = Color.FromArgb(64, 64, 64)
                };
                flpComments.Controls.Add(titleLabel);

                if (!ratings.Any())
                {
                    var emptyLabel = new Label
                    {
                        Text = "Chưa có đánh giá nào. Hãy là người đầu tiên đánh giá!",
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 9),
                        Margin = new Padding(0, 10, 0, 20)
                    };
                    flpComments.Controls.Add(emptyLabel);
                    return;
                }

                foreach (var r in ratings.Take(_commentLimit))
                {
                    var item = new ucCommentItem(r);
                    item.Width = flpComments.ClientSize.Width - 25;
                    item.Margin = new Padding(0, 0, 0, 10);
                    flpComments.Controls.Add(item);
                }

                if (ratings.Count > _commentLimit)
                {
                    var btnMore = new Button
                    {
                        Text = "Xem thêm...",
                        BackColor = Color.LightBlue,
                        FlatStyle = FlatStyle.Flat,
                        Margin = new Padding(0, 10, 0, 0)
                    };
                    btnMore.Click += (s, e) =>
                    {
                        _commentLimit += 10;
                        LoadAllRatings();
                    };
                    flpComments.Controls.Add(btnMore);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải đánh giá: {ex.Message}");
            }
        }

        private void btnPostComment_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("btnPostComment_Click được gọi!");

                if (_currentUserId == 0)
                {
                    MessageBox.Show("Vui lòng đăng nhập để đánh giá!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int score = (int)rsMyRating.Value;
                string comment = guna2TextBox1.Text.Trim();

                Console.WriteLine($"Score: {score}, Comment: {comment}");

                // Validate
                if (score == 0)
                {
                    MessageBox.Show("Vui lòng chọn số sao đánh giá!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    MessageBox.Show("Vui lòng nhập nội dung bình luận!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newRating = new Rating
                {
                    UserId = _currentUserId,
                    RecipeId = _recipeId,
                    Score = score,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    Username = AuthManager.CurrentUser?.Username ?? "Ẩn danh"
                };

                Console.WriteLine("Đang gọi AddOrUpdateRating...");
                bool success = _interactionService.AddOrUpdateRating(newRating);

                if (success)
                {
                    MessageBox.Show("Đã gửi đánh giá thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reset form
                    guna2TextBox1.Clear();
                    rsMyRating.Value = 0;

                    // Reload comments
                    _commentLimit = 10;
                    LoadAllRatings();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi lưu đánh giá. Vui lòng thử lại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi hệ thống",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"EXCEPTION: {ex}");
            }
        }

        private void btnFavorite_Click(object sender, EventArgs e)
        {
            if (_currentUserId == 0)
            {
                MessageBox.Show("Cần đăng nhập!");
                return;
            }

            try
            {
                if (_isCurrentlyFavorite)
                    _interactionService.RemoveFavorite(_currentUserId, _recipeId);
                else
                    _interactionService.AddFavorite(_currentUserId, _recipeId);

                _isCurrentlyFavorite = !_isCurrentlyFavorite;
                UpdateFavoriteButtonVisuals();

                MessageBox.Show(_isCurrentlyFavorite ? "Đã thêm vào yêu thích!" : "Đã xóa khỏi yêu thích!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void UpdateFavoriteButtonVisuals()
        {
            guna2Button2.FillColor =
                _isCurrentlyFavorite
                ? Color.FromArgb(255, 128, 128)
                : Color.LightSalmon;

            guna2Button2.Text = _isCurrentlyFavorite ? "❤️ Đã thích" : "🤍 Thích";
        }

        private void btnAddToCollection_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tính năng đang phát triển.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Các sự kiện phụ khác
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