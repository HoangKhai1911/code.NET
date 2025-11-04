using Guna.UI2.WinForms;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmRecipeDetails : Form
    {
        private int recipeId;

        public frmRecipeDetails(int id)
        {
            InitializeComponent();
            recipeId = id;

            // Gọi load dữ liệu khi form hiển thị
            this.Load += (s, e) => LoadDetails();

            // Nút chỉnh sửa công thức
            guna2Button1.Click += (s, e) =>
            {
                var editForm = new frmAddRecipie(recipeId);
                editForm.ShowDialog();
                LoadDetails(); // Refresh lại dữ liệu
            };

            // Nút xoá công thức
            guna2Button2.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xoá công thức này?",
                                    "Xác nhận", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    using (var conn = new SqlConnection(DBHelper.ConnectionString))
                    {
                        conn.Open();

                        // Xoá trước các bản ghi phụ thuộc (nếu có ràng buộc)
                        var cmd1 = new SqlCommand("DELETE FROM Ingredients WHERE recipe_id=@id", conn);
                        cmd1.Parameters.AddWithValue("@id", recipeId);
                        cmd1.ExecuteNonQuery();

                        var cmd2 = new SqlCommand("DELETE FROM Steps WHERE recipe_id=@id", conn);
                        cmd2.Parameters.AddWithValue("@id", recipeId);
                        cmd2.ExecuteNonQuery();

                        // Sau đó xoá công thức chính
                        var cmd3 = new SqlCommand("DELETE FROM Recipes WHERE recipe_id=@id", conn);
                        cmd3.Parameters.AddWithValue("@id", recipeId);
                        cmd3.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đã xoá công thức thành công!", "Thông báo");
                    this.Close();
                }
            };
        }

        private void LoadDetails()
        {
            using (var conn = new SqlConnection(DBHelper.ConnectionString))
            {
                conn.Open();

                // 1️⃣ Lấy thông tin cơ bản của món ăn
                var cmd = new SqlCommand("GetRecipeDetail", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@recipe_id", recipeId);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    guna2HtmlLabel3.Text = reader["title"].ToString();                // Tên món
                    guna2HtmlLabel5.Text = reader["author"].ToString();               // Tác giả
                    guna2HtmlLabel7.Text = reader["category"].ToString();             // Danh mục
                    guna2HtmlLabel11.Text = reader["time_needed"]?.ToString() ?? "N/A"; // Thời gian
                    guna2HtmlLabel9.Text = reader["difficulty"]?.ToString() ?? "Medium"; // Độ khó

                    string imagePath = reader["image_url"].ToString();
                    if (System.IO.File.Exists(imagePath))
                        pictureBox1.ImageLocation = imagePath;
                }
                reader.Close();

                // 2️⃣ Nguyên liệu
                var daIngredients = new SqlDataAdapter(
                    "SELECT name, quantity FROM Ingredients WHERE recipe_id=@id", conn);
                daIngredients.SelectCommand.Parameters.AddWithValue("@id", recipeId);

                var dtIngredients = new DataTable();
                daIngredients.Fill(dtIngredients);
                guna2HtmlLabel12.Text = dtIngredients.Rows.Count > 0
                    ? string.Join("\n", dtIngredients.AsEnumerable().Select(x => $"{x["name"]} - {x["quantity"]}"))
                    : "(Không có nguyên liệu)";

                // 3️⃣ Các bước thực hiện
                var daSteps = new SqlDataAdapter(
                    "SELECT step_number, instruction FROM Steps WHERE recipe_id=@id ORDER BY step_number", conn);
                daSteps.SelectCommand.Parameters.AddWithValue("@id", recipeId);

                var dtSteps = new DataTable();
                daSteps.Fill(dtSteps);
                guna2HtmlLabel15.Text = dtSteps.Rows.Count > 0
                    ? string.Join("\n\n", dtSteps.AsEnumerable().Select(x => $"{x["step_number"]}. {x["instruction"]}"))
                    : "(Chưa có bước làm)";
            }
        }
    }
}
