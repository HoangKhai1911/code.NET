//Services/InteractionService.cs
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
            string query = "INSERT INTO Favorites (user_id, recipe_id) VALUES (@user_id, @recipe_id)";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Có thể lỗi do đã tồn tại (vi phạm Primary Key)
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

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
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
            // Câu query này JOIN 5 bảng và dùng View, khá phức tạp
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
                ORDER BY f.created_at DESC"; // Sắp xếp theo ngày yêu thích

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
                                // Chúng ta tái sử dụng hàm Map của RecipeService (nếu có thể)
                                // Hoặc map thủ công ở đây
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

        #region === Ratings (Đánh giá) ===

        /// <summary>
        /// (Nhóm B) Thêm hoặc Cập nhật đánh giá (score + comment) của người dùng cho 1 công thức.
        /// </summary>
        public bool AddOrUpdateRating(Rating rating)
        {
            // Logic: Kiểm tra xem đã đánh giá chưa. Nếu rồi thì UPDATE, chưa thì INSERT.
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
                        if (result != null)
                        {
                            existingRatingId = (int)result;
                        }
                    }

                    // 2. Quyết định INSERT hay UPDATE
                    if (existingRatingId.HasValue)
                    {
                        // UPDATE
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
                        // INSERT
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

        /// <summary>
        /// (Nhóm B) Lấy tất cả đánh giá của một công thức để hiển thị (kèm tên người đánh giá).
        /// </summary>
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
                                    Username = (string)reader["username"] // Lấy tên người đánh giá
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

        #region === Notes (Ghi chú cá nhân) ===

        /// <summary>
        /// (Nhóm B) Lấy ghi chú cá nhân của người dùng cho một công thức.
        /// </summary>
        public Note GetNote(int userId, int recipeId)
        {
            Note note = null;
            string query = "SELECT note_id, note_text, created_at FROM Notes WHERE user_id = @user_id AND recipe_id = @recipe_id";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@recipe_id", recipeId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                note = new Note
                                {
                                    NoteId = (int)reader["note_id"],
                                    NoteText = (string)reader["note_text"],
                                    CreatedAt = (DateTime)reader["created_at"],
                                    UserId = userId,
                                    RecipeId = recipeId
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy Note: " + ex.Message);
            }
            return note; // Trả về null nếu không tìm thấy
        }

        /// <summary>
        /// (Nhóm B) Thêm hoặc Cập nhật ghi chú cá nhân.
        /// </summary>
        public bool AddOrUpdateNote(Note note)
        {
            // Logic: Tương tự như Rating, kiểm tra xem Note đã tồn tại chưa
            string queryCheck = "SELECT note_id FROM Notes WHERE user_id = @user_id AND recipe_id = @recipe_id";
            string queryInsert = "INSERT INTO Notes (user_id, recipe_id, note_text) VALUES (@user_id, @recipe_id, @note_text)";
            string queryUpdate = "UPDATE Notes SET note_text = @note_text, created_at = GETDATE() WHERE note_id = @note_id";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int? existingNoteId = null;

                    // 1. Kiểm tra
                    using (SqlCommand cmdCheck = new SqlCommand(queryCheck, connection))
                    {
                        cmdCheck.Parameters.AddWithValue("@user_id", note.UserId);
                        cmdCheck.Parameters.AddWithValue("@recipe_id", note.RecipeId);
                        object result = cmdCheck.ExecuteScalar();
                        if (result != null)
                        {
                            existingNoteId = (int)result;
                        }
                    }

                    // 2. Quyết định INSERT hay UPDATE
                    if (existingNoteId.HasValue)
                    {
                        // UPDATE
                        using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, connection))
                        {
                            cmdUpdate.Parameters.AddWithValue("@note_text", note.NoteText);
                            cmdUpdate.Parameters.AddWithValue("@note_id", existingNoteId.Value);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // INSERT
                        using (SqlCommand cmdInsert = new SqlCommand(queryInsert, connection))
                        {
                            cmdInsert.Parameters.AddWithValue("@user_id", note.UserId);
                            cmdInsert.Parameters.AddWithValue("@recipe_id", note.RecipeId);
                            cmdInsert.Parameters.AddWithValue("@note_text", note.NoteText);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm/cập nhật Note: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region === Collections (Bộ sưu tập) ===

        // (Các chức năng này phức tạp hơn, có thể làm sau)
        // Dưới đây là các hàm cơ bản

        /// <summary>
        /// (Nhóm B) Lấy tất cả Bộ sưu tập của một người dùng.
        /// </summary>
        public List<Collection> GetUserCollections(int userId)
        {
            List<Collection> collections = new List<Collection>();
            string query = "SELECT collection_id, name, description, created_at FROM Collections WHERE user_id = @user_id ORDER BY name";
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
                                collections.Add(new Collection
                                {
                                    CollectionId = (int)reader["collection_id"],
                                    Name = (string)reader["name"],
                                    Description = reader["description"].ToString(),
                                    CreatedAt = (DateTime)reader["created_at"],
                                    UserId = userId
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy Collections: " + ex.Message);
            }
            return collections;
        }

        /// <summary>
        /// (Nhóm B) Tạo một Bộ sưu tập mới.
        /// </summary>
        public bool CreateCollection(Collection collection)
        {
            string query = "INSERT INTO Collections (user_id, name, description) VALUES (@user_id, @name, @description)";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", collection.UserId);
                        command.Parameters.AddWithValue("@name", collection.Name);
                        command.Parameters.AddWithValue("@description", (object)collection.Description ?? DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tạo Collection: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// (Nhóm B) Thêm một công thức vào một Bộ sưu tập.
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

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Có thể lỗi do đã tồn tại (vi phạm Primary Key)
                Console.WriteLine("Lỗi khi thêm Recipe vào Collection: " + ex.Message);
                return false;
            }
        }

        #endregion
    }
}