using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCook.Models
{
    /// <summary>
    /// Model cho bảng Ratings
    /// </summary>
    public class Rating
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bổ sung: Tên người đánh giá
        public string Username { get; set; }
    }
}
