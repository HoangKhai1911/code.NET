using System;
using System.Drawing;
using System.Windows.Forms;
using WinCook.Models;

namespace WinCook.Controls
{
    public partial class ucMyRecipeCard : UserControl
    {
        public Recipe CurrentRecipe { get; private set; }

        // Khai báo sự kiện
        public event EventHandler EditClicked;
        public event EventHandler DeleteClicked;
        public event EventHandler CardClicked;

        public ucMyRecipeCard()
        {
            InitializeComponent();
        }

        public ucMyRecipeCard(Recipe recipe) : this()
        {
            CurrentRecipe = recipe;

            // 1. Gán dữ liệu UI (Kiểm tra null để tránh crash)
            if (lblTitle != null) lblTitle.Text = recipe.Title;
            if (lblInfo != null) lblInfo.Text = $"{recipe.TimeNeeded} | {recipe.Difficulty}";

            // Rating
            if (lblRating != null)
            {
                lblRating.Text = recipe.AverageRating > 0 ? $"{recipe.AverageRating:F1} ★" : "Chưa đánh giá";
                lblRating.ForeColor = Color.Goldenrod;
            }

            // Ảnh
            if (picImage != null && !string.IsNullOrEmpty(recipe.ImageUrl))
            {
                try { picImage.LoadAsync(recipe.ImageUrl); }
                catch { picImage.Image = null; }
            }

            // 2. Gán sự kiện Click cho các Nút
            // (Quan trọng: Gán trực tiếp bằng toán tử +=)

            if (btnEdit != null)
                btnEdit.Click += (s, e) => EditClicked?.Invoke(this, EventArgs.Empty);

            if (btnDelete != null)
                btnDelete.Click += (s, e) => DeleteClicked?.Invoke(this, EventArgs.Empty);

            // Click vào vùng khác để xem chi tiết
            this.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
            if (picImage != null) picImage.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
            if (lblTitle != null) lblTitle.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
        }

        public int GetRecipeId()
        {
            return CurrentRecipe != null ? CurrentRecipe.RecipeId : 0;
        }
    }
}