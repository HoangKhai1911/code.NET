using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinCook.Models;   // Đảm bảo Model
using WinCook.Services; // Đảm bảo Service
using WinCook.Controls; // Đảm bảo UserControl
using Microsoft.VisualBasic; // Dùng cho InputBox

namespace WinCook
{
    public partial class frmCollection : Form
    {
        // === KHAI BÁO SERVICE (NHÓM B) ===
        private readonly InteractionService _interactionService;
        private readonly int _currentUserId;

        // SỬA LỖI CS0104: Chỉ định rõ namespace WinCook.Models
        private List<WinCook.Models.Collection> _allUserCollections;

        public frmCollection()
        {
            InitializeComponent();

            // Khởi tạo Service
            _interactionService = new InteractionService();

            // Lấy ID người dùng
            if (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
            {
                _currentUserId = AuthManager.CurrentUser.UserId;
            }
            else
            {
                MessageBox.Show("Vui lòng đăng nhập để xem Bộ sưu tập.");
                this.Close();
                return;
            }

            _allUserCollections = new List<WinCook.Models.Collection>();

            // Gán sự kiện Load
            this.Load += frmCollection_Load;

            // Gán sự kiện FormClosing để điều hướng về Home
            this.FormClosing += FrmCollection_FormClosing;
        }

        private void frmCollection_Load(object sender, EventArgs e)
        {
            LoadCollections();
        }

        #region === Logic Chính (Nhóm B) ===

        /// <summary>
        /// Tải tất cả Collection của user từ Service
        /// </summary>
        private void LoadCollections()
        {
            try
            {
                // Gọi Service lấy danh sách mới nhất
                _allUserCollections = _interactionService.GetUserCollections(_currentUserId);

                // Hiển thị lên FlowLayoutPanel
                PopulateCollectionList(_allUserCollections);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bộ sưu tập: " + ex.Message);
            }
        }

        /// <summary>
        /// Đổ danh sách vào FlowLayoutPanel sử dụng ucCollection
        /// </summary>
        private void PopulateCollectionList(List<WinCook.Models.Collection> collections)
        {
            // 1. Xóa thẻ cũ
            flowLayoutPanel1.Controls.Clear();

            // 2. Kiểm tra rỗng
            if (collections == null || collections.Count == 0)
            {
                Label lblEmpty = new Label();
                lblEmpty.Text = "Bạn chưa có bộ sưu tập nào.\nHãy bấm nút (+) để tạo mới!";
                lblEmpty.AutoSize = false;
                lblEmpty.Width = flowLayoutPanel1.Width - 20;
                lblEmpty.Height = 100;
                lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                lblEmpty.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
                lblEmpty.ForeColor = System.Drawing.Color.Gray;

                flowLayoutPanel1.Controls.Add(lblEmpty);
                return;
            }

            // 3. Tạo thẻ ucCollection
            foreach (var col in collections)
            {
                // === SỬ DỤNG ucCollection ===
                ucCollection card = new ucCollection(col);
                card.Margin = new Padding(15); // Cách xa nhau 1 chút cho thoáng

                // --- XỬ LÝ SỰ KIỆN XÓA ---
                card.DeleteClicked += (s, e) =>
                {
                    var confirm = MessageBox.Show($"Bạn có chắc muốn xóa bộ sưu tập '{col.Name}'?\nCác món ăn bên trong sẽ KHÔNG bị xóa khỏi hệ thống.",
                                                  "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirm == DialogResult.Yes)
                    {
                        if (_interactionService.DeleteCollection(col.CollectionId))
                        {
                            MessageBox.Show("Đã xóa bộ sưu tập!");
                            LoadCollections(); // Tải lại danh sách
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi xóa.");
                        }
                    }
                };

                // --- XỬ LÝ SỰ KIỆN CLICK (XEM CHI TIẾT) ---
                card.CollectionClicked += (s, e) =>
                {
                    // 1. Khởi tạo form chi tiết, truyền ID và Tên bộ sưu tập
                    using (var f = new frmCollectionDetails(col.CollectionId, col.Name))
                    {
                        // 2. Hiển thị form lên
                        f.ShowDialog();
                    }
                };

                flowLayoutPanel1.Controls.Add(card);
            }
        }

        /// <summary>
        /// Thêm Collection mới (Nút Add)
        /// </summary>
        private void AddNewCollection()
        {
            // Dùng InputBox của VisualBasic để nhập nhanh tên
            string newName = Interaction.InputBox("Nhập tên Bộ sưu tập mới:", "Tạo Bộ Sưu Tập", "Món ngon mỗi ngày");

            if (string.IsNullOrWhiteSpace(newName))
            {
                return; // Người dùng hủy hoặc không nhập gì
            }

            // Tạo đối tượng
            var newCollection = new WinCook.Models.Collection
            {
                UserId = _currentUserId,
                Name = newName.Trim(),
                Description = ""
            };

            // Gọi Service
            if (_interactionService.CreateCollection(newCollection))
            {
                MessageBox.Show("Tạo thành công!", "Thông báo");
                LoadCollections(); // Tải lại để thấy cái mới
            }
            else
            {
                MessageBox.Show("Lỗi khi tạo. Có thể tên đã tồn tại.");
            }
        }

        /// <summary>
        /// Tìm kiếm bộ sưu tập (Nút Search)
        /// </summary>
        private void SearchCollections()
        {
            // Giả sử textbox tên là guna2TextBox1
            string keyword = guna2TextBox1 != null ? guna2TextBox1.Text.Trim().ToLower() : "";

            if (string.IsNullOrEmpty(keyword))
            {
                PopulateCollectionList(_allUserCollections);
            }
            else
            {
                var filtered = _allUserCollections
                    .Where(c => c.Name.ToLower().Contains(keyword))
                    .ToList();
                PopulateCollectionList(filtered);
            }
        }

        #endregion

        #region === Điều hướng & Form ===

        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        private void FrmCollection_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var homePage = Application.OpenForms.OfType<frmHomePage>().FirstOrDefault();
                if (homePage != null) homePage.Show();
                else
                {
                    var login = Application.OpenForms.OfType<frmLogin>().FirstOrDefault();
                    if (login != null) login.Show();
                    else new frmLogin().Show();
                }
            }
        }

        // Menu Navigation
        private void guna2Button1_Click(object sender, EventArgs e) => OpenForm(new frmHomePage());
        private void guna2Button3_Click(object sender, EventArgs e) => OpenForm(new frmRecipes());
        private void guna2Button5_Click(object sender, EventArgs e) => OpenForm(new frmMyFavRecipes());
        private void guna2Button2_Click(object sender, EventArgs e) { } // Đang ở đây
        private void guna2Button4_Click(object sender, EventArgs e) => OpenForm(new frmProfile());

        // Nút Search
        private void guna2Button6_Click(object sender, EventArgs e) => SearchCollections();

        // Nút Add (+)
        private void guna2Button7_Click(object sender, EventArgs e) => AddNewCollection();

        // Nút Delete (Thùng rác lớn) - Có thể bỏ vì đã có nút xóa trên từng thẻ
        private void guna2Button8_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem có dữ liệu để xóa không
            if (_allUserCollections == null || _allUserCollections.Count == 0)
            {
                MessageBox.Show("Danh sách trống, không có gì để xóa.", "Thông báo");
                return;
            }

            // 2. Hộp thoại xác nhận (Rất quan trọng cho nút Delete All)
            var result = MessageBox.Show(
                "CẢNH BÁO: Bạn có chắc chắn muốn xóa TẤT CẢ bộ sưu tập không?\n\nHành động này không thể hoàn tác!",
                "Xác nhận Xóa Hết",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2); // Mặc định chọn No cho an toàn

            if (result == DialogResult.Yes)
            {
                // 3. Gọi Service xóa sạch
                bool success = _interactionService.DeleteAllUserCollections(_currentUserId);

                if (success)
                {
                    MessageBox.Show("Đã xóa toàn bộ bộ sưu tập thành công!", "Thành công");

                    // 4. Tải lại danh sách (lúc này sẽ trống trơn)
                    LoadCollections();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi xóa dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        } 

        #endregion
    }
}