using System;

namespace WinCook.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public string Difficulty { get; set; }
        public string TimeNeeded { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
        public int TotalFavorites { get; set; }

        // Dùng string để khớp với CSDL và tránh lỗi convert
        // Đây chính là cấu trúc "cũ" mà bạn muốn
        public string Ingredients { get; set; }
        public string Steps { get; set; }

        public Recipe()
        {
            Ingredients = "";
            Steps = "";
        }
    }
}