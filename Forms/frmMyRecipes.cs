using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinCook.Models;
using WinCook.Services;

namespace WinCook
{
    public partial class frmMyRecipes : Form
    {
        // Dùng chung RecipeService + AuthManager
        private readonly RecipeService _recipeService = new RecipeService();

        // Lấy UserId hiện tại: nếu chưa login thì tạm dùng 1 (demo)
        private int CurrentUserId => AuthManager.IsLoggedIn
            ? AuthManager.CurrentUser.UserId
            : 1;

        public frmMyRecipes()
        {
            InitializeComponent();

            // Khi form load thì hiển thị danh sách công thức
            this.Load += (s, e) => LoadMyRecipes();

            // Nút Add: Designer đã gán sẵn Click += guna2Button7_Click;
            // nên ở đây KHÔNG cần gán lại để tránh chạy 2 lần.
        }

        // 🟢 Hàm hiển thị danh sách công thức
        private void LoadMyRecipes()
        {
            flowLayoutPanel1.Controls.Clear();

            // Lấy danh sách công thức của user hiện tại
            List<Recipe> recipes = _recipeService.GetRecipesByUser(CurrentUserId);

            if (recipes == null || recipes.Count == 0)
            {
                Label noDataLabel = new Label
                {
                    Text = "Chưa có công thức nào!",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 12, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                };
                flowLayoutPanel1.Controls.Add(noDataLabel);
                return;
            }

            foreach (var recipe in recipes)
            {
                // ==== Tạo card giống layout mẫu ====

                // Panel ngoài chứa cả hình + phần thông tin
                Panel card = new Panel
                {
                    Width = 275,
                    Height = 366,
                    Margin = new Padding(20, 10, 0, 10),
                };

                // Ảnh món ăn
                PictureBox pic = new PictureBox
                {
                    Location = new Point(0, 3),
                    Size = new Size(278, 199),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackgroundImageLayout = ImageLayout.Zoom
                };

                try
                {
                    if (!string.IsNullOrWhiteSpace(recipe.ImageUrl) && File.Exists(recipe.ImageUrl))
                    {
                        pic.Image = Image.FromFile(recipe.ImageUrl);
                    }
                    else
                    {
                        pic.Image = Properties.Resources.no_image;
                    }
                }
                catch
                {
                    pic.Image = Properties.Resources.no_image;
                }

                // Panel trắng phía dưới chứa text + nút Edit/Delete
                Panel infoPanel = new Panel
                {
                    BackColor = Color.White,
                    Location = new Point(0, 202),
                    Size = new Size(278, 164)
                };

                // ===== Các label giống file Designer =====

                // Tiêu đề món
                Label lblTitle = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Location = new Point(18, -2),
                    Text = recipe.Title
                };

                // "Author :" + tên tác giả
                Label lblAuthorCaption = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(18, 23),
                    Text = "Author :"
                };

                Label lblAuthor = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(84, 23),
                    Text = string.IsNullOrWhiteSpace(recipe.AuthorName)
                        ? "Unknown"
                        : recipe.AuthorName
                };

                // Thời gian
                Label lblTimeCaption = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(18, 47),
                    Text = "Time :"
                };

                Label lblTime = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(84, 46),
                    Text = string.IsNullOrWhiteSpace(recipe.TimeNeeded)
                        ? "N/A"
                        : recipe.TimeNeeded
                };

                // Category
                Label lblCateCaption = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(18, 68),
                    Text = "Cate :"
                };

                Label lblCate = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(82, 71),
                    Text = string.IsNullOrWhiteSpace(recipe.CategoryName)
                        ? "Unknown"
                        : recipe.CategoryName
                };

                // Level / Difficulty
                Label lblLevelCaption = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(18, 91),
                    Text = "Level : "
                };

                Label lblLevel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    Location = new Point(84, 92),
                    Text = string.IsNullOrWhiteSpace(recipe.Difficulty)
                        ? "N/A"
                        : recipe.Difficulty
                };

                // ===== Nút Edit / Delete (dùng Button thường cho đơn giản) =====

                Button btnEdit = new Button
                {
                    Text = "Edit",
                    BackColor = Color.Orange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Size = new Size(77, 32),
                    Location = new Point(89, 127)
                };
                btnEdit.FlatAppearance.BorderSize = 0;
                btnEdit.Click += (s, e) =>
                {
                    var editForm = new frmAddRecipie(recipe.RecipeId);
                    editForm.ShowDialog();
                    // Sau khi sửa xong, load lại danh sách
                    LoadMyRecipes();
                };

                Button btnDelete = new Button
                {
                    Text = "Delete",
                    BackColor = Color.Brown,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Size = new Size(94, 32),
                    Location = new Point(172, 127)
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += (s, e) =>
                {
                    if (MessageBox.Show("Bạn có chắc muốn xóa công thức này?",
                            "Xác nhận xóa",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DeleteRecipe(recipe.RecipeId);
                        LoadMyRecipes();
                    }
                };

                // (Tuỳ chọn) click vào hình để xem chi tiết
                pic.Cursor = Cursors.Hand;
                pic.Click += (s, e) =>
                {
                    var detailForm = new frmRecipeDetails(recipe.RecipeId);
                    detailForm.ShowDialog();
                };

                // Thêm control vào infoPanel
                infoPanel.Controls.Add(btnEdit);
                infoPanel.Controls.Add(btnDelete);
                infoPanel.Controls.Add(lblLevel);
                infoPanel.Controls.Add(lblLevelCaption);
                infoPanel.Controls.Add(lblCate);
                infoPanel.Controls.Add(lblCateCaption);
                infoPanel.Controls.Add(lblTime);
                infoPanel.Controls.Add(lblTimeCaption);
                infoPanel.Controls.Add(lblAuthor);
                infoPanel.Controls.Add(lblAuthorCaption);
                infoPanel.Controls.Add(lblTitle);

                // Thêm vào card
                card.Controls.Add(infoPanel);
                card.Controls.Add(pic);

                // Thêm card vào flowLayoutPanel
                flowLayoutPanel1.Controls.Add(card);
            }
        }

        // 🗑️ Xóa công thức – dùng RecipeService
        private void DeleteRecipe(int recipeId)
        {
            bool success = _recipeService.DeleteRecipe(recipeId);

            if (success)
            {
                MessageBox.Show("Đã xóa công thức!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Không thể xóa công thức.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ➕ Nút thêm công thức mới (Add)
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            var addForm = new frmAddRecipie();
            addForm.ShowDialog();
            LoadMyRecipes();
        }

        // ⚙️ Các nút khác (chưa dùng) – giữ nguyên để không ảnh hưởng chỗ khác
        private void guna2Button8_Click(object sender, EventArgs e) { }
        private void guna2Button6_Click(object sender, EventArgs e) { }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }
        private void guna2Button1_Click(object sender, EventArgs e) { }
        private void guna2Button2_Click(object sender, EventArgs e) { }
        private void guna2Button4_Click(object sender, EventArgs e) { }
        private void guna2Button3_Click(object sender, EventArgs e) { }
        private void guna2Button9_Click(object sender, EventArgs e) { }
        private void guna2Button5_Click(object sender, EventArgs e) { }
        private void guna2Button11_Click(object sender, EventArgs e) { }
        private void guna2Button10_Click(object sender, EventArgs e) { }
        private void guna2Button13_Click(object sender, EventArgs e) { }
        private void guna2Button12_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label45_Click(object sender, EventArgs e) { }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
        }
    }
}
