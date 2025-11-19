//Models/Recipe.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCook.Models
{
    /// <summary>
    /// Model chính cho bảng Recipes
    /// </summary>
    public class Recipe
    {
        public int RecipeId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; } // Có thể là null
        public string Difficulty { get; set; }
        public string TimeNeeded { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // --- Thuộc tính bổ sung để hiển thị ---
        // Những thuộc tính này sẽ được điền bởi các hàm Service

        /// <summary>
        /// Tên tác giả (lấy từ bảng Users)
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Tên danh mục (lấy từ bảng Categories)
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Điểm đánh giá trung bình (từ View Recipe_Stats)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Tổng số lượt yêu thích (từ View Recipe_Stats)
        /// </summary>
        public int TotalFavorites { get; set; }

        /// <summary>
        /// Danh sách các nguyên liệu (lấy từ bảng Ingredients)
        /// </summary>
        public string Ingredients { get; set; }

        /// <summary>
        /// Danh sách các bước làm (lấy từ bảng Steps)
        /// </summary>
        public string Steps { get; set; }


        public Recipe()
        {
            // Khởi tạo 2 list để tránh lỗi null
            // (Xóa khởi tạo List<T> cũ)
            Ingredients = "";
            Steps = "";
        }
    }
}
