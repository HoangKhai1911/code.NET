using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Xử lý tất cả logic nghiệp vụ cho Nhóm B (Tương tác & Cá nhân hóa).
    /// Nhiệm vụ của: Tổng tài audio.
    /// </summary>
    public class InteractionService
    {
        private string connectionString = DBHelper.ConnectionString;

        #region === Favorites (Yêu thích) ===

        /// <summary>
        /// (Nhóm B) Kiểm tra xem người dùng hiện tại đã yêu thích công thức này chưa.
        /// </summary>
        public bool IsRecipeFavorited(int userId, int recipeId)
        {
            string query = "SELECT COUNT(1) FROM Favorites WHERE user_id = @user_id AND recipe_id = @recipe_id";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi kiểm tra Favorite: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// (Nhóm B) Thêm một công thức vào danh sách yêu thích.
        /// </summary>
        public bool AddFavorite(int userId, int recipeId)
        {
            // Dùng IF NOT EXISTS để tránh lỗi trùng lặp
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM Favorites WHERE user_id = @user_id AND recipe_id = @recipe_id)
                BEGIN
                    INSERT INTO Favorites (user_id, recipe_id) VALUES (@user_id, @recipe_id)
                END";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        command.ExecuteNonQuery();
                        return true; // Coi như thành công
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm Favorite: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// (Nhóm B) Xóa một công thức khỏi danh sách yêu thích.
        /// </summary>
        public bool RemoveFavorite(int userId, int recipeId)
        {
            string query = "DELETE FROM Favorites WHERE user_id = @user_id AND recipe_id = @recipe_id";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xóa Favorite: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// (Nhóm B) Lấy tất cả công thức yêu thích của một người dùng.
        /// </summary>
        public List<Recipe> GetFavoriteRecipes(int userId)
        {
            List<Recipe> recipes = new List<Recipe>();
            string query = @"
                SELECT 
                    r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
                    u.username AS AuthorName, 
                    c.name AS CategoryName,
                    ISNULL(rs.total_favorites, 0) AS TotalFavorites,
                    ISNULL(rs.avg_rating, 0) AS AverageRating
                FROM Recipes r
                JOIN Favorites f ON r.recipe_id = f.recipe_id
                JOIN Users u ON r.user_id = u.user_id
                LEFT JOIN Categories c ON r.category_id = c.category_id
                LEFT JOIN Recipe_Stats rs ON r.recipe_id = rs.recipe_id
                WHERE f.user_id = @user_id
                ORDER BY f.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recipes.Add(new Recipe
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
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy danh sách yêu thích: " + ex.Message);
            }
            return recipes;
        }

        #endregion

        #region === Ratings (Đánh giá & Bình luận) ===

        public bool AddOrUpdateRating(Rating rating)
        {
            string queryCheck = "SELECT rating_id FROM Ratings WHERE user_id = @user_id AND recipe_id = @recipe_id";
            string queryInsert = "INSERT INTO Ratings (user_id, recipe_id, score, comment) VALUES (@user_id, @recipe_id, @score, @comment)";
            string queryUpdate = "UPDATE Ratings SET score = @score, comment = @comment, created_at = GETDATE() WHERE rating_id = @rating_id";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int? existingRatingId = null;

                    // 1. Kiểm tra
                    using (SqlCommand cmdCheck = new SqlCommand(queryCheck, connection))
                    {
                        cmdCheck.Parameters.AddWithValue("@user_id", rating.UserId);
                        cmdCheck.Parameters.AddWithValue("@recipe_id", rating.RecipeId);
                        object result = cmdCheck.ExecuteScalar();
                        if (result != null) existingRatingId = (int)result;
                    }

                    // 2. Quyết định INSERT hay UPDATE
                    if (existingRatingId.HasValue)
                    {
                        using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, connection))
                        {
                            cmdUpdate.Parameters.AddWithValue("@score", rating.Score);
                            cmdUpdate.Parameters.AddWithValue("@comment", (object)rating.Comment ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@rating_id", existingRatingId.Value);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlCommand cmdInsert = new SqlCommand(queryInsert, connection))
                        {
                            cmdInsert.Parameters.AddWithValue("@user_id", rating.UserId);
                            cmdInsert.Parameters.AddWithValue("@recipe_id", rating.RecipeId);
                            cmdInsert.Parameters.AddWithValue("@score", rating.Score);
                            cmdInsert.Parameters.AddWithValue("@comment", (object)rating.Comment ?? DBNull.Value);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm/cập nhật Rating: " + ex.Message);
                return false;
            }
        }

        public List<Rating> GetRatingsForRecipe(int recipeId)
        {
            List<Rating> ratings = new List<Rating>();
            string query = @"
                SELECT r.rating_id, r.user_id, r.recipe_id, r.score, r.comment, r.created_at, u.username
                FROM Ratings r
                JOIN Users u ON r.user_id = u.user_id
                WHERE r.recipe_id = @recipe_id
                ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@recipe_id", recipeId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ratings.Add(new Rating
                                {
                                    RatingId = (int)reader["rating_id"],
                                    UserId = (int)reader["user_id"],
                                    RecipeId = (int)reader["recipe_id"],
                                    Score = (int)reader["score"],
                                    Comment = reader["comment"].ToString(),
                                    CreatedAt = (DateTime)reader["created_at"],
                                    Username = (string)reader["username"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy danh sách Ratings: " + ex.Message);
            }
            return ratings;
        }

        #endregion

        #region === Collections (Bộ sưu tập) ===

        /// <summary>
        /// Lấy danh sách bộ sưu tập của User, kèm số lượng món ăn trong đó.
        /// </summary>
        public List<Collection> GetUserCollections(int userId)
        {
            var list = new List<Collection>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Query đếm số bài viết
                    string query = @"
                        SELECT c.collection_id, c.name, c.description, c.created_at, 
                               COUNT(cr.recipe_id) as RecipeCount
                        FROM Collections c
                        LEFT JOIN Collection_Recipes cr ON c.collection_id = cr.collection_id
                        WHERE c.user_id = @uid
                        GROUP BY c.collection_id, c.name, c.description, c.created_at
                        ORDER BY c.created_at DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new Collection
                                {
                                    CollectionId = (int)reader["collection_id"],
                                    UserId = userId,
                                    Name = reader["name"].ToString(),
                                    Description = reader["description"]?.ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    RecipeCount = Convert.ToInt32(reader["RecipeCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy danh sách BST: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Tạo bộ sưu tập mới.
        /// </summary>
        public bool CreateCollection(Collection col)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Kiểm tra trùng tên
                    string check = "SELECT COUNT(*) FROM Collections WHERE user_id = @uid AND name = @name";
                    using (SqlCommand cmdCheck = new SqlCommand(check, connection))
                    {
                        cmdCheck.Parameters.AddWithValue("@uid", col.UserId);
                        cmdCheck.Parameters.AddWithValue("@name", col.Name);
                        if ((int)cmdCheck.ExecuteScalar() > 0) return false; // Đã tồn tại
                    }

                    string query = "INSERT INTO Collections (user_id, name, description) VALUES (@uid, @name, @desc)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", col.UserId);
                        cmd.Parameters.AddWithValue("@name", col.Name);
                        cmd.Parameters.AddWithValue("@desc", col.Description ?? "");
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Xóa bộ sưu tập.
        /// </summary>
        public bool DeleteCollection(int collectionId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Xóa liên kết món ăn trước (để sạch dữ liệu)
                    string delLinks = "DELETE FROM Collection_Recipes WHERE collection_id = @id";
                    using (SqlCommand cmd1 = new SqlCommand(delLinks, connection))
                    {
                        cmd1.Parameters.AddWithValue("@id", collectionId);
                        cmd1.ExecuteNonQuery();
                    }

                    // 2. Xóa collection
                    string query = "DELETE FROM Collections WHERE collection_id = @id";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", collectionId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Thêm một công thức vào một Bộ sưu tập.
        /// </summary>
        public bool AddRecipeToCollection(int collectionId, int recipeId)
        {
            string query = "INSERT INTO Collection_Recipes (collection_id, recipe_id) VALUES (@collection_id, @recipe_id)";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@collection_id", collectionId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm Recipe vào Collection: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Xóa TẤT CẢ bộ sưu tập của một người dùng.
        /// </summary>
        public bool DeleteAllUserCollections(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Vì trong SQL bảng Collection_Recipes đã có ON DELETE CASCADE
                    // Nên chỉ cần xóa ở bảng Collections là các liên kết sẽ tự bay màu.
                    string query = "DELETE FROM Collections WHERE user_id = @uid";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        // ExecuteNonQuery trả về số dòng bị xóa.
                        // Nếu >= 0 nghĩa là lệnh chạy thành công (dù không có gì để xóa vẫn là thành công)
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi xóa tất cả BST: " + ex.Message);
                return false;
            }
        }

        // Trong Services/InteractionService.cs

        /// <summary>
        /// Lấy danh sách các món ăn nằm trong một Bộ sưu tập cụ thể.
        /// </summary>
        public List<Recipe> GetRecipesInCollection(int collectionId)
        {
            List<Recipe> recipes = new List<Recipe>();

            // JOIN bảng Recipes với Collection_Recipes
            string query = @"
        SELECT 
            r.recipe_id, r.title, r.difficulty, r.time_needed, r.image_url,
            u.username AS AuthorName, 
            c.name AS CategoryName,
            ISNULL(rs.total_favorites, 0) AS TotalFavorites,
            ISNULL(rs.avg_rating, 0) AS AverageRating
        FROM Recipes r
        JOIN Collection_Recipes cr ON r.recipe_id = cr.recipe_id
        JOIN Users u ON r.user_id = u.user_id
        LEFT JOIN Categories c ON r.category_id = c.category_id
        LEFT JOIN Recipe_Stats rs ON r.recipe_id = rs.recipe_id
        WHERE cr.collection_id = @colId
        ORDER BY r.created_at DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@colId", collectionId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recipes.Add(new Recipe
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
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy món trong BST: " + ex.Message);
            }
            return recipes;
        }

        #endregion
    }
}