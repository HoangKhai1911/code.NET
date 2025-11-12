using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinCook.Models;

namespace WinCook.Services
{
    /// <summary>
    /// Lớp static (tĩnh) dùng để lưu trữ thông tin
    /// của người dùng đang đăng nhập trong suốt phiên làm việc.
    /// (Nhiệm vụ: Nhóm 0 - Ktuoi, Khải)
    /// </summary>
    public static class AuthManager
    {
        /// <summary>
        /// Lưu trữ thông tin người dùng (User) sau khi đăng nhập thành công.
        /// </summary>
        public static User CurrentUser { get; set; }

        /// <summary>
        /// Kiểm tra xem đã có ai đăng nhập hay chưa.
        /// </summary>
        public static bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Xử lý đăng xuất, xóa thông tin người dùng.
        /// </summary>
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}