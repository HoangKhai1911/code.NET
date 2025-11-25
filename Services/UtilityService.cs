using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Xử lý các nghiệp vụ cho Nhóm C (Tiện ích) và Nhóm D (Thống kê).
    /// </summary>
    public class UtilityService
    {
        private string connectionString = DBHelper.ConnectionString;

        #region === Nhóm C: Tiện ích Nấu ăn (Adjust Servings) ===

        public List<Ingredient> AdjustServings(List<Ingredient> originalIngredients, double adjustmentFactor)
        {
            if (originalIngredients == null || adjustmentFactor == 1.0)
                return originalIngredients;

            List<Ingredient> adjustedIngredients = new List<Ingredient>();

            foreach (var original in originalIngredients)
            {
                adjustedIngredients.Add(new Ingredient
                {
                    Name = original.Name,
                    Quantity = AdjustQuantityString(original.Quantity, adjustmentFactor)
                });
            }

            return adjustedIngredients;
        }

        private string AdjustQuantityString(string quantityStr, double factor)
        {
            if (string.IsNullOrWhiteSpace(quantityStr)) return quantityStr;

            try
            {
                Match match = Regex.Match(quantityStr, @"^[\d\.,]+");
                if (match.Success)
                {
                    string numberPart = match.Value;
                    string restPart = quantityStr.Substring(numberPart.Length).TrimStart();

                    if (double.TryParse(numberPart.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double originalValue))
                    {
                        double newValue = Math.Round(originalValue * factor, 2);
                        return $"{newValue} {restPart}";
                    }
                }
                return quantityStr;
            }
            catch
            {
                return quantityStr;
            }
        }

        #endregion

        #region === Nhóm D: Thống kê Hệ thống (Global - Cho trang chủ) ===

        public BasicStatistics GetBasicStatistics()
        {
            var stats = new BasicStatistics();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Tổng số công thức
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Recipes", connection))
                    {
                        stats.TotalRecipes = (int)cmd.ExecuteScalar();
                    }

                    // 2. Tổng số người dùng
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", connection))
                    {
                        stats.TotalUsers = (int)cmd.ExecuteScalar();
                    }

                    // 3. Top 5 món yêu thích nhất toàn hệ thống
                    string queryTop5 = @"
                        SELECT TOP 5 r.Title, COUNT(f.FavoriteId) as total_favorites 
                        FROM Recipes r
                        LEFT JOIN Favorites f ON r.RecipeId = f.RecipeId
                        GROUP BY r.RecipeId, r.Title
                        HAVING COUNT(f.FavoriteId) > 0
                        ORDER BY total_favorites DESC";

                    using (SqlCommand cmd = new SqlCommand(queryTop5, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.Top5Favorites.Add(new RecipeStat
                                {
                                    Title = reader["Title"].ToString(),
                                    TotalFavorites = Convert.ToInt32(reader["total_favorites"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetBasicStatistics: " + ex.Message);
            }

            return stats;
        }

        #endregion

        #region === [MỚI] Nhóm D: Thống kê Cá nhân (Cho trang Profile) ===

        public UserStatistics GetUserStatistics(int userId)
        {
            var stats = new UserStatistics();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Tổng công thức do User này tạo
                    string sqlCountRecipes = "SELECT COUNT(*) FROM Recipes WHERE AuthorId = @uid";
                    using (SqlCommand cmd = new SqlCommand(sqlCountRecipes, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        stats.TotalRecipes = (int)cmd.ExecuteScalar();
                    }

                    // 2. Tổng lượt yêu thích User nhận được
                    string sqlCountFavs = @"
                        SELECT COUNT(f.FavoriteId) 
                        FROM Favorites f 
                        JOIN Recipes r ON f.RecipeId = r.RecipeId 
                        WHERE r.AuthorId = @uid";
                    using (SqlCommand cmd = new SqlCommand(sqlCountFavs, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        object result = cmd.ExecuteScalar();
                        stats.TotalFavoritesReceived = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }

                    // 3. Top 5 món ăn CỦA USER được yêu thích nhất (Đã đổi tên class thành StatChartData)
                    string sqlTopMyRecipes = @"
                        SELECT TOP 5 r.Title, COUNT(f.FavoriteId) as FavCount
                        FROM Recipes r
                        LEFT JOIN Favorites f ON r.RecipeId = f.RecipeId
                        WHERE r.AuthorId = @uid
                        GROUP BY r.RecipeId, r.Title
                        HAVING COUNT(f.FavoriteId) > 0
                        ORDER BY FavCount DESC";

                    using (SqlCommand cmd = new SqlCommand(sqlTopMyRecipes, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.TopRecipes.Add(new StatChartData
                                {
                                    Label = reader["Title"].ToString(),
                                    Value = Convert.ToInt32(reader["FavCount"])
                                });
                            }
                        }
                    }

                    // 4. Phân bố công thức theo Danh mục
                    string sqlCatStats = @"
                        SELECT c.CategoryName, COUNT(r.RecipeId) as RecipeCount
                        FROM Recipes r
                        JOIN Categories c ON r.CategoryId = c.CategoryId
                        WHERE r.AuthorId = @uid
                        GROUP BY c.CategoryName";

                    using (SqlCommand cmd = new SqlCommand(sqlCatStats, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.RecipesByCategory.Add(new StatChartData
                                {
                                    Label = reader["CategoryName"].ToString(),
                                    Value = Convert.ToInt32(reader["RecipeCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetUserStatistics: " + ex.Message);
            }

            return stats;
        }

        #endregion
    }

    #region === Các Model DTO (Data Transfer Object) ===

    // 1. Thống kê toàn hệ thống
    public class BasicStatistics
    {
        public int TotalRecipes { get; set; }
        public int TotalUsers { get; set; }
        public List<RecipeStat> Top5Favorites { get; set; } = new List<RecipeStat>();
    }

    public class RecipeStat
    {
        public string Title { get; set; }
        public int TotalFavorites { get; set; }
    }

    // 2. Thống kê cá nhân User
    public class UserStatistics
    {
        public int TotalRecipes { get; set; }
        public int TotalFavoritesReceived { get; set; }

        // Đã đổi tên List<ChartData> -> List<StatChartData>
        public List<StatChartData> TopRecipes { get; set; } = new List<StatChartData>();
        public List<StatChartData> RecipesByCategory { get; set; } = new List<StatChartData>();
    }

    // 3. Model chung cho Biểu đồ (ĐÃ ĐỔI TÊN ĐỂ TRÁNH TRÙNG LẶP)
    public class StatChartData
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }

    #endregion
}