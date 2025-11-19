//Forms/frmCollection.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;   // <-- THÊM
using WinCook.Services; // <-- THÊM
using Microsoft.VisualBasic; // <-- THÊM (Để dùng InputBox)

namespace WinCook
{
    public partial class frmCollection : Form
    {
        // === KHAI BÁO SERVICE (NHÓM B) ===
        private readonly InteractionService _interactionService;
        private readonly int _currentUserId;
        
        // SỬA LỖI CS0104: Chỉ định rõ WinCook.Models.Collection
        private List<WinCook.Models.Collection> _allUserCollections; // Biến lưu trữ danh sách

        public frmCollection()
        {
            InitializeComponent();

            // Khởi tạo Service
            _interactionService = new InteractionService();

            // Lấy ID người dùng
            if (AuthManager.IsLoggedIn)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                // (Xử lý nếu chưa đăng nhập)
                MessageBox.Show("Vui lòng đăng nhập.");
                this.Close();
                return;
            }
            
            // SỬA LỖI CS0104: Chỉ định rõ WinCook.Models.Collection
            _allUserCollections = new List<WinCook.Models.Collection>();

            // === SỬA LỖI DESIGNER (CS0103) ===
            // Gán sự kiện thủ công (bỏ qua file Designer)
            //guna2Button1.Click += guna2Button1_Click; // Home
            //guna2Button3.Click += guna2Button3_Click; // Recipes
            //guna2Button5.Click += guna2Button5_Click; // Favorites
            //guna2Button2.Click += guna2Button2_Click; // Collections (hiện tại)
            //guna2Button4.Click += guna2Button4_Click; // Profiles
            //guna2Button6.Click += guna2Button6_Click; // Search
            //guna2Button7.Click += guna2Button7_Click; // Add
            //guna2Button8.Click += guna2Button8_Click; // Delete
        }

        private void frmCollection_Load(object sender, EventArgs e)
        {
            // Tải danh sách Collections khi form được mở
            LoadCollections();
        }

        #region === Logic Chính (Nhóm B) ===

        /// <summary>
        /// (Nhóm B) Tải tất cả Collection của user từ Service
        /// </summary>
        private void LoadCollections()
        {
            // Gọi Service
            _allUserCollections = _interactionService.GetUserCollections(_currentUserId);
            
            // Hiển thị
            PopulateCollectionList(_allUserCollections);
        }

        /// <summary>
        // SỬA LỖI CS0104: Chỉ định rõ WinCook.Models.Collection
        /// </summary>
        private void PopulateCollectionList(List<WinCook.Models.Collection> collections)
        {
            flowLayoutPanel1.Controls.Clear(); // Xóa thẻ cũ

            if (collections == null || collections.Count == 0)
            {
                // (Có thể thêm 1 Label báo "Bạn chưa có bộ sưu tập nào")
                return;
            }

            foreach (var collection in collections)
            {
                // === BẠN CẦN TẠO USER CONTROL 'ucCollectionCard.cs' ===
                
                // (Bỏ comment sau khi tạo ucCollectionCard)
                // ucCollectionCard card = new ucCollectionCard(collection);
                // 
                // card.Click += (s, e) => {
                //    // TODO: Mở form chi tiết bộ sưu tập (frmCollectionDetails)
                //    // (Form này chưa có, có thể mở frmRecipes đã được lọc?)
                //    MessageBox.Show("Sẽ mở chi tiết bộ sưu tập: " + collection.Name);
                // };
                // 
                // flowLayoutPanel1.Controls.Add(card);
            }
        }

        /// <summary>
        /// (Nhóm B) Lọc danh sách (Nút Search)
        /// </summary>
        private void SearchCollections()
        {
            string searchTerm = guna2TextBox1.Text.Trim().ToLower(); // Giả định tên ô Search

            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateCollectionList(_allUserCollections); // Hiển thị lại tất cả
                return;
            }

            // Lọc danh sách đã tải
            var filteredList = _allUserCollections
                .Where(c => c.Name.ToLower().Contains(searchTerm))
                .ToList();
            
            PopulateCollectionList(filteredList);
        }

        /// <summary>
        /// (Nhóm B) Thêm Collection mới (Nút Add)
        /// </summary>
        private void AddNewCollection()
        {
            // Dùng InputBox của VisualBasic (cần 'using Microsoft.VisualBasic;')
            string newName = Interaction.InputBox("Nhập tên Bộ sưu tập mới:", "Tạo Bộ Sưu Tập", "Ví dụ: Món ăn ngày Tết");

            if (string.IsNullOrWhiteSpace(newName))
            {
                return; // Người dùng hủy
            }

            // Tạo đối tượng mới
            // SỬA LỖI CS0104: Chỉ định rõ WinCook.Models.Collection
            WinCook.Models.Collection newCollection = new WinCook.Models.Collection
            {
                UserId = _currentUserId,
                Name = newName,
                Description = "" // (Có thể thêm 1 ô nhập mô tả sau)
            };

            // Gọi Service
            bool success = _interactionService.CreateCollection(newCollection);

            if (success)
            {
                MessageBox.Show("Đã tạo Bộ sưu tập thành công!");
                LoadCollections(); // Tải lại danh sách (để lấy ID mới)
            }
            else
            {
                MessageBox.Show("Lỗi khi tạo Bộ sưu tập.");
            }
        }

        #endregion

        // Helper dùng chung
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // ===== Thanh menu top trong frmCollection =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections (đang ở Collections -> không cần chuyển)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Đã ở Collections, không làm gì
        }

        // Profiles
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmProfile();
            OpenForm(f);
        }

        // Các nút Search / Add / Delete
        // Search
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // (Đã thêm logic)
            SearchCollections();
        }

        // Add
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // (Đã thêm logic)
            AddNewCollection();
        }

        // Delete
        private void guna2Button8_Click(object sender, EventArgs e)
        {
            // TODO: (Tổng tài audio) Logic Xóa
            // 1. Cần xác định Collection nào đang được chọn
            //    (FlowLayoutPanel không hỗ trợ 'SelectedItem' tốt)
            // 2. Lấy 'collection.CollectionId'
            // 3. Gọi _interactionService.DeleteCollection(collectionId)
            // 4. LoadCollections();
            MessageBox.Show("Chức năng Xóa đang được phát triển!");
        }
    }
}