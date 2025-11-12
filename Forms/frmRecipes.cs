using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;      //  dùng SqlConnection, SqlCommand
using System.IO;                 // check tồn tại file ảnh

namespace WinCook
{
    public partial class frmRecipes : Form
    {
        public frmRecipes()
        {
            InitializeComponent();
        }

        // ====== HÀM LOAD DANH SÁCH CÔNG THỨC LÊN GRID ======
        private void LoadRecipes()
        {
            // Xoá các item cũ (panel1, panel3… mẫu trong Designer) để hiển thị dữ liệu thật
            flowLayoutPanel1.Controls.Clear();

            try
            {
                using (var conn = new SqlConnection(DBHelper.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            r.recipe_id,
                            r.title,
                            r.difficulty,
                            r.time_needed,
                            c.name      AS category,
                            u.username  AS author,
                            r.image_url
                        FROM Recipes r
                        JOIN Users u ON r.user_id = u.user_id
                        LEFT JOIN Categories c ON r.category_id = c.category_id
                        ORDER BY r.created_at DESC";

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
            card.Width = 275;
            card.Height = 323;
            card.Margin = new Padding(10);
            card.BackColor = Color.Transparent;
            card.Cursor = Cursors.Hand;

            int recipeId = Convert.ToInt32(reader["recipe_id"]);
            card.Tag = recipeId;

            // --- hình món ăn ---
            var pic = new PictureBox();
            pic.Location = new Point(0, 0);
            pic.Size = new Size(275, 199);
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
            infoPanel.Size = new Size(275, 120);

            // Title
            var lblTitle = new Label();
            lblTitle.Font = label2.Font; // dùng font mẫu
            lblTitle.AutoSize = false;
            lblTitle.Width = infoPanel.Width - 20;
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

            // click ở đâu trên card cũng chọn được công thức
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

        // Tạm thời: click card chỉ show ID, sau này ta mở frmRecipeDetails
        private void RecipeCard_Click(object? sender, EventArgs e)
        {
            if (sender is Control c && c.Tag != null)
            {
                int recipeId = Convert.ToInt32(c.Tag);
                MessageBox.Show("Bạn đã chọn công thức ID = " + recipeId,
                    "Recipe", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }
    }
}
