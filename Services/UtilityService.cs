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

        #region === Nhóm C: Tiện ích Nấu ăn ===

        /// <summary>
        /// (Chức năng Nhóm C - Fuc)
        /// Tính toán lại số lượng nguyên liệu dựa trên hệ số điều chỉnh.
        /// </summary>
        /// <param name="originalIngredients">Danh sách nguyên liệu gốc.</param>
        /// <param name="adjustmentFactor">Hệ số điều chỉnh (ví dụ: 2.0 cho x2, 0.5 cho 1/2).</param>
        /// <returns>Một danh sách Ingredients mới với số lượng đã được điều chỉnh.</returns>
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

        /// <summary>
        /// Hàm nội bộ: Cố gắng tìm số ở đầu chuỗi Quantity và nhân nó với hệ số.
        /// Ví dụ: "100g bột" * 2.0 => "200g bột"
        /// Ví dụ: "1/2 cup" => không thay đổi (quá phức tạp để xử lý)
        /// Ví dụ: "2 quả trứng" * 1.5 => "3 quả trứng"
        /// </summary>
        private string AdjustQuantityString(string quantityStr, double factor)
        {
            if (string.IsNullOrWhiteSpace(quantityStr))
                return quantityStr;

            try
            {
                // Sử dụng Regex để tìm số đầu tiên (bao gồm cả số thập phân)
                // Regex này tìm: một số (có thể có dấu phẩy/chấm) ở đầu chuỗi
                Match match = Regex.Match(quantityStr, @"^[\d\.,]+");

                if (match.Success)
                {
                    // Lấy giá trị số (ví dụ: "100" từ "100g")
                    string numberPart = match.Value;
                    // Lấy phần còn lại (ví dụ: "g bột" từ "100g bột")
                    string restPart = quantityStr.Substring(numberPart.Length).TrimStart();

                    // Chuyển đổi số (xử lý cả ',' và '.' cho an toàn)
                    if (double.TryParse(numberPart.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double originalValue))
                    {
                        // Tính toán giá trị mới
                        double newValue = originalValue * factor;

                        // Làm tròn (ví dụ: 0.5 * 1.5 = 0.75)
                        newValue = Math.Round(newValue, 2);

                        // Ghép lại chuỗi
                        return $"{newValue} {restPart}";
                    }
                }

                // Nếu không tìm thấy số hoặc không thể parse, trả về chuỗi gốc
                return quantityStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi điều chỉnh chuỗi Quantity: " + ex.Message);
                return quantityStr; // Trả về gốc nếu có lỗi
            }
        }

        #endregion

        #region === Nhóm D: Thống kê ===

        /// <summary>
        /// (Chức năng Nhóm D - Tổng tài audio)
        /// Lấy các thống kê cơ bản cho Báo cáo.
        /// </summary>
        public BasicStatistics GetBasicStatistics()
        {
            var stats = new BasicStatistics();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Đếm tổng số công thức
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Recipes", connection))
                    {
                        stats.TotalRecipes = (int)cmd.ExecuteScalar();
                    }

                    // 2. Đếm tổng số người dùng
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", connection))
                    {
                        stats.TotalUsers = (int)cmd.ExecuteScalar();
                    }

                    // 3. Lấy Top 5 món yêu thích nhất (Sử dụng View Recipe_Stats)
                    string queryTop5 = @"
                        SELECT TOP 5 title, total_favorites 
                        FROM Recipe_Stats 
                        WHERE total_favorites > 0
                        ORDER BY total_favorites DESC";
                    using (SqlCommand cmd = new SqlCommand(queryTop5, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stats.Top5Favorites.Add(new RecipeStat
                                {
                                    Title = (string)reader["title"],
                                    TotalFavorites = (int)reader["total_favorites"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy Thống kê: " + ex.Message);
                // Trả về stats (có thể rỗng)
            }

            return stats;
        }

        #endregion
    }

    #region === Lớp Model cho Thống kê (Nhóm D) ===

    /// <summary>
    /// Model DTO (Data Transfer Object) để chứa kết quả thống kê
    /// </summary>
    public class BasicStatistics
    {
        public int TotalRecipes { get; set; }
        public int TotalUsers { get; set; }
        public List<RecipeStat> Top5Favorites { get; set; }

        public BasicStatistics()
        {
            Top5Favorites = new List<RecipeStat>();
        }
    }

    /// <summary>
    /// Model DTO phụ trợ cho BasicStatistics
    /// </summary>
    public class RecipeStat
    {
        public string Title { get; set; }
        public int TotalFavorites { get; set; }
    }

    #endregion
}