//Services/RecipeService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Xử lý tất cả logic nghiệp vụ cho Nhóm A (Quản lý Công thức).
    /// ĐÃ NÂNG CẤP: Sử dụng Stored Procedures và các cột NVARCHAR(MAX)
    /// (ingredients, steps) để khớp với CSDL của Ktuoi.
    /// </summary>
    public class RecipeService
    {
        private string connectionString = DBHelper.ConnectionString;

        #region === CREATE (Thêm mới) ===

        /// <summary>
        /// Thêm một công thức mới vào CSDL.
        /// Sử dụng Stored Procedure 'AddRecipe'.
        /// </summary>
        /// <param name="recipe">Đối tượng Recipe chứa đầy đủ thông tin</param>
        /// <returns>True nếu thành công, False nếu thất bại.</returns>
        public bool AddNewRecipe(Recipe recipe)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // --- Thêm Recipe chính (dùng SP AddRecipe) ---
                    using (SqlCommand cmd = new SqlCommand("AddRecipe", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@user_id", recipe.UserId);
                        cmd.Parameters.AddWithValue("@category_name", (object)recipe.CategoryName ?? DBNull.Value); // SP này xử lý category name
                        cmd.Parameters.AddWithValue("@title", recipe.Title);
                        cmd.Parameters.AddWithValue("@difficulty", (object)recipe.Difficulty ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@time_needed", (object)recipe.TimeNeeded ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@image_url", (object)recipe.ImageUrl ?? DBNull.Value);

                        // === NÂNG CẤP ===
                        // Gửi 2 chuỗi NVARCHAR(MAX) khớp với SP của bạn
                        cmd.Parameters.AddWithValue("@ingredients", (object)recipe.Ingredients ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@steps", (object)recipe.Steps ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm công thức (AddNewRecipe): " + ex.Message);
                return false;
            }
        }

        #endregion

        #region === READ (Đọc dữ liệu) ===

        /// <summary>
        /// Lấy tất cả công thức để hiển thị trên trang chủ (chưa bao gồm Ingredients/Steps).
        /// </summary>

        public List<Recipe> GetAllRecipes()
        {
            List<Recipe> recipes = new List<Recipe>();

            string query = @"
        SELECT 
            r.recipe_id,
            r.title,
            r.difficulty,
            r.time_needed,
            r.image_url,
            u.username AS AuthorName,
            c.name AS CategoryName
        FROM Recipes r
        LEFT JOIN Users u ON r.user_id = u.user_id
        LEFT JOIN Categories c ON r.category_id = c.category_id
        ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recipes.Add(MapRecipeFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi SQL GetAllRecipes: " + ex.Message);
            }

            // DEBUG: show số lượng recipe đọc được
            //MessageBox.Show("GetAllRecipes đọc được: " + recipes.Count + " món");

            return recipes;
        }


        /// <summary>
        /// Lấy chi tiết đầy đủ của 1 công thức (bao gồm Ingredients và Steps).
        /// Sử dụng SP 'GetRecipeDetail'.
        /// </summary>
        public Recipe GetRecipeDetails(int recipeId)
        {
            Recipe recipe = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // --- Bước 1: Lấy thông tin chính (dùng SP) ---
                    using (SqlCommand cmd = new SqlCommand("GetRecipeDetail", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recipe_id", recipeId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // === NÂNG CẤP ===
                                // SP 'GetRecipeDetail' của bạn đã trả về 
                                // 'ingredients' và 'steps', bây giờ chúng ta đọc nó
                                recipe = new Recipe
                                {
                                    RecipeId = Convert.ToInt32(reader["recipe_id"]),
                                    Title = reader["title"].ToString(),
                                    Difficulty = reader["difficulty"].ToString(),
                                    TimeNeeded = reader["time_needed"].ToString(),
                                    ImageUrl = reader["image_url"].ToString(),
                                    AuthorName = reader["author"].ToString(),
                                    CategoryName = reader["category"].ToString(),
                                    AverageRating = Convert.ToDouble(reader["avg_rating"]),

                                    // Đọc 2 cột NVARCHAR(MAX)
                                    Ingredients = reader["ingredients"].ToString(),
                                    Steps = reader["steps"].ToString()
                                };
                            }
                        } // Reader tự đóng
                    }

                    // (Không cần Bước 2 & 3 - Lấy Ingredients/Steps riêng nữa)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy chi tiết công thức (ID: {recipeId}): {ex.Message}");
            }
            return recipe;
        }

        /// <summary>
        /// (Chức năng Nhóm A - Fuc) Tìm kiếm công thức theo Tiêu đề.
        /// </summary>

        public List<Recipe> SearchRecipesByTitle(string searchTerm)
        {
            List<Recipe> recipes = new List<Recipe>();

            string query = @"
        SELECT 
            r.recipe_id,
            r.title,
            r.difficulty,
            r.time_needed,
            r.image_url,
            u.username AS AuthorName,
            c.name AS CategoryName
        FROM Recipes r
        JOIN Users u ON r.user_id = u.user_id
        LEFT JOIN Categories c ON r.category_id = c.category_id
        WHERE r.title LIKE @searchTerm
        ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recipes.Add(MapRecipeFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tìm kiếm công thức: " + ex.Message);
            }

            return recipes;
        }


        /// <summary>
        /// (Chức năng Nhóm A - Fuc) Lọc công thức theo Category ID.
        /// </summary>
        public List<Recipe> FilterRecipesByCategory(int categoryId)
        {
            List<Recipe> recipes = new List<Recipe>();
            // Dùng View Recipe_Stats
            string query = @"
                SELECT 
                    r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
                    u.username AS AuthorName, 
                    c.name AS CategoryName
                    
                FROM Recipes r
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
               
                WHERE r.category_id = @categoryId
                ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@categoryId", categoryId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recipes.Add(MapRecipeFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lọc công thức: " + ex.Message);
            }
            return recipes;
        }

        /// <summary>
        /// (HÀM MỚI - CHO FRMMYRECIPES) Lấy công thức theo ID Tác giả
        /// </summary>
        public List<Recipe> GetRecipesByAuthor(int authorUserId)
        {
            List<Recipe> recipes = new List<Recipe>();
            // Dùng View Recipe_Stats
            string query = @"
                SELECT 
                    r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
                    u.username AS AuthorName, 
                    c.name AS CategoryName
                    
                FROM Recipes r
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
               
                WHERE r.user_id = @authorUserId
                ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@authorUserId", authorUserId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recipes.Add(MapRecipeFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy công thức theo tác giả: " + ex.Message);
            }
            return recipes;
        }

        /// <summary>
        /// (Hàm mới) Lấy tất cả danh mục (Categories) cho ComboBox
        /// </summary>
        public List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            string query = "SELECT category_id, name FROM Categories ORDER BY name";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryId = (int)reader["category_id"],
                                    Name = (string)reader["name"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy Categories: " + ex.Message);
            }
            return categories;
        }


        #endregion

        #region === UPDATE (Cập nhật) ===

        /// <summary>
        /// Cập nhật thông tin công thức.
        /// Sử dụng SP 'UpdateRecipe'.
        /// </summary>
        /// <param name="recipe">Đối tượng Recipe đã được cập nhật thông tin</param>
        /// <returns>True nếu thành công</returns>
        public bool UpdateRecipe(Recipe recipe)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // --- Cập nhật (dùng SP UpdateRecipe) ---
                    // SP 'UpdateRecipe' của bạn đã bao gồm logic
                    // tự tìm Category ID, rất tốt!

                    // === NÂNG CẤP ===
                    // (Code trong 'frmAddRecipie' của bạn dùng 1 câu SQL dài
                    // thay vì SP 'UpdateRecipe'. Ở đây tôi sẽ dùng
                    // SP 'UpdateRecipe' có sẵn trong CSDL của bạn
                    // để code C# sạch hơn)

                    using (SqlCommand cmd = new SqlCommand("UpdateRecipe", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recipe_id", recipe.RecipeId);

                        // Lấy Category ID (vì SP UpdateRecipe cần ID)
                        int? categoryId = GetOrCreateCategoryId(recipe.CategoryName, connection, null);
                        cmd.Parameters.AddWithValue("@category_id", (object)categoryId ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@title", recipe.Title);
                        cmd.Parameters.AddWithValue("@difficulty", recipe.Difficulty);
                        cmd.Parameters.AddWithValue("@time_needed", recipe.TimeNeeded);
                        cmd.Parameters.AddWithValue("@image_url", (object)recipe.ImageUrl ?? DBNull.Value);

                        // Gửi 2 chuỗi NVARCHAR(MAX)
                        cmd.Parameters.AddWithValue("@ingredients", (object)recipe.Ingredients ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@steps", (object)recipe.Steps ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật công thức: " + ex.Message);
                return false;
            }
        }


        #endregion

        #region === DELETE (Xóa) ===

        /// <summary>
        /// Xóa một công thức. Sử dụng SP 'DeleteRecipe'.
        /// </summary>
        // Trong WinCook/Services/RecipeService.cs

        public bool DeleteRecipe(int recipeId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteRecipe", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recipe_id", recipeId);

                        // THỰC THI LỆNH
                        cmd.ExecuteNonQuery();

                        // --- SỬA QUAN TRỌNG TẠI ĐÂY ---
                        // Vì Procedure có 'SET NOCOUNT ON', nó sẽ trả về -1.
                        // Đừng kiểm tra > 0 nữa. Chỉ cần không bị lỗi Exception là thành công.
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiện lỗi chi tiết để debug nếu có vấn đề khác
                System.Windows.Forms.MessageBox.Show("Lỗi Service Xóa: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region === Tiện ích (Helpers) ===

        /// <summary>
        /// Hàm nội bộ: Lấy ID của Category, nếu chưa có thì tạo mới.
        /// (Dùng cho hàm UpdateRecipe)
        /// </summary>
        private int? GetOrCreateCategoryId(string categoryName, SqlConnection connection, SqlTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            int categoryId;

            // 1. Thử tìm
            string queryFind = "SELECT category_id FROM Categories WHERE name = @name";
            using (SqlCommand cmdFind = new SqlCommand(queryFind, connection, transaction))
            {
                cmdFind.Parameters.AddWithValue("@name", categoryName);
                object result = cmdFind.ExecuteScalar();
                if (result != null)
                {
                    return (int)result;
                }
            }

            // 2. Không tìm thấy -> Tạo mới
            string queryCreate = "INSERT INTO Categories (name) OUTPUT INSERTED.category_id VALUES (@name)";
            using (SqlCommand cmdCreate = new SqlCommand(queryCreate, connection, transaction))
            {
                cmdCreate.Parameters.AddWithValue("@name", categoryName);
                categoryId = (int)cmdCreate.ExecuteScalar();
                return categoryId;
            }
        }

        /// <summary>
        /// Hàm nội bộ: Đọc dữ liệu từ SqlDataReader và map sang đối tượng Recipe (cho danh sách)
        /// </summary>
        private Recipe MapRecipeFromReader(SqlDataReader reader)
        {
            return new Recipe
            {
                RecipeId = Convert.ToInt32(reader["recipe_id"]),
                Title = reader["title"]?.ToString(),
                Difficulty = reader["difficulty"]?.ToString(),
                TimeNeeded = reader["time_needed"]?.ToString(),
                ImageUrl = reader["image_url"]?.ToString(),
                AuthorName = reader["AuthorName"]?.ToString(),
                CategoryName = reader["CategoryName"]?.ToString(),
                TotalFavorites = 0,
                AverageRating = 0
            };
        }


        #endregion
    }
}