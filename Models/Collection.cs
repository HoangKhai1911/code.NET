using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCook.Models
{
    /// <summary>
    /// Model cho bảng Collections (Bộ sưu tập)
    /// </summary>
    public class Collection
    {
        public int CollectionId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bổ sung: Danh sách các công thức (dùng để hiển thị)
        public List<Recipe> Recipes { get; set; }
        public int RecipeCount { get; internal set; }

        public Collection()
        {
            Recipes = new List<Recipe>();
        }
    }
}