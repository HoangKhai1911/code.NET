using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;
using System.Linq; // Cần cho hàm OfType
using System.Data;
using Guna.UI2.WinForms;
using System.Drawing; // Cần cho màu sắc

namespace WinCook
{
    public partial class frmRecipeDetails : Form
    {
        // Khai báo các Service cần dùng
        private readonly RecipeService _recipeService;
        private readonly InteractionService _interactionService;
        private readonly UtilityService _utilityService; // Thêm Service Nhóm C

        // Biến lưu trữ trạng thái
        private readonly int _recipeId;
        private readonly int _currentUserId;

        // SỬA LỖI CS8618: Thêm dấu ? để cho phép _currentRecipe là null
        private Recipe? _currentRecipe; // Lưu trữ công thức gốc 
        private bool _isCurrentlyFavorite;

        /// <summary>
        /// Constructor: Phải nhận vào recipeId để biết tải món nào
        /// </summary>
        public frmRecipeDetails(int id)
        {
            InitializeComponent();

            _recipeService = new RecipeService();
            _interactionService = new InteractionService();
            _utilityService = new UtilityService(); // Khởi tạo UtilityService

            _recipeId = id;

            // Lấy ID người dùng đang đăng nhập (Rất quan trọng cho Nhóm B)
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                MessageBox.Show("Bạn phải đăng nhập để xem chi tiết.");
                this.Close();
                return; // Dừng lại nếu chưa đăng nhập
            }

            // Gán sự kiện
            this.Load += (s, e) => LoadDetails();

            // === CẬP NHẬT LOGIC NÚT ===
            // 1. (Nút TAG) Gán sự kiện 'Thêm vào Bộ sưu tập' cho guna2Button1
            guna2Button1.Click += btnAddToCollection_Click;

            // 2. (Nút TIM) Gán sự kiện 'Yêu thích' cho guna2Button2
            guna2Button2.Click += btnFavorite_Click;

            // Gán sự kiện cho các nút mới của Nhóm B & C
            // (Bạn cần bỏ comment các dòng này sau khi thêm control vào Designer)
            // btnSaveRating.Click += btnSaveRating_Click;
            // btnSaveNote.Click += btnSaveNote_Click;
            // nudServingsFactor.ValueChanged += nudServingsFactor_ValueChanged;
            // btnCookingMode.Click += btnCookingMode_Click;
        }


        #region === Tải Dữ Liệu (Load) ===

        /// <summary>
        /// (Nhóm A) Tải thông tin chính của công thức (Tên, Ảnh, Nguyên liệu, Các bước)
        /// THAY THẾ logic LoadDetails() cũ bằng Service
        /// </summary>
        private void LoadDetails() // Giữ nguyên tên hàm của bạn
        {
            // Bước 1: Gọi Service để lấy dữ liệu
            _currentRecipe = _recipeService.GetRecipeDetails(_recipeId);
            if (_currentRecipe == null)
            {
                MessageBox.Show("Không thể tải chi tiết công thức.", "Lỗi");
                this.Close();
                return;
            }

            // Bước 2: GÁN DỮ LIỆU LÊN UI (Sử dụng tên control của bạn)
            guna2HtmlLabel3.Text = _currentRecipe.Title;        // Tên món
            guna2HtmlLabel5.Text = _currentRecipe.AuthorName;   // Tác giả
            guna2HtmlLabel7.Text = _currentRecipe.CategoryName; // Danh mục
            guna2HtmlLabel11.Text = !string.IsNullOrEmpty(_currentRecipe.TimeNeeded) ? _currentRecipe.TimeNeeded : "N/A"; // Thời gian
            guna2HtmlLabel9.Text = !string.IsNullOrEmpty(_currentRecipe.Difficulty) ? _currentRecipe.Difficulty : "Medium"; // Độ khó

            // Hiển thị điểm trung bình (lên Label mới, ví dụ: lblAvgRating)
            // lblAvgRating.Text = _currentRecipe.AverageRating.ToString("F1") + " sao";

            // Tải ảnh (Giữ logic của bạn, nhưng dùng ImageUrl từ object)
            string imagePath = _currentRecipe.ImageUrl;
            if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                pictureBox1.ImageLocation = imagePath;
            else
                pictureBox1.Image = null; // Hoặc set ảnh mặc định

            // === SỬA LỖI CS1503 ===
            // Bước 3: Tải Nguyên liệu và Các bước (dưới dạng STRING)
            PopulateIngredients(_currentRecipe.Ingredients); // Gửi string
            PopulateSteps(_currentRecipe.Steps);             // Gửi string

            // Bước 4: Tải các chức năng Nhóm B
            LoadInteractionData();
        }

