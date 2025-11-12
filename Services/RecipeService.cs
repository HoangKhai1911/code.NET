using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Xử lý tất cả logic nghiệp vụ cho Nhóm A (Quản lý Công thức).
    /// Nhiệm vụ của: Ktuoi, Fuc, Khải.
    /// Đã cập nhật để dùng Stored Procedures và CSDL chuẩn hóa (Ingredients/Steps).
    /// </summary>
    public class RecipeService
    {
        private string connectionString = DBHelper.ConnectionString;

        #region === CREATE (Thêm mới) ===

        /// <summary>
        /// Thêm một công thức mới (bao gồm cả Ingredients và Steps) vào CSDL.
        /// Sử dụng Stored Procedure 'AddRecipe'.
        /// </summary>
        /// <param name="recipe">Đối tượng Recipe chứa đầy đủ thông tin</param>
        /// <returns>True nếu thành công, False nếu thất bại.</returns>
        public bool AddNewRecipe(Recipe recipe)
        {
            // Yêu cầu: Thông tin Recipe phải có UserId (từ AuthManager)
            // và danh sách Ingredients, Steps.

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Bắt đầu một Transaction để đảm bảo toàn vẹn dữ liệu
                // Hoặc Thêm tất cả, hoặc không thêm gì cả.
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    int newRecipeId;

                    // --- Bước 1: Thêm Recipe chính (dùng SP AddRecipe) ---
                    using (SqlCommand cmd = new SqlCommand("AddRecipe", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@user_id", recipe.UserId);
                        cmd.Parameters.AddWithValue("@category_name", (object)recipe.CategoryName ?? DBNull.Value); // SP này xử lý category name
                        cmd.Parameters.AddWithValue("@title", recipe.Title);
                        cmd.Parameters.AddWithValue("@difficulty", (object)recipe.Difficulty ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@time_needed", (object)recipe.TimeNeeded ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@image_url", (object)recipe.ImageUrl ?? DBNull.Value);

                        // SP 'AddRecipe' của bạn trả về ID của Recipe mới
                        newRecipeId = Convert.ToInt32(cmd.ExecuteScalar());

                        if (newRecipeId <= 0)
                        {
                            throw new Exception("Không thể tạo Recipe ID mới.");
                        }
                    }

                    // --- Bước 2: Thêm Ingredients (dùng ID mới lấy được) ---
                    if (recipe.Ingredients != null && recipe.Ingredients.Count > 0)
                    {
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            string query = "INSERT INTO Ingredients (recipe_id, name, quantity) VALUES (@recipe_id, @name, @quantity)";
                            using (SqlCommand cmdIng = new SqlCommand(query, connection, transaction))
                            {
                                cmdIng.Parameters.AddWithValue("@recipe_id", newRecipeId);
                                cmdIng.Parameters.AddWithValue("@name", ingredient.Name);
                                cmdIng.Parameters.AddWithValue("@quantity", (object)ingredient.Quantity ?? DBNull.Value);
                                cmdIng.ExecuteNonQuery();
                            }
                        }
                    }

                    // --- Bước 3: Thêm Steps (dùng ID mới lấy được) ---
                    if (recipe.Steps != null && recipe.Steps.Count > 0)
                    {
                        foreach (var step in recipe.Steps)
                        {
                            string query = "INSERT INTO Steps (recipe_id, step_number, instruction) VALUES (@recipe_id, @step_number, @instruction)";
                            using (SqlCommand cmdStep = new SqlCommand(query, connection, transaction))
                            {
                                cmdStep.Parameters.AddWithValue("@recipe_id", newRecipeId);
                                cmdStep.Parameters.AddWithValue("@step_number", step.StepNumber);
                                cmdStep.Parameters.AddWithValue("@instruction", step.Instruction);
                                cmdStep.ExecuteNonQuery();
                            }
                        }
                    }

                    // Nếu tất cả thành công, Commit Transaction
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi, Rollback Transaction
                    transaction.Rollback();
                    Console.WriteLine("Lỗi khi thêm công thức (AddNewRecipe): " + ex.Message);
                    return false;
                }
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
            // Sử dụng View Recipe_Stats để lấy thông tin tổng hợp
            string query = @"
                SELECT 
                    r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
                    u.username AS AuthorName, 
                    c.name AS CategoryName,
                    ISNULL(rs.total_favorites, 0) AS TotalFavorites,
                    ISNULL(rs.avg_rating, 0) AS AverageRating
                FROM Recipes r
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
                LEFT JOIN Recipe_Stats rs ON r.recipe_id = rs.recipe_id
                ORDER BY r.created_at DESC"; // Sắp xếp theo món mới nhất

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
                                recipes.Add(MapRecipeFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy tất cả công thức: " + ex.Message);
            }
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
                                // SP này không trả về Ingredients/Steps/Favorites, ta phải tự map
                                recipe = new Recipe
                                {
                                    RecipeId = Convert.ToInt32(reader["recipe_id"]),
                                    Title = reader["title"].ToString(),
                                    Difficulty = reader["difficulty"].ToString(),
                                    TimeNeeded = reader["time_needed"].ToString(),
                                    ImageUrl = reader["image_url"].ToString(),
                                    AuthorName = reader["author"].ToString(),
                                    CategoryName = reader["category"].ToString(),
                                    AverageRating = Convert.ToDouble(reader["avg_rating"])
                                    // Các trường Ingredients/Steps sẽ được load ở Bước 2 & 3
                                };
                            }
                        } // Reader tự đóng
                    }

                    if (recipe == null) return null; // Không tìm thấy Recipe

                    // --- Bước 2: Lấy Ingredients ---
                    string queryIng = "SELECT ingredient_id, name, quantity FROM Ingredients WHERE recipe_id = @recipe_id";
                    using (SqlCommand cmdIng = new SqlCommand(queryIng, connection))
                    {
                        cmdIng.Parameters.AddWithValue("@recipe_id", recipeId);
                        using (SqlDataReader readerIng = cmdIng.ExecuteReader())
                        {
                            while (readerIng.Read())
                            {
                                recipe.Ingredients.Add(new Ingredient
                                {
                                    IngredientId = (int)readerIng["ingredient_id"],
                                    Name = (string)readerIng["name"],
                                    Quantity = readerIng["quantity"].ToString(),
                                    RecipeId = recipeId
                                });
                            }
                        }
                    }

                    // --- Bước 3: Lấy Steps ---
                    string queryStep = "SELECT step_id, step_number, instruction FROM Steps WHERE recipe_id = @recipe_id ORDER BY step_number ASC";
                    using (SqlCommand cmdStep = new SqlCommand(queryStep, connection))
                    {
                        cmdStep.Parameters.AddWithValue("@recipe_id", recipeId);
                        using (SqlDataReader readerStep = cmdStep.ExecuteReader())
                        {
                            while (readerStep.Read())
                            {
                                recipe.Steps.Add(new Step
                                {
                                    StepId = (int)readerStep["step_id"],
                                    StepNumber = (int)readerStep["step_number"],
                                    Instruction = (string)readerStep["instruction"],
                                    RecipeId = recipeId
                                });
                            }
                        }
                    }
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
            // Dùng View Recipe_Stats
            string query = @"
                SELECT 
                    r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
                    u.username AS AuthorName, 
                    c.name AS CategoryName,
                    ISNULL(rs.total_favorites, 0) AS TotalFavorites,
                    ISNULL(rs.avg_rating, 0) AS AverageRating
                FROM Recipes r
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
                LEFT JOIN Recipe_Stats rs ON r.recipe_id = rs.recipe_id
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
                    c.name AS CategoryName,
                    ISNULL(rs.total_favorites, 0) AS TotalFavorites,
                    ISNULL(rs.avg_rating, 0) AS AverageRating
                FROM Recipes r
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
                LEFT JOIN Recipe_Stats rs ON r.recipe_id = rs.recipe_id
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

        #endregion

        #region === UPDATE (Cập nhật) ===

        /// <summary>
        /// Cập nhật thông tin công thức (bao gồm Ingredients và Steps).
        /// Sử dụng SP 'UpdateRecipe'.
        /// </summary>
        /// <param name="recipe">Đối tượng Recipe đã được cập nhật thông tin</param>
        /// <returns>True nếu thành công</returns>
        public bool UpdateRecipe(Recipe recipe)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // --- Bước 1: Cập nhật thông tin chính (dùng SP UpdateRecipe) ---
                    // Lưu ý: SP 'UpdateRecipe' của bạn không tự xử lý category_name,
                    // nên chúng ta cần lấy category_id trước.
                    int? categoryId = GetOrCreateCategoryId(recipe.CategoryName, connection, transaction);

                    using (SqlCommand cmd = new SqlCommand("UpdateRecipe", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@recipe_id", recipe.RecipeId);
                        cmd.Parameters.AddWithValue("@category_id", (object)categoryId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@title", recipe.Title);
                        cmd.Parameters.AddWithValue("@difficulty", recipe.Difficulty);
                        cmd.Parameters.AddWithValue("@time_needed", recipe.TimeNeeded);
                        cmd.Parameters.AddWithValue("@image_url", (object)recipe.ImageUrl ?? DBNull.Value);

                        // SP 'UpdateRecipe' của bạn thiếu 2 trường ingredients và steps
                        // (Điều này là ĐÚNG vì chúng ta đã chuẩn hóa CSDL)
                        // Chúng ta sẽ bỏ 2 tham số đó khỏi SP call.

                        cmd.ExecuteNonQuery();
                    }

                    // --- Bước 2: Xóa Ingredients và Steps cũ ---
                    // (Đây là cách đơn giản nhất để cập nhật)
                    using (SqlCommand cmdDelete = new SqlCommand(
                        "DELETE FROM Ingredients WHERE recipe_id = @recipe_id; DELETE FROM Steps WHERE recipe_id = @recipe_id;",
                        connection, transaction))
                    {
                        cmdDelete.Parameters.AddWithValue("@recipe_id", recipe.RecipeId);
                        cmdDelete.ExecuteNonQuery();
                    }

                    // --- Bước 3 & 4: Thêm lại Ingredients và Steps mới (giống hệt AddNewRecipe) ---
                    if (recipe.Ingredients != null && recipe.Ingredients.Count > 0)
                    {
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            string query = "INSERT INTO Ingredients (recipe_id, name, quantity) VALUES (@recipe_id, @name, @quantity)";
                            using (SqlCommand cmdIng = new SqlCommand(query, connection, transaction))
                            {
                                cmdIng.Parameters.AddWithValue("@recipe_id", recipe.RecipeId);
                                cmdIng.Parameters.AddWithValue("@name", ingredient.Name);
                                cmdIng.Parameters.AddWithValue("@quantity", (object)ingredient.Quantity ?? DBNull.Value);
                                cmdIng.ExecuteNonQuery();
                            }
                        }
                    }

                    if (recipe.Steps != null && recipe.Steps.Count > 0)
                    {
                        foreach (var step in recipe.Steps)
                        {
                            string query = "INSERT INTO Steps (recipe_id, step_number, instruction) VALUES (@recipe_id, @step_number, @instruction)";
                            using (SqlCommand cmdStep = new SqlCommand(query, connection, transaction))
                            {
                                cmdStep.Parameters.AddWithValue("@recipe_id", recipe.RecipeId);
                                cmdStep.Parameters.AddWithValue("@step_number", step.StepNumber);
                                cmdStep.Parameters.AddWithValue("@instruction", step.Instruction);
                                cmdStep.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Lỗi khi cập nhật công thức: " + ex.Message);
                    return false;
                }
            }
        }


        #endregion

        #region === DELETE (Xóa) ===

        /// <summary>
        /// Xóa một công thức. Sử dụng SP 'DeleteRecipe'.
        /// </summary>
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

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                        // Do CSDL đã cài đặt ON DELETE CASCADE,
                        // Ingredients, Steps, Favorites, Ratings, Notes...
                        // sẽ tự động bị xóa theo. Rất hiệu quả!
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xóa công thức: " + ex.Message);
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
                Title = reader["title"].ToString(),
                Difficulty = reader["difficulty"].ToString(),
                TimeNeeded = reader["time_needed"].ToString(),
                ImageUrl = reader["image_url"].ToString(),
                AuthorName = reader["AuthorName"].ToString(),
                CategoryName = reader["CategoryName"].ToString(),
                TotalFavorites = Convert.ToInt32(reader["TotalFavorites"]),
                AverageRating = Convert.ToDouble(reader["AverageRating"])
            };
        }

        #endregion
    }
}