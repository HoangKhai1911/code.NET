using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using WinCook.Services;   // dùng AuthManager

namespace WinCook
{
    public partial class frmMyFavRecipes : Form
    {
        public frmMyFavRecipes()
        {
            InitializeComponent();
        }

        // Helper mở form khác
        private void OpenForm(Form f)
        {
            f.Show();
            this.Hide();
        }

        // ===== Thanh menu top =====

        // Home
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var f = new frmHomePage();
            OpenForm(f);
        }

        // Recipes
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var f = new frmRecipes();
            OpenForm(f);
        }

        // Favorites (đang ở đây nên không làm gì)
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            // no-op
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

        // ====== LOAD DANH SÁCH YÊU THÍCH ======
        private void LoadFavorites(string? keyword = null)
        {
            flowLayoutPanel1.Controls.Clear();

            if (!AuthManager.IsLoggedIn || AuthManager.CurrentUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem danh sách yêu thích.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // NOTE: sửa lại property nếu User của bạn tên khác
            int userId = AuthManager.CurrentUser.UserId;

            try
            {
                using (var conn = new SqlConnection(DBHelper.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    string sql = @"
SELECT
    r.recipe_id,
    r.title,
    r.difficulty,
    r.time_needed,
    c.name      AS category,
    u.username  AS author,
    r.image_url
FROM Favorites f
JOIN Recipes r ON f.recipe_id = r.recipe_id
JOIN Users   u ON r.user_id = u.user_id
LEFT JOIN Categories c ON r.category_id = c.category_id
WHERE f.user_id = @uid
";

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " AND r.title LIKE @keyword";
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                    }

                    sql += " ORDER BY f.created_at DESC";

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@uid", userId);

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
                MessageBox.Show("Lỗi khi tải danh sách công thức yêu thích: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Tạo 1 card cho 1 công thức yêu thích
        private Control BuildRecipeCard(SqlDataReader reader)
        {
            var card = new Panel();
            card.Width = 240;
            card.Height = 323;
            card.Margin = new Padding(10);
            card.BackColor = Color.Transparent;
            card.Cursor = Cursors.Hand;

            int recipeId = Convert.ToInt32(reader["recipe_id"]);
            card.Tag = recipeId;

            // ----- Hình món ăn -----
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
                    // fallback: dùng hình mẫu nếu có sẵn trong Designer
                    if (pictureBox1 != null && pictureBox1.Image != null)
                        pic.Image = pictureBox1.Image;
                }
            }
            catch
            {
                if (pictureBox1 != null && pictureBox1.Image != null)
                    pic.Image = pictureBox1.Image;
            }

            // ----- Panel info bên dưới -----
            var infoPanel = new Panel();
            infoPanel.BackColor = Color.White;
            infoPanel.Location = new Point(0, 199);
            infoPanel.Size = new Size(card.Width, 120);

            // Title
            var lblTitle = new Label();
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.AutoSize = false;
            lblTitle.Width = infoPanel.Width - 40;
            lblTitle.Location = new Point(10, -2);
            lblTitle.Text = reader["title"].ToString();
            lblTitle.Tag = recipeId;

            // Author caption
            var lblAuthorCaption = new Label();
            lblAuthorCaption.Font = new Font("Segoe UI Semibold", 10);
            lblAuthorCaption.AutoSize = true;
            lblAuthorCaption.Location = new Point(10, 23);
            lblAuthorCaption.Text = "Author :";

            // Author
            var lblAuthor = new Label();
            lblAuthor.Font = new Font("Segoe UI Semibold", 10);
            lblAuthor.AutoSize = true;
            lblAuthor.Location = new Point(80, 23);
            lblAuthor.Text = reader["author"].ToString();
            lblAuthor.Tag = recipeId;

            // Time
            var lblTimeCaption = new Label();
            lblTimeCaption.Font = new Font("Segoe UI Semibold", 10);
            lblTimeCaption.AutoSize = true;
            lblTimeCaption.Location = new Point(10, 47);
            lblTimeCaption.Text = "Time :";

            var lblTime = new Label();
            lblTime.Font = new Font("Segoe UI Semibold", 10);
            lblTime.AutoSize = true;
            lblTime.Location = new Point(80, 47);
            lblTime.Text = reader["time_needed"]?.ToString();
            lblTime.Tag = recipeId;

            // Category
            var lblCateCaption = new Label();
            lblCateCaption.Font = new Font("Segoe UI Semibold", 10);
            lblCateCaption.AutoSize = true;
            lblCateCaption.Location = new Point(10, 68);
            lblCateCaption.Text = "Cate :";

            var lblCate = new Label();
            lblCate.Font = new Font("Segoe UI Semibold", 10);
            lblCate.AutoSize = true;
            lblCate.Location = new Point(80, 68);
            lblCate.Text = reader["category"] == DBNull.Value
                ? "-"
                : reader["category"].ToString();
            lblCate.Tag = recipeId;

            // Level
            var lblLevelCaption = new Label();
            lblLevelCaption.Font = new Font("Segoe UI Semibold", 10);
            lblLevelCaption.AutoSize = true;
            lblLevelCaption.Location = new Point(10, 91);
            lblLevelCaption.Text = "Level : ";

            var lblLevel = new Label();
            lblLevel.Font = new Font("Segoe UI Semibold", 10);
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(80, 91);
            lblLevel.Text = reader["difficulty"] == DBNull.Value
                ? "-"
                : reader["difficulty"].ToString();
            lblLevel.Tag = recipeId;

            // Icon ♥ để bỏ khỏi yêu thích nếu muốn
            var favLabel = new Label();
            favLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            favLabel.AutoSize = true;
            favLabel.ForeColor = Color.Red;
            favLabel.Text = "♥"; // ở trang Favorites toàn bộ đều đang được yêu thích
            favLabel.Location = new Point(infoPanel.Width - 30, 5);
            favLabel.Cursor = Cursors.Hand;
            favLabel.Tag = recipeId;
            favLabel.Click += FavLabel_Click;

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

            // click card => tạm thời chỉ show ID
            card.Click += Card_Click;
            pic.Click += Card_Click;
            lblTitle.Click += Card_Click;
            lblAuthor.Click += Card_Click;
            lblTime.Click += Card_Click;
            lblCate.Click += Card_Click;
            lblLevel.Click += Card_Click;

            card.Controls.Add(pic);
            card.Controls.Add(infoPanel);

            return card;
        }

        private void Card_Click(object? sender, EventArgs e)
        {
            if (sender is Control c && c.Tag != null)
            {
                int recipeId = Convert.ToInt32(c.Tag);
                MessageBox.Show("Công thức yêu thích ID = " + recipeId,
                    "Favorite", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Click icon ♥ ở trang Favorites => bỏ yêu thích và reload
        private void FavLabel_Click(object? sender, EventArgs e)
        {
            if (sender is not Label lbl || lbl.Tag == null)
                return;

            if (!AuthManager.IsLoggedIn || AuthManager.CurrentUser == null)
                return;

            int recipeId = Convert.ToInt32(lbl.Tag);
            int userId = AuthManager.CurrentUser.UserId;

            try
            {
                using (var conn = new SqlConnection(DBHelper.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
DELETE FROM Favorites 
WHERE user_id = @uid AND recipe_id = @rid";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@rid", recipeId);
                    cmd.ExecuteNonQuery();
                }

                // Reload lại danh sách favorites
                LoadFavorites(guna2TextBox1.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi bỏ yêu thích: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Form load
        private void frmMyFavRecipes_Load(object sender, EventArgs e)
        {
            LoadFavorites();
        }

        // Nút Search trên form Favorites (nếu có)
        private void guna2Button6_Click(object sender, EventArgs e)
        {
            string keyword = guna2TextBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
                LoadFavorites();
            else
                LoadFavorites(keyword);
        }
    }
}