        /// <summary>
        /// (Nhóm C) Hiển thị danh sách nguyên liệu lên Guna Label
        /// (Nâng cấp từ code của bạn)
        /// SỬA LỖI CS1503: Đổi tham số từ List<T> sang string
        /// </summary>
        private void PopulateIngredients(string ingredientsText) // <-- Đã đổi
        {
            if (!string.IsNullOrEmpty(ingredientsText))
            {
                // Guna HTML Label hỗ trợ <br> tốt hơn \n
                // Thay thế \n (từ CSDL) bằng <br>
                guna2HtmlLabel12.Text = ingredientsText.Replace("\n", "<br>");
            }
            else
            {
                guna2HtmlLabel12.Text = "(Không có nguyên liệu)";
            }
        }

        /// <summary>
        /// (Nhóm A) Hiển thị các bước làm lên Guna Label
        /// (Nâng cấp từ code của bạn)
        /// SỬA LỖI CS1503: Đổi tham số từ List<T> sang string
        /// </summary>
        private void PopulateSteps(string stepsText) // <-- Đã đổi
        {
            if (!string.IsNullOrEmpty(stepsText))
            {
                // Thay thế \n (từ CSDL) bằng <br><br> (cho đẹp)
                // (Chúng ta không thể in đậm "Bước 1:" vì nó là 1 khối text)
                guna2HtmlLabel15.Text = stepsText.Replace("\n", "<br><br>");
            }
            else
            {
                guna2HtmlLabel15.Text = "(Chưa có bước làm)";
            }
        }


        /// <summary>
        /// (Nhóm B - Tổng tài audio) Tải các thông tin tương tác
        /// </summary>
        private void LoadInteractionData()
        {
            // 1. Tải trạng thái Yêu thích
            _isCurrentlyFavorite = _interactionService.IsRecipeFavorited(_currentUserId, _recipeId);
            // Cập nhật Giao diện Nút Tim (guna2Button2)
            UpdateFavoriteButtonVisuals();

            // 2. Tải Ghi chú cá nhân
            var note = _interactionService.GetNote(_currentUserId, _recipeId);
            // if (note != null && txtMyNote != null)
            // {
            //     txtMyNote.Text = note.NoteText;
            // }

            // 3. Tải các bình luận/đánh giá
            LoadAllRatings();
        }

        /// <summary>
        /// (Nhóm B) Tải lại danh sách bình luận (dùng khi mới lưu)
        /// </summary>
        private void LoadAllRatings()
        {
            List<Rating> ratings = _interactionService.GetRatingsForRecipe(_recipeId);

            // Giả định bạn dùng DataGridView 'dgvComments'
            // if (dgvComments != null)
            // {
            //     dgvComments.DataSource = null;
            //     dgvComments.DataSource = ratings;

            //     // Tùy chỉnh các cột
            //     dgvComments.Columns["RatingId"].Visible = false;
            //     dgvComments.Columns["UserId"].Visible = false;
            //     dgvComments.Columns["RecipeId"].Visible = false;
            //     dgvComments.Columns["Username"].HeaderText = "Người dùng";
            //     dgvComments.Columns["Score"].HeaderText = "Điểm";
            //     dgvComments.Columns["Comment"].HeaderText = "Bình luận";
            //     dgvComments.Columns["Comment"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //     dgvComments.Columns["CreatedAt"].HeaderText = "Ngày";
            // }
        }

        #endregion

        #region === Xử Lý Sự Kiện (Nhóm A, B, C) ===

        // (Hàm này không còn được dùng, giữ lại để tránh lỗi Designer)
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // (Giữ rỗng)
        }

        /// <summary>
        /// (Nhóm B) Sự kiện click nút Yêu thích (NÚT TIM - guna2Button2)
        /// </summary>
        private void btnFavorite_Click(object sender, EventArgs e)
        {
            // Đảo ngược trạng thái
            if (_isCurrentlyFavorite)
            {
                // Đang yêu thích -> Bỏ yêu thích
                _interactionService.RemoveFavorite(_currentUserId, _recipeId);
            }
            else
            {
                // Chưa yêu thích -> Thêm yêu thích
                _interactionService.AddFavorite(_currentUserId, _recipeId);
            }

            // Cập nhật trạng thái
            _isCurrentlyFavorite = !_isCurrentlyFavorite;

            // Cập nhật Giao diện (màu sắc)
            UpdateFavoriteButtonVisuals();
        }

        /// <summary>
        /// (Hàm mới) Cập nhật màu sắc cho nút Tim
        /// </summary>
        private void UpdateFavoriteButtonVisuals()
        {
            if (_isCurrentlyFavorite)
            {
                // Đổi màu nút (guna2Button2) sang màu Đỏ (hoặc màu yêu thích của bạn)
                guna2Button2.FillColor = Color.FromArgb(255, 128, 128); // Màu đỏ nhạt
            }
            else
            {
                // Đổi màu nút (guna2Button2) về màu mặc định (màu cam nhạt)
                guna2Button2.FillColor = Color.LightSalmon;
            }
        }


        /// <summary>
        /// (Nhóm B) Sự kiện click nút Lưu vào Bộ sưu tập (NÚT TAG - guna2Button1)
        /// </summary>
        private void btnAddToCollection_Click(object sender, EventArgs e)
        {
            // (Hàm này giả định tên control là 'guna2Button1')

            // 1. Lấy danh sách Collection của user
            List<Collection> collections = _interactionService.GetUserCollections(_currentUserId);

            // 2. Hiển thị một form/dialog mới cho phép user chọn 1 Collection
            //    (Bạn cần tạo 1 form mới, ví dụ: frmSelectCollection)

            // GIẢ ĐỊNH: Bạn có 1 form tên là frmCollection (theo screenshot)
            // Chúng ta sẽ mở nó
            frmCollection collectionForm = new frmCollection();

            // Code tạm thời
            MessageBox.Show("Chức năng 'Thêm vào Bộ sưu tập' đang được phát triển! \n(Cần tạo frmSelectCollection.cs)");
            // collectionForm.ShowDialog();
        }


        /// <summary>
        /// (Nhóm B) Sự kiện click nút Lưu Đánh giá
        /// </summary>
        private void btnSaveRating_Click(object sender, EventArgs e)
        {
            // Giả định tên control: rsMyRating (Guna2RatingStar), txtMyComment
            // int score = (int)rsMyRating.Value;
            // string comment = txtMyComment.Text.Trim();

            // if (score == 0)
            // {
            //     MessageBox.Show("Vui lòng chấm điểm (chọn sao) trước khi lưu.");
            //     return;
            // }

            // Rating newRating = new Rating
            // {
            //     UserId = _currentUserId,
            //     RecipeId = _recipeId,
            //     Score = score,
            //     Comment = comment
            // };

            // bool success = _interactionService.AddOrUpdateRating(newRating);
            // if (success)
            // {
            //     MessageBox.Show("Đã lưu đánh giá của bạn!");
            //     LoadAllRatings(); // Tải lại danh sách bình luận
            //     // Tải lại điểm trung bình
            //     var avgRating = _recipeService.GetRecipeDetails(_recipeId).AverageRating;
            //     lblAvgRating.Text = avgRating.ToString("F1") + " sao";
            // }
            // else
            // {
            //     MessageBox.Show("Lỗi khi lưu đánh giá.");
            // }
        }

        /// <summary>
        /// (Nhóm B) Sự kiện click nút Lưu Ghi chú
        /// </summary>
        private void btnSaveNote_Click(object sender, EventArgs e)
        {
            // Giả định tên control: txtMyNote
            // string noteText = txtMyNote.Text.Trim();

            // Note myNote = new Note
            // {
            //     UserId = _currentUserId,
            //     RecipeId = _recipeId,
            //     NoteText = noteText
            // };

            // bool success = _interactionService.AddOrUpdateNote(myNote);
            // if (success)
            // {
            //     MessageBox.Show("Đã lưu ghi chú cá nhân!");
            // }
            // else
            // {
            //     MessageBox.Show("Lỗi khi lưu ghi chú.");
            // }
        }

        /// <summary>
        /// (Nhóm C - Fuc) Tự động tính toán lại khẩu phần khi người dùng thay đổi giá trị
        /// </summary>
        private void nudServingsFactor_ValueChanged(object sender, EventArgs e)
        {
            // Giả định tên control: nudServingsFactor (Guna2NumericUpDown)
            if (_currentRecipe == null) return; // Đảm bảo _currentRecipe đã được tải

            // double factor = (double)nudServingsFactor.Value;

            // === LƯU Ý: HÀM NÀY SẼ KHÔNG HOẠT ĐỘNG CHÍNH XÁC NỮA ===
            // (Vì _utilityService.AdjustServings đang mong đợi List<Ingredient>)
            // (Chúng ta phải viết lại UtilityService để phân tích chuỗi NVARCHAR(MAX),
            // việc này rất phức tạp và nằm ngoài phạm vi cơ bản)

            MessageBox.Show("Chức năng 'Điều chỉnh khẩu phần' yêu cầu CSDL được chuẩn hóa (bảng Ingredients riêng biệt) mới có thể hoạt động chính xác.", "Thông báo");

            // // Lấy danh sách nguyên liệu gốc (đã lưu trong _currentRecipe)
            // string originalList = _currentRecipe.Ingredients; // <-- Dòng này LÀ string

            // // Gọi Service để tính toán (Service cũng cần được viết lại)
            // string adjustedList = _utilityService.AdjustServings(originalList, factor); 

            // // Hiển thị lại danh sách nguyên liệu đã điều chỉnh
            // PopulateIngredients(adjustedList);
        }

        /// <summary>
        /// (Nhóm C) Mở Chế độ Nấu ăn
        /// </summary>
        private void btnCookingMode_Click(object sender, EventArgs e)
        {
            // Giả định bạn đã tạo 'frmCookingMode' (theo cây thư mục)
            // if (_currentRecipe != null && !string.IsNullOrEmpty(_currentRecipe.Steps))
            // {
            //    frmCookingMode frm = new frmCookingMode(_currentRecipe.Steps); // Gửi chuỗi Steps
            //    frm.Show();
            // }
        }

        #endregion
    }
}