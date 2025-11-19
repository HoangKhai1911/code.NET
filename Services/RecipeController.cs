// Services/RecipeController.cs
using System;
using System.Collections.Generic;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Lớp trung gian giữa Forms và các Service:
    /// - Đọc danh sách Recipe
    /// - Đọc Favorites
    /// - Lấy chi tiết Recipe
    /// </summary>
    public class RecipeController
    {
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService;

        public RecipeController()
        {
            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
        }

        // === DÙNG CHO TRANG Recipes ===
        public List<Recipe> GetAllRecipes()
        {
            return _recipeService.GetAllRecipes();
        }

        // === DÙNG CHO TRANG Favorites ===
        public List<Recipe> GetFavoriteRecipes(int userId)
        {
            return _interactionService.GetFavoriteRecipes(userId);
        }

        // === DÙNG CHO RECIPE DETAILS (từ mọi nơi) ===
        public Recipe GetRecipeDetails(int recipeId)
        {
            return _recipeService.GetRecipeDetails(recipeId);
        }
    }
}
