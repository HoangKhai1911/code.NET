//frmHomePage.cs
using System;
using System.Linq;
using System.Windows.Forms;
using WinCook.Services;

namespace WinCook
{
    public partial class frmHomePage : Form
    {
        public frmHomePage()
        {
            InitializeComponent();

            // Chạy các hàm thiết lập
            SetupWelcomeMessage();
            SetupButtonEvents();
        }

        /// <summary>
        /// (Nhóm 0) Hiển thị lời chào mừng người dùng.
        /// </summary>
        private void SetupWelcomeMessage()
        {
            if (AuthManager.IsLoggedIn)
            {
                // Tái sử dụng label 2 để hiển thị lời chào
                guna2HtmlLabel2.Text = "Chào mừng, " + AuthManager.CurrentUser.Username + "!";
                guna2HtmlLabel3.Text = "Khám phá các công thức hoặc tạo món ăn của riêng bạn.";
            }
            else
            {
                guna2HtmlLabel2.Text = "Chào mừng, Khách!";
                guna2HtmlLabel3.Text = "Vui lòng đăng nhập để trải nghiệm đầy đủ.";
            }
        }

        /// <summary>
        /// Gán sự kiện Click cho các nút chưa có trong Designer.
        /// </summary>
        private void SetupButtonEvents()
        {
            // Các nút này CHƯA có sự kiện trong file Designer của bạn,
            // nên chúng ta phải gán thủ công.
            guna2Button2.Click += BtnRecipes_Click;     // Nút "Recipes"
            guna2Button5.Click += BtnFavorites_Click;   // Nút "Favorites"
            guna2Button3.Click += BtnCollections_Click; // Nút "Collections"
            guna2Button4.Click += BtnProfiles_Click;    // Nút "Profiles"

            // Gán sự kiện FormClosing để xử lý Logout
            this.FormClosing += FrmHomePage_FormClosing;
        }

        #region === (Event Handlers - Từ Designer) ===

        // (Nhóm A - Fuc) Nút "Go!" (guna2Button6)
        // Đây là sự kiện đã có trong Designer của bạn (guna2Button6_Click_1)
        private void guna2Button6_Click_1(object sender, EventArgs e)
        {
            // Nút "Go!" sẽ mở form danh sách Recipes
            OpenForm(new frmRecipes()); // <-- Tên form từ screenshot của bạn
        }

        // Nút "Home" (guna2Button1)
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Chúng ta đang ở Home, không cần làm gì
            MessageBox.Show("Bạn đã ở trang chủ.", "Thông báo");
        }

        // Sự kiện trống từ Designer (để tránh lỗi)
        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {
            // (Không cần code)
        }

        #endregion

        #region === (Event Handlers - Gán thủ công) ===

        // (Nhóm A - Fuc) Nút "Recipes" (guna2Button2)
        private void BtnRecipes_Click(object sender, EventArgs e)
        {
            OpenForm(new frmRecipes());
        }
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // (Sự kiện này được gọi từ Designer - Bỏ trống)
        }
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            // (Sự kiện này được gọi từ Designer - Bỏ trống)
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            // (Sự kiện này được gọi từ Designer - Bỏ trống)
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // (Sự kiện này được gọi từ Designer - Bỏ trống)
        }
        // (Nhóm B - Tổng tài audio) Nút "Favorites" (guna2Button5)
        private void BtnFavorites_Click(object sender, EventArgs e)
        {
            OpenForm(new frmMyFavRecipes()); // <-- Tên form từ screenshot của bạn
        }

        // (Nhóm B - Tổng tài audio) Nút "Collections" (guna2Button3)
        private void BtnCollections_Click(object sender, EventArgs e)
        {
            OpenForm(new frmCollection()); // <-- Tên form từ screenshot của bạn
        }

        // (Nhóm B) Nút "Profiles" (guna2Button4)
        // (Nhóm B) Nút "Profiles" (guna2Button4)
        private void BtnProfiles_Click(object sender, EventArgs e)
        {
            // Mở đúng form Profiles
            OpenForm(new frmProfile());
        }


        #endregion

        #region === (Hàm tiện ích & Logout) ===

        /// <summary>
        /// Hàm tiện ích để mở Form mới và *ẨN* Form hiện tại.
        /// (SỬA LỖI: Đổi 'this.Close()' thành 'this.Hide()')
        /// </summary>
        private void OpenForm(Form newForm)
        {
            newForm.Show();
            this.Hide(); // <-- LỖI LÀ Ở ĐÂY, ĐÃ SỬA TỪ 'Close()' THÀNH 'Hide()'
        }

        /// <summary>
        /// (Nhóm 0 - Khải) Xử lý Đăng xuất khi đóng Form.
        /// (SỬA LỖI: Thêm kiểm tra 'IsLoggedIn' để tránh logout 2 lần)
        /// </summary>
        private void FrmHomePage_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Chỉ logout và mở Login nếu người dùng bấm 'X'
            // và họ CHƯA chủ động logout (ví dụ: từ frmProfile)
            if (e.CloseReason == CloseReason.UserClosing && AuthManager.IsLoggedIn)
            {
                AuthManager.Logout();

                // Tìm và hiển thị lại form Login đã bị ẩn
                frmLogin loginForm = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();

                if (loginForm != null)
                {
                    loginForm.Show();
                }
                else
                {
                    // Fallback: Nếu không tìm thấy form Login (trường hợp hiếm)
                    new frmLogin().Show();
                }
            }
        }

        #endregion
    }
}