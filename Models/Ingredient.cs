using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WinCook.Models

{
    /// <summary>
    /// Model cho bảng Ingredients
    /// </summary>
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
    }
}
