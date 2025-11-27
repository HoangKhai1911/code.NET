using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using WinCook.Controls; // Để dùng ucRecipeCard

namespace WinCook
{
    public partial class frmCollectionDetails : Form
    {
        // Các biến lưu trữ thông tin
        private readonly int _collectionId;
        private readonly string _collectionName;
        private readonly InteractionService _interactionService;

        /// <summary>
        /// Constructor nhận ID và Tên bộ sưu tập
        /// </summary>
        public frmCollectionDetails(int collectionId, string collectionName)
        {
            InitializeComponent(); // Hàm này khởi tạo giao diện từ Designer

            // Lưu thông tin
            _collectionId = collectionId;
            _collectionName = collectionName;
            _interactionService = new InteractionService();

            // Cài đặt giao diện ban đầu
            SetupForm();
        }

        private void SetupForm()
        {
            // Gán tiêu đề form và Label
            this.Text = _collectionName;
            if (lblTitle != null)
                lblTitle.Text = $"Bộ sưu tập: {_collectionName}";

            // Gán sự kiện nút Quay lại
            if (btnBack != null)
                btnBack.Click += (s, e) => this.Close();

            // Gán sự kiện Load form để tải dữ liệu
            this.Load += frmCollectionDetails_Load;
        }

        private void frmCollectionDetails_Load(object sender, EventArgs e)
        {
            LoadRecipesInCollection();
        }

        /// <summary>
        /// Tải danh sách món ăn trong BST và hiển thị lên
        /// </summary>
        private void LoadRecipesInCollection()
        {
            try
            {
                // 1. Gọi Service lấy danh sách món ăn theo Collection ID
                List<Recipe> recipes = _interactionService.GetRecipesInCollection(_collectionId);

                // 2. Xóa nội dung cũ trong FlowLayoutPanel
                if (flpContent != null)
                    flpContent.Controls.Clear();
                else return; // Tránh lỗi nếu chưa có panel

                // 3. Kiểm tra nếu danh sách trống
                if (recipes == null || recipes.Count == 0)
                {
                    ShowEmptyState();
                    return;
                }

                // 4. Tạo thẻ cho từng món ăn
                foreach (var r in recipes)
                {
                    // Sử dụng UserControl ucRecipeCard
                    ucRecipeCard card = new ucRecipeCard(r);
                    card.Margin = new Padding(10);

                    // Xử lý khi bấm vào thẻ -> Mở chi tiết món ăn
                    card.CardClicked += (s, e) =>
                    {
                        using (var f = new frmRecipeDetails(r.RecipeId))
                        {
                            f.ShowDialog();
                        }
                        // (Tùy chọn) Có thể tải lại danh sách nếu muốn cập nhật Like/Rating
                        // LoadRecipesInCollection(); 
                    };

                    flpContent.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách món ăn: " + ex.Message);
            }
        }

        /// <summary>
        /// Hiển thị thông báo khi bộ sưu tập trống
        /// </summary>
        private void ShowEmptyState()
        {
            Label lblEmpty = new Label
            {
                Text = "Bộ sưu tập này chưa có món ăn nào.\nHãy thêm món ăn vào bộ sưu tập nhé!",
                AutoSize = false,
                Width = flpContent.Width - 40,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            flpContent.Controls.Add(lblEmpty);
        }
    }
}