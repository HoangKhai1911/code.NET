using System;
using System.Drawing;
using System.Windows.Forms;
using WinCook.Models;

namespace WinCook.Controls
{
    public partial class ucCollection : UserControl
    {
        public Collection CurrentCollection { get; private set; }

        // Sự kiện để Form cha (frmCollection) bắt lấy
        public event EventHandler CollectionClicked; // Khi bấm vào để xem chi tiết
        public event EventHandler DeleteClicked;     // Khi bấm nút xóa

        public ucCollection()
        {
            InitializeComponent();
        }

        // Constructor nhận dữ liệu
        public ucCollection(Collection collection) : this()
        {
            CurrentCollection = collection;

            // 1. Gán dữ liệu lên UI
            if (lblName != null) lblName.Text = collection.Name;

            // Hiển thị số lượng bài viết
            if (lblCount != null)
                lblCount.Text = $"{collection.RecipeCount} công thức";

            // 2. Gán sự kiện Click

            // Nút Xóa
            if (btnDelete != null)
            {
                btnDelete.Click += (s, e) => DeleteClicked?.Invoke(this, EventArgs.Empty);
            }

            // Click vào nền/ảnh/tên -> Mở bộ sưu tập
            this.Click += (s, e) => CollectionClicked?.Invoke(this, EventArgs.Empty);
            if (picIcon != null) picIcon.Click += (s, e) => CollectionClicked?.Invoke(this, EventArgs.Empty);
            if (lblName != null) lblName.Click += (s, e) => CollectionClicked?.Invoke(this, EventArgs.Empty);
        }

        // Hàm lấy ID nhanh
        public int GetCollectionId() => CurrentCollection?.CollectionId ?? 0;
    }
}