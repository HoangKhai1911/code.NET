// Forms/frmRecipes.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;      // dùng SqlConnection, SqlCommand
using System.IO;                  // check tồn tại file ảnh
using WinCook.Services;           // dùng AuthManager

namespace WinCook
{
    public partial class frmRecipes : Form
    {
        public frmRecipes()
        {
            InitializeComponent();
        }

        // ===== Helper dùng chung để mở form khác =====
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // ===== Thanh menu trên cùng của frmRecipes =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes (đang ở Recipes rồi -> không làm gì)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Đang ở Recipes, không cần chuyển
        }

        // Favorites
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var f = new frmMyFavRecipes();
            OpenForm(f);
        }

        // Collections
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            var f = new frmCollection();
            OpenForm(f);
        }

        // Profiles
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var f = new frmProfile();
            OpenForm(f);
        }

        // ====== HÀM LOAD DANH SÁCH CÔNG THỨC LÊN GRID (CÓ TÌM KIẾM + YÊU THÍCH) ======
        private void LoadRecipes(string? keyword = null)
        {
            // Xoá các item cũ để hiển thị dữ liệu thật
            flowLayoutPanel1.Controls.Clear();

            // Lấy user hiện tại (nếu đã đăng nhập)
            int? currentUserId = null;
            if (AuthManager.IsLoggedIn && AuthManager.CurrentUser != null)
            {
                // NOTE: nếu model User của bạn đặt tên property khác (vd: UserID)
                // thì sửa lại dòng dưới cho đúng:
                currentUserId = AuthManager.CurrentUser.UserId;
            }

            try
            {
                using (var conn = new SqlConnection(DBHelper.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    // SQL cơ bản
                    string sql = @"
SELECT 
    r.recipe_id,
    r.title,
    r.difficulty,
    r.time_needed,
    c.name      AS category,
    u.username  AS author,
    r.image_url,
    CASE 
        WHEN f.user_id IS NULL THEN 0 
        ELSE 1 
    END AS is_favorite
FROM Recipes r
JOIN Users u ON r.user_id = u.user_id
LEFT JOIN Categories c ON r.category_id = c.category_id
LEFT JOIN Favorites f 
    ON f.recipe_id = r.recipe_id AND f.user_id = @uid
";

                    // Nếu có keyword -> thêm WHERE lọc theo title
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " WHERE r.title LIKE @keyword";
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                    }

                    // Sắp xếp món mới nhất lên trước
                    sql += " ORDER BY r.created_at DESC";

                    cmd.CommandText = sql;

                    // Truyền @uid (có thể null -> không có bản ghi nào trong Favorites)
                    if (currentUserId.HasValue)
                        cmd.Parameters.AddWithValue("@uid", currentUserId.Value);
                    else
                        cmd.Parameters.AddWithValue("@uid", DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var card = BuildRecipeCard(reader);
                            flowLayoutPanel1.Controls.Add(card);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách công thức: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Tạo 1 thẻ (card) hiển thị 1 công thức
        private Control BuildRecipeCard(SqlDataReader reader)
        {
            // --- panel chứa cả card ---
            var card = new Panel();
            card.Width = 240;
            card.Height = 323;
            card.Margin = new Padding(10);
            card.BackColor = Color.Transparent;
            card.Cursor = Cursors.Hand;

            int recipeId = Convert.ToInt32(reader["recipe_id"]);
            card.Tag = recipeId;

            bool isFavorite = false;
            if (reader["is_favorite"] != DBNull.Value)
            {
                isFavorite = Convert.ToInt32(reader["is_favorite"]) == 1;
            }

            // --- hình món ăn ---
            var pic = new PictureBox();
            pic.Location = new Point(0, 0);
            pic.Size = new Size(card.Width, 199);
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            pic.Tag = recipeId;

            try
            {
                string? imageUrl = reader["image_url"] as string;

                if (!string.IsNullOrWhiteSpace(imageUrl) && File.Exists(imageUrl))
                {
                    pic.Image = Image.FromFile(imageUrl);
                }
                else
                {
                    // fallback: dùng hình mẫu đã set cho pictureBox1 trong Designer
                    pic.Image = pictureBox1.Image;
                }
            }
            catch
            {
                pic.Image = pictureBox1.Image;
            }

            // --- panel thông tin bên dưới ---
            var infoPanel = new Panel();
            infoPanel.BackColor = Color.White;
            infoPanel.Location = new Point(0, 199);
            infoPanel.Size = new Size(card.Width, 120);

            // Title
            var lblTitle = new Label();
            lblTitle.Font = label2.Font; // dùng font mẫu
            lblTitle.AutoSize = false;
            lblTitle.Width = infoPanel.Width - 40; // chừa chỗ cho icon ♥
            lblTitle.Location = new Point(10, -2);
            lblTitle.Text = reader["title"].ToString();
            lblTitle.Tag = recipeId;

            // Author caption
            var lblAuthorCaption = new Label();
            lblAuthorCaption.Font = label3.Font;
            lblAuthorCaption.AutoSize = true;
            lblAuthorCaption.Location = new Point(10, 23);
            lblAuthorCaption.Text = "Author :";

            // Author
            var lblAuthor = new Label();
            lblAuthor.Font = label4.Font;
            lblAuthor.AutoSize = true;
            lblAuthor.Location = new Point(80, 23);
            lblAuthor.Text = reader["author"].ToString();
            lblAuthor.Tag = recipeId;

            // Time caption
            var lblTimeCaption = new Label();
            lblTimeCaption.Font = label5.Font;
            lblTimeCaption.AutoSize = true;
            lblTimeCaption.Location = new Point(10, 47);
            lblTimeCaption.Text = "Time :";

            // Time value
            var lblTime = new Label();
            lblTime.Font = label6.Font;
            lblTime.AutoSize = true;
            lblTime.Location = new Point(80, 47);
            lblTime.Text = reader["time_needed"]?.ToString();
            lblTime.Tag = recipeId;

            // Category caption
            var lblCateCaption = new Label();
            lblCateCaption.Font = label7.Font;
            lblCateCaption.AutoSize = true;
            lblCateCaption.Location = new Point(10, 68);
            lblCateCaption.Text = "Cate :";

            // Category value
            var lblCate = new Label();
            lblCate.Font = label8.Font;
            lblCate.AutoSize = true;
            lblCate.Location = new Point(80, 68);
            lblCate.Text = reader["category"] == DBNull.Value
                ? "-"
                : reader["category"].ToString();
            lblCate.Tag = recipeId;

            // Level caption
            var lblLevelCaption = new Label();
            lblLevelCaption.Font = label44.Font;
            lblLevelCaption.AutoSize = true;
            lblLevelCaption.Location = new Point(10, 91);
            lblLevelCaption.Text = "Level : ";

            // Level value
            var lblLevel = new Label();
            lblLevel.Font = label45.Font;
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(80, 91);
            lblLevel.Text = reader["difficulty"] == DBNull.Value
                ? "-"
                : reader["difficulty"].ToString();
            lblLevel.Tag = recipeId;

            // ===== Icon YÊU THÍCH (♥ / ♡) =====
            var favLabel = new Label();
            favLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            favLabel.AutoSize = true;
            favLabel.ForeColor = Color.Red;
            favLabel.Text = isFavorite ? "♥" : "♡";
            favLabel.Location = new Point(infoPanel.Width - 30, 5);
            favLabel.Cursor = Cursors.Hand;
            favLabel.Tag = recipeId;
            favLabel.Click += FavLabel_Click;

            // add các label vào infoPanel
            infoPanel.Controls.Add(lblTitle);
            infoPanel.Controls.Add(lblAuthorCaption);
            infoPanel.Controls.Add(lblAuthor);
            infoPanel.Controls.Add(lblTimeCaption);
            infoPanel.Controls.Add(lblTime);
            infoPanel.Controls.Add(lblCateCaption);
            infoPanel.Controls.Add(lblCate);
            infoPanel.Controls.Add(lblLevelCaption);
            infoPanel.Controls.Add(lblLevel);
            infoPanel.Controls.Add(favLabel);

            // click ở đâu trên card cũng chọn được công thức (sau dùng cho xem chi tiết)
            card.Click += RecipeCard_Click;
            pic.Click += RecipeCard_Click;
            lblTitle.Click += RecipeCard_Click;
            lblAuthor.Click += RecipeCard_Click;
            lblTime.Click += RecipeCard_Click;
            lblCate.Click += RecipeCard_Click;
            lblLevel.Click += RecipeCard_Click;

            // ghép vào card
            card.Controls.Add(pic);
            card.Controls.Add(infoPanel);

            return card;
        }

        // Click card: tạm thời chỉ báo ID (sau này có thể mở frmRecipeDetails)
        private void RecipeCard_Click(object? sender, EventArgs e)
        {
            if (sender is Control c && c.Tag != null)
            {
                int recipeId = Convert.ToInt32(c.Tag);
                MessageBox.Show("Bạn đã chọn công thức ID = " + recipeId,
                    "Recipe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ====== CLICK ICON YÊU THÍCH (♥ / ♡) ======
        private void FavLabel_Click(object? sender, EventArgs e)
        {
            if (sender is not Label lbl || lbl.Tag == null)
                return;

            if (!AuthManager.IsLoggedIn || AuthManager.CurrentUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập để dùng chức năng Yêu thích.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int recipeId = Convert.ToInt32(lbl.Tag);

            // NOTE: nếu User của bạn dùng tên property khác thì sửa dòng dưới
            int userId = AuthManager.CurrentUser.UserId;

            bool isCurrentlyFavorite = lbl.Text == "♥";

            try
            {
                using (var conn = new SqlConnection(DBHelper.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    if (isCurrentlyFavorite)
                    {
                        // Đang là favorite -> bỏ thích
                        cmd.CommandText = @"
DELETE FROM Favorites 
WHERE user_id = @uid AND recipe_id = @rid";
                    }
                    else
                    {
                        // Chưa thích -> thêm mới
                        cmd.CommandText = @"
IF NOT EXISTS (
    SELECT 1 FROM Favorites WHERE user_id = @uid AND recipe_id = @rid
)
BEGIN
    INSERT INTO Favorites(user_id, recipe_id) 
    VALUES(@uid, @rid);
END";
                    }

                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@rid", recipeId);

                    cmd.ExecuteNonQuery();
                }

                // Đổi icon trên UI
                lbl.Text = isCurrentlyFavorite ? "♡" : "♥";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật Yêu thích: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====== CÁC EVENT CŨ (GIỮ NGUYÊN) ======

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {

        }

        private void label45_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
        }

        private void panel12_Paint(object sender, PaintEventArgs e)
        {

        }

        // Khi form load, gọi LoadRecipes()
        private void frmRecipes_Load(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            // Lấy keyword từ ô search
            string keyword = guna2TextBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Không nhập gì -> load toàn bộ
                LoadRecipes();
            }
            else
            {
                // Có từ khoá -> load theo tên món
                LoadRecipes(keyword);
            }
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // Mở form thêm / sửa công thức
            using (var f = new frmAddRecipie())   // ✅ ĐÚNG TÊN FORM
            {
                f.ShowDialog();   // mở modal, nhập xong bấm Add sẽ đóng form
            }

            // Sau khi form đóng, load lại danh sách để thấy recipe mới
            LoadRecipes();
        }
    }
}
