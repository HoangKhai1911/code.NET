using System;
using System.Drawing;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;

namespace WinCook.Controls
{
    public partial class ucRecipeCard : UserControl
    {
        private readonly Recipe _recipe;
        private readonly InteractionService _interactionService;

        public event EventHandler CardClicked;
        // Sự kiện khi người dùng chấm điểm xong (để form cha reload nếu cần)
        public event EventHandler RatingSubmitted;

        // Menu chọn sao
        private ContextMenuStrip _ratingMenu;

        public ucRecipeCard()
        {
            InitializeComponent();
            // Khởi tạo menu rỗng để tránh lỗi Designer
            _ratingMenu = new ContextMenuStrip();
        }

        public ucRecipeCard(Recipe recipe) : this()
        {
            _recipe = recipe;
            _interactionService = new InteractionService();

            // 1. Gán dữ liệu UI
            lblTitle.Text = recipe.Title;
            lblAuthor.Text = "by " + recipe.AuthorName;

            UpdateRatingDisplay();

            // Tải ảnh
            if (!string.IsNullOrEmpty(recipe.ImageUrl))
            {
                try { picImage.LoadAsync(recipe.ImageUrl); }
                catch { picImage.ImageLocation = "https://placehold.co/200x150/E8AA8B/ffffff?text=Error"; }
            }

            // Check yêu thích
            if (AuthManager.IsLoggedIn)
            {
                bool isFav = _interactionService.IsRecipeFavorited(AuthManager.CurrentUser.UserId, recipe.RecipeId);
                SetFavoriteIcon(isFav);
            }

            // 2. Tạo Menu Đánh giá (0.5 -> 5.0 sao)
            InitializeRatingMenu();

            // 3. Gán sự kiện Click
            // Click Tim -> Lưu yêu thích
            btnFavoriteSmall.Click += (s, e) => ToggleFavorite();

            // Click Số sao -> Hiện Menu chấm điểm
            lblRating.Cursor = Cursors.Hand;
            lblRating.Click += (s, e) =>
            {
                // Hiện menu ngay dưới label số sao
                _ratingMenu.Show(lblRating, new Point(0, lblRating.Height));
            };

            // Click chỗ khác -> Mở chi tiết
            this.Click += (s, e) => CardClicked?.Invoke(this, e);
            picImage.Click += (s, e) => CardClicked?.Invoke(this, e);
            lblTitle.Click += (s, e) => CardClicked?.Invoke(this, e);
            lblAuthor.Click += (s, e) => CardClicked?.Invoke(this, e);
        }

        private void InitializeRatingMenu()
        {
            _ratingMenu = new ContextMenuStrip();
            _ratingMenu.RenderMode = ToolStripRenderMode.System;

            // Thêm các mức điểm từ 5.0 xuống 1.0
            for (double i = 5.0; i >= 1.0; i -= 1.0)
            {
                double score = i; // Biến cục bộ cho lambda
                ToolStripMenuItem item = new ToolStripMenuItem($"{score} ★");
                item.Click += (s, e) => SubmitRating((int)score);

                // Tô màu vàng cho đẹp
                item.ForeColor = Color.Goldenrod;
                item.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                _ratingMenu.Items.Add(item);
            }
        }

        private void SubmitRating(int score)
        {
            if (!AuthManager.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để đánh giá.");
                return;
            }

            // Lưu đánh giá nhanh (không có comment)
            Rating newRating = new Rating
            {
                UserId = AuthManager.CurrentUser.UserId,
                RecipeId = _recipe.RecipeId,
                Score = score,
                Comment = "" // Đánh giá nhanh ko có comment
            };

            bool success = _interactionService.AddOrUpdateRating(newRating);
            if (success)
            {
                MessageBox.Show($"Đã chấm {score} sao cho món này!");
                // Kích hoạt sự kiện để form cha biết (có thể reload lại list để cập nhật điểm TB mới)
                RatingSubmitted?.Invoke(this, EventArgs.Empty);

                // Tạm thời cập nhật UI (số thực tế cần reload từ DB)
                lblRating.Text = "Đã chấm " + score + " ★";
            }
            else
            {
                MessageBox.Show("Lỗi khi chấm điểm.");
            }
        }

        private void UpdateRatingDisplay()
        {
            if (_recipe.AverageRating > 0)
            {
                lblRating.Text = $"{_recipe.AverageRating:F1} ★";
                lblRating.ForeColor = Color.Goldenrod;
            }
            else
            {
                lblRating.Text = "☆ Đánh giá"; // Chưa có đánh giá
                lblRating.ForeColor = Color.Gray;
            }
        }

        public int GetRecipeId() => _recipe?.RecipeId ?? 0;

        private void SetFavoriteIcon(bool isFav)
        {
            if (isFav)
            {
                btnFavoriteSmall.Text = "♥";
                btnFavoriteSmall.ForeColor = Color.Red;
            }
            else
            {
                btnFavoriteSmall.Text = "♡";
                btnFavoriteSmall.ForeColor = Color.LightGray;
            }
        }

        private void ToggleFavorite()
        {
            if (!AuthManager.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập.");
                return;
            }

            int userId = AuthManager.CurrentUser.UserId;
            int recipeId = _recipe.RecipeId;
            bool isFav = btnFavoriteSmall.Text == "♥";

            if (isFav)
                _interactionService.RemoveFavorite(userId, recipeId);
            else
                _interactionService.AddFavorite(userId, recipeId);

            SetFavoriteIcon(!isFav);
        }

        private void picImage_Click(object sender, EventArgs e)
        {

        }
    }
}