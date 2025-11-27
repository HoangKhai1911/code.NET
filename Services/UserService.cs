//UserService.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using WinCook.Models; // Thêm namespace của User model
using System.Windows.Forms; // để dùng MessageBox cho debug

namespace WinCook.Services
{
    /// <summary>
    /// Xử lý tất cả logic liên quan đến Đăng ký và Đăng nhập.
    /// Đây là nhiệm vụ của Nhóm 0 (Ktuoi, Khải).
    /// </summary>
    public class UserService
    {
        // Lấy chuỗi kết nối từ lớp DBHelper
        private string connectionString = DBHelper.ConnectionString;

        /// <summary>
        /// Xử lý đăng ký tài khoản mới.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <returns>True nếu đăng ký thành công, False nếu tên đăng nhập đã tồn tại.</returns>
        public bool Register(string username, string password, string email)
        {
            try
            {
                // Mã hóa mật khẩu trước khi lưu
                string hashedPassword = HashPassword(password);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // ===== DEBUG: xem app đang ghi vào đâu =====
                    using (var who = new SqlCommand(
                        "SELECT @@SERVERNAME AS Srv, DB_NAME() AS Db, SUSER_SNAME() AS LoginName", connection))
                    using (var r = who.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            MessageBox.Show(
                                $"Server = {r["Srv"]}\nDB = {r["Db"]}\nLogin = {r["LoginName"]}\nConn = {connection.DataSource}",
                                "Where am I writing?");
                        }
                    }
                    // ===== END DEBUG =====

                    // Câu lệnh SQL để chèn người dùng mới
                    string query = "INSERT INTO Users (username, password_hash, email) VALUES (@username, @password_hash, @email)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password_hash", hashedPassword);
                        command.Parameters.AddWithValue("@email", email);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Lỗi 2627 hoặc 2601 là lỗi vi phạm UNIQUE (tên đăng nhập đã tồn tại)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    Console.WriteLine("Tên đăng nhập đã tồn tại.");
                    return false;
                }
                // Xử lý các lỗi SQL khác
                Console.WriteLine("Lỗi SQL khi đăng ký: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi không xác định khi đăng ký: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Xử lý đăng nhập.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Trả về đối tượng User nếu đăng nhập thành công, null nếu thất bại.</returns>
        public User Login(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Lấy user_id và mật khẩu đã mã hóa từ DB
                    // Tìm dòng này trong hàm Login:
                    string query = "SELECT user_id, password_hash, email, AvatarUrl, FullName FROM Users WHERE username = @username";
                    // Đảm bảo có đủ 5 cột trên.

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Lấy thông tin từ DB
                                int userId = (int)reader["user_id"];
                                string storedHash = (string)reader["password_hash"];
                                string email = reader["email"] != DBNull.Value ? (string)reader["email"] : null;

                                // Mã hóa mật khẩu người dùng nhập và so sánh
                                string inputHash = HashPassword(password);

                                if (inputHash == storedHash)
                                {
                                    // Đăng nhập thành công, trả về thông tin người dùng
                                    return new User { UserId = userId, Username = username, Email = email };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đăng nhập: " + ex.Message);
            }

            // Nếu không tìm thấy user hoặc sai mật khẩu
            return null;
        }

        /// <summary>
        /// Hàm băm mật khẩu bằng SHA256.
        /// </summary>
        /// <param name="password">Mật khẩu chuỗi thô.</param>
        /// <returns>Chuỗi hash 64 ký tự (hex).</returns>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Băm mật khẩu
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển byte array sang chuỗi hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ... (Các using giữ nguyên)

        public bool UpdateUserAvatar(int userId, string avatarPath)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Cập nhật đường dẫn ảnh
                    string query = "UPDATE Users SET AvatarUrl = @Path WHERE user_id = @Id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        // Xử lý null an toàn
                        cmd.Parameters.AddWithValue("@Path", (object)avatarPath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", userId);

                        int rows = cmd.ExecuteNonQuery();

                        // Nếu update thành công -> Cập nhật luôn bộ nhớ đệm AuthManager
                        if (rows > 0)
                        {
                            if (AuthManager.CurrentUser != null)
                                AuthManager.CurrentUser.AvatarUrl = avatarPath;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Hiện lỗi ra để biết sai ở đâu
                MessageBox.Show("Lỗi SQL Update Avatar: " + ex.Message);
            }
            return false;
        }

        public bool UpdateUserProfile(int userId, string newFullName, string newEmail, string phone)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Cập nhật Tên hiển thị (FullName) và Email
                    // Lưu ý: Kiểm tra kỹ tên bảng là 'Users' và tên cột ID là 'user_id'
                    string query = "UPDATE Users SET FullName = @Name, Email = @Email WHERE user_id = @Id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", (object)newFullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", (object)newEmail ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", userId);

                        int rows = cmd.ExecuteNonQuery();

                        // Nếu update thành công -> Cập nhật luôn bộ nhớ đệm AuthManager
                        if (rows > 0)
                        {
                            if (AuthManager.CurrentUser != null)
                            {
                                AuthManager.CurrentUser.FullName = newFullName;
                                AuthManager.CurrentUser.Email = newEmail;
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Hiện lỗi ra để biết sai ở đâu
                MessageBox.Show("Lỗi SQL Update Profile: " + ex.Message);
            }
            return false;
        }
    }
}