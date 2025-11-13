//Forms/frmAdd-EditRecipie.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using WinCook.Services;
using WinCook.Models;

namespace WinCook
{
    public partial class frmAddRecipie : Form
    {
        private int? recipeId = null; // null = thêm mới, khác null = chỉnh sửa
        private int currentUserId = 3; // sau này thay bằng AuthManager.CurrentUser.UserId
        private string connectionString = DBHelper.ConnectionString;
        private string imagePath = null;


        public frmAddRecipie(int? id = null)
        {
            InitializeComponent();
            recipeId = id;

            // Load danh sách combobox
            LoadLevels();
            LoadCategories();

                      // nút xóa ảnh

            if (recipeId != null)
            {
                guna2HtmlLabel3.Text = "Chỉnh sửa công thức";
                guna2Button1.Text = "Lưu thay đổi";
                LoadRecipeForEdit();
            }
            else
            {
                guna2HtmlLabel3.Text = "Thêm công thức mới";
                guna2Button1.Text = "Thêm";
            }
        }

        private void LoadRecipeForEdit()
        {
            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(DBHelper.ConnectionString)
)
            {
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                    SELECT r.*, c.name AS category_name
                    FROM Recipes r
                    LEFT JOIN Categories c ON r.category_id = c.category_id
                    WHERE r.recipe_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", recipeId);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    guna2TextBox1.Text = reader["title"].ToString();
                    guna2TextBox2.Text = reader["time_needed"].ToString();
                    comboBox1.Text = reader["difficulty"].ToString();
                    comboBox2.Text = reader["category_name"]?.ToString();
                    richTextBox1.Text = reader["ingredients"].ToString();
                    richTextBox2.Text = reader["steps"].ToString();

                    string imgPath = reader["image_url"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                    {
                        guna2PictureBox1.Image = Image.FromFile(imgPath);
                        guna2PictureBox1.Tag = imgPath;
                    }
                }
                reader.Close();
            }
        }

        // 🟢 Load danh mục
        private void LoadCategories()
        {
            try
            {
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(DBHelper.ConnectionString)
)
                {
                    conn.Open();
                    var da = new Microsoft.Data.SqlClient.SqlDataAdapter("SELECT name FROM Categories", conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    comboBox2.DataSource = dt;
                    comboBox2.DisplayMember = "name";
                    comboBox2.ValueMember = "name";
                }
            }
            catch
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Khác");
                comboBox2.SelectedIndex = 0;
            }
        }

        // 🟢 Load độ khó
        private void LoadLevels()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new string[] { "Dễ", "Trung bình", "Khó" });
            comboBox1.SelectedIndex = 0;
        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string title = guna2TextBox1.Text.Trim();
            string timeNeeded = guna2TextBox2.Text.Trim();
            string difficulty = comboBox1.Text;
            string categoryName = comboBox2.Text; // chỉ gửi tên danh mục
            string ingredients = richTextBox1.Text.Trim();
            string steps = richTextBox2.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Vui lòng nhập tên món ăn!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sao chép ảnh vào thư mục /Images
            string savedImagePath = null;
            if (guna2PictureBox1.Tag != null)
            {
                string src = guna2PictureBox1.Tag.ToString();
                string destFolder = Path.Combine(Application.StartupPath, "Images");
                Directory.CreateDirectory(destFolder);
                string dest = Path.Combine(destFolder, Path.GetFileName(src));
                File.Copy(src, dest, true);
                savedImagePath = dest;
            }

            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(DBHelper.ConnectionString)
)
            {
                conn.Open();
                Microsoft.Data.SqlClient.SqlCommand cmd;

                if (recipeId == null)
                {
                    // ➕ Thêm mới (gọi stored procedure AddRecipe)
                    cmd = new Microsoft.Data.SqlClient.SqlCommand("AddRecipe", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@user_id", currentUserId);
                    cmd.Parameters.AddWithValue("@category_name", (object)categoryName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@difficulty", (object)difficulty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@time_needed", (object)timeNeeded ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ingredients", (object)ingredients ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@steps", (object)steps ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@image_url", (object)savedImagePath ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // ✏️ Chỉnh sửa công thức
                    cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                        DECLARE @category_id INT;
                        SELECT @category_id = category_id FROM Categories WHERE name = @catName;
                        IF @category_id IS NULL
                        BEGIN
                            INSERT INTO Categories (name) VALUES (@catName);
                            SET @category_id = SCOPE_IDENTITY();
                        END;
                        UPDATE Recipes SET
                            title = @title,
                            difficulty = @diff,
                            time_needed = @time,
                            ingredients = @ing,
                            steps = @steps,
                            image_url = @img,
                            category_id = @category_id,
                            updated_at = GETDATE()
                        WHERE recipe_id = @id;
                    ", conn);

                    cmd.Parameters.AddWithValue("@id", recipeId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@diff", difficulty);
                    cmd.Parameters.AddWithValue("@time", timeNeeded);
                    cmd.Parameters.AddWithValue("@ing", ingredients);
                    cmd.Parameters.AddWithValue("@steps", steps);
                    cmd.Parameters.AddWithValue("@img", (object)savedImagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@catName", categoryName);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(recipeId == null ? "Đã thêm công thức thành công!" : "Đã cập nhật công thức!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image files|*.jpg;*.jpeg;*.png;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath = ofd.FileName;
                    guna2PictureBox1.ImageLocation = imagePath;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                guna2PictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                guna2PictureBox1.Tag = openFileDialog1.FileName;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            guna2PictureBox1.Image = null;
            guna2PictureBox1.Tag = null;
            imagePath = null;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
