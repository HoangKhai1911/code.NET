using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CollectionModel = WinCook.Models.Collection;
using WinCook.Services;
using Microsoft.VisualBasic; // Cần cho InputBox

namespace WinCook.Forms
{
    public partial class frmSelectCollection : Form
    {
        private readonly InteractionService _interactionService;
        private readonly int _currentUserId;

        // Property để trả về ID đã chọn
        public int SelectedCollectionId { get; private set; } = -1;

        public frmSelectCollection(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            _interactionService = new InteractionService();

            LoadCollections();
        }

        private void LoadCollections()
        {
            var collections = _interactionService.GetUserCollections(_currentUserId);

            cboCollections.DataSource = collections;
            cboCollections.DisplayMember = "Name";
            cboCollections.ValueMember = "CollectionId";

            if (collections.Count == 0)
            {
                cboCollections.Enabled = false;
                btnOK.Enabled = false;
                cboCollections.Text = "(Chưa có bộ sưu tập nào)";
            }
            else
            {
                cboCollections.Enabled = true;
                btnOK.Enabled = true;
            }
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Nhập tên bộ sưu tập mới:", "Tạo mới", "");
            if (!string.IsNullOrWhiteSpace(newName))
            {
                CollectionModel newCol = new CollectionModel
                {
                    UserId = _currentUserId,
                    Name = newName,
                    Description = ""
                };

                if (_interactionService.CreateCollection(newCol))
                {
                    MessageBox.Show("Tạo thành công!", "Thông báo");
                    LoadCollections(); // Tải lại danh sách

                    // Chọn cái mới tạo (cách đơn giản nhất là chọn cái cuối hoặc tìm theo tên)
                    // Ở đây tạm thời reload, user chọn lại
                }
                else
                {
                    MessageBox.Show("Lỗi khi tạo bộ sưu tập.");
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cboCollections.SelectedValue != null)
            {
                SelectedCollectionId = (int)cboCollections.SelectedValue;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}