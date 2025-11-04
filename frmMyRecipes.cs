using Microsoft.Data.SqlClient;
using System.Data;

namespace WinCook
{
    public partial class frmMyRecipes : Form
    {
        private readonly string connectionString = "Data Source=.;Initial Catalog=WinCook;Integrated Security=True";
        private readonly int currentUserId = 1; // sau này thay bằng user đăng nhập thực tế

        public frmMyRecipes()
        {
            InitializeComponent();

            // Khi form load thì hiển thị danh sách công thức
            this.Load += (s, e) => LoadMyRecipes();

            // Gán sự kiện cho nút thêm công thức
            guna2Button7.Click += guna2Button7_Click;
        }

        // 🟢 Hàm hiển thị danh sách công thức
        private void LoadMyRecipes()
        {
            flowLayoutPanel1.Controls.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(@"
                        SELECT r.recipe_id, r.title, r.image_url, c.name AS category_name
                        FROM Recipes r
                        LEFT JOIN Categories c ON r.category_id = c.category_id
                        WHERE r.user_id = @uid
                        ORDER BY r.updated_at DESC", conn);

                    da.SelectCommand.Parameters.AddWithValue("@uid", currentUserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
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

                    foreach (DataRow row in dt.Rows)
                    {
                        int recipeId = Convert.ToInt32(row["recipe_id"]);
                        string title = row["title"].ToString();
                        string category = row["category_name"]?.ToString() ?? "Không có danh mục";
                        string imagePath = row["image_url"]?.ToString();

                        // 🟫 Tạo panel card
                        Panel card = new Panel
                        {
                            Width = 260,
                            Height = 330,
                            Margin = new Padding(10),
                            BorderStyle = BorderStyle.FixedSingle,
                            BackColor = Color.WhiteSmoke
                        };

                        // 🟦 Hình món ăn
                        PictureBox pic = new PictureBox
                        {
                            Width = 260,
                            Height = 160,
                            Dock = DockStyle.Top,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            BackColor = Color.White
                        };
                        try
                        {
                            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                                pic.Image = Image.FromFile(imagePath);
                            else
                                pic.Image = Properties.Resources.no_image;
                        }
                        catch
                        {
                            pic.Image = Properties.Resources.no_image;
                        }

                        // 🟨 Tiêu đề món ăn
                        Label lblTitle = new Label
                        {
                            Text = title,
                            Dock = DockStyle.Top,
                            Height = 40,
                            Font = new Font("Segoe UI", 10, FontStyle.Bold),
                            TextAlign = ContentAlignment.MiddleCenter,
                            ForeColor = Color.Black
                        };

                        // 🟩 Danh mục
                        Label lblCategory = new Label
                        {
                            Text = "Danh mục: " + category,
                            Dock = DockStyle.Top,
                            Height = 25,
                            Font = new Font("Segoe UI", 9, FontStyle.Italic),
                            TextAlign = ContentAlignment.MiddleCenter,
                            ForeColor = Color.DimGray
                        };

                        // 🔵 Nút chi tiết
                        Button btnView = new Button
                        {
                            Text = "Chi tiết",
                            Height = 30,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.LightSteelBlue,
                            FlatStyle = FlatStyle.Flat
                        };
                        btnView.Click += (s, e) =>
                        {
                            frmRecipeDetails detailForm = new frmRecipeDetails(recipeId);
                            detailForm.ShowDialog();
                        };

                        // 🟢 Nút sửa
                        Button btnEdit = new Button
                        {
                            Text = "Edit",
                            Height = 30,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.LightGreen,
                            FlatStyle = FlatStyle.Flat
                        };
                        btnEdit.Click += (s, e) =>
                        {
                            frmAddRecipie editForm = new frmAddRecipie(recipeId);
                            editForm.ShowDialog();
                            LoadMyRecipes();
                        };

                        // 🔴 Nút xóa
                        Button btnDelete = new Button
                        {
                            Text = "Delete",
                            Height = 30,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.LightCoral,
                            FlatStyle = FlatStyle.Flat
                        };
                        btnDelete.Click += (s, e) =>
                        {
                            if (MessageBox.Show("Bạn có chắc muốn xóa công thức này?", "Xác nhận xóa",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                DeleteRecipe(recipeId);
                                LoadMyRecipes();
                            }
                        };

                        // 🧩 Thêm các control vào card
                        card.Controls.Add(btnDelete);
                        card.Controls.Add(btnEdit);
                        card.Controls.Add(btnView);
                        card.Controls.Add(lblCategory);
                        card.Controls.Add(lblTitle);
                        card.Controls.Add(pic);

                        // 🧊 Thêm card vào danh sách
                        flowLayoutPanel1.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách công thức: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🗑️ Xóa công thức
        private void DeleteRecipe(int recipeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Recipes WHERE recipe_id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", recipeId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Đã xóa công thức!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ➕ Nút thêm công thức mới
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            frmAddRecipie addForm = new frmAddRecipie();
            addForm.ShowDialog();
            LoadMyRecipes();
        }

        // ⚙️ Các nút khác (chưa dùng)
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
