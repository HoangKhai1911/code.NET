//Models/User.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WinCook.Models

{
    /// <summary>
    /// Lớp Model để lưu trữ thông tin người dùng khi đã đăng nhập.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; } // Tên hiển thị (Mới thêm)
        public string Email { get; set; }
        public string AvatarUrl { get; set; } // Add this property
    }
}
