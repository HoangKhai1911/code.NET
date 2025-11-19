using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinCook.Models; // Vẫn cần using Models

namespace WinCook.Controls // <-- ĐÃ CẬP NHẬT NAMESPACE
{
    public partial class ucRecipeCard : UserControl
    {
        // Biến này sẽ lưu trữ toàn bộ thông tin của thẻ
        private readonly Recipe _recipe;

        // Tạo một Event (sự kiện) tên là CardClicked
        // Form frmRecipes sẽ "lắng nghe" sự kiện này
        public event EventHandler CardClicked;

        // === SỬA LỖI DESIGNER ===
        // 1. Thêm constructor không tham số (parameterless)
        //    BẮT BUỘC phải có để Designer hoạt động
        public ucRecipeCard()
        {
            InitializeComponent();
        }

        // 2. Constructor chính (dùng khi chạy): Nhận dữ liệu Recipe
        public ucRecipeCard(Recipe recipe) : this() // Gọi this() để chạy InitializeComponent() ở trên
        {
            _recipe = recipe;

            // 3. Gán dữ liệu lên UI
            lblTitle.Text = recipe.Title;
            lblAuthor.Text = "by " + recipe.AuthorName;

            // 4. Tải ảnh (xử lý lỗi)
            if (!string.IsNullOrEmpty(recipe.ImageUrl))
            {
                try
                {
                    // Dùng LoadAsync để tải ảnh bất đồng bộ,
                    // giúp UI không bị "đơ" khi tải 30 ảnh
                    picImage.LoadAsync(recipe.ImageUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi tải ảnh: " + ex.Message);
                    // Nếu lỗi, hiển thị ảnh mặc định
                    picImage.ImageLocation = "https://placehold.co/200x150/E8AA8B/ffffff?text=Image+Error";
                }
            }

            // 5. Gán sự kiện Click cho TOÀN BỘ control
            // Bất kể bấm vào Ảnh, Chữ hay Nền, đều sẽ kích hoạt
            this.Click += new EventHandler(OnCardClick);
            lblTitle.Click += new EventHandler(OnCardClick);
            lblAuthor.Click += new EventHandler(OnCardClick);
            picImage.Click += new EventHandler(OnCardClick);
        }

        /// <summary>
        /// Hàm này được gọi khi bấm vào bất kỳ đâu trên thẻ
        /// </summary>
        private void OnCardClick(object sender, EventArgs e)
        {
            // Kích hoạt sự kiện CardClicked
            // và gửi "chính nó" (this - tức là cái ucRecipeCard) 
            // về cho frmRecipes biết
            CardClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Hàm này cho phép frmRecipes lấy RecipeId của thẻ đã được bấm
        /// </summary>
        public int GetRecipeId()
        {
            // Thêm kiểm tra null (phòng trường hợp Designer gọi)
            if (_recipe != null)
            {
                return _recipe.RecipeId;
            }
            return 0; // Trả về 0 nếu đang ở chế độ Design
        }
    }
}