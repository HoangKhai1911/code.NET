//frmAdd-EditRecipie.cs
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
// using System.Data.SqlClient; // <-- Bỏ đi, Service sẽ lo
using WinCook.Services;
using WinCook.Models;
// using Microsoft.Data.SqlClient; // <-- Bỏ đi, Service sẽ lo

namespace WinCook
{
    public partial class frmAddRecipie : Form
    {
        private int? recipeId = null; // null = thêm mới, khác null = chỉnh sửa
        private int currentUserId; // Sẽ được lấy từ AuthManager
                                   // private string connectionString = DBHelper.ConnectionString; // Bỏ đi
                                   // private string imagePath = null; // Bỏ đi, dùng Tag của PictureBox

        // === KHAI BÁO SERVICE ===
        private readonly RecipeService _recipeService;

        public frmAddRecipie(int? id = null)
        {
            InitializeComponent();

            _recipeService = new RecipeService(); // Khởi tạo Service
            recipeId = id;

            // Kiểm tra đăng nhập
            if (!AuthManager.IsLoggedIn)
            {
                MessageBox.Show("Bạn phải đăng nhập để thêm/sửa công thức.");
                this.Close();
                return;
            }
            currentUserId = AuthManager.CurrentUser.UserId; // Lấy ID người dùng

            // Load danh sách combobox
            LoadLevels();
            LoadCategories(); // Nâng cấp hàm này

            if (recipeId != null)
            {
                guna2HtmlLabel3.Text = "Chỉnh sửa công thức";
                guna2Button1.Text = "Lưu thay đổi";
                LoadRecipeForEdit(); // Nâng cấp hàm này
            }
            else
            {
                guna2HtmlLabel3.Text = "Thêm công thức mới";
                guna2Button1.Text = "Thêm";
            }
        }

        /// <summary>
        /// NÂNG CẤP: Dùng RecipeService
        /// </summary>
        private void LoadRecipeForEdit()
        {
            // Gọi Service thay vì gọi CSDL trực tiếp
            Recipe recipe = _recipeService.GetRecipeDetails(recipeId.Value);

            if (recipe != null)
            {
                guna2TextBox1.Text = recipe.Title;
                guna2TextBox2.Text = recipe.TimeNeeded;
                comboBox1.Text = recipe.Difficulty;
                comboBox2.Text = recipe.CategoryName;
                richTextBox1.Text = recipe.Ingredients; // Khớp với CSDL
                richTextBox2.Text = recipe.Steps;       // Khớp với CSDL

                string imgPath = recipe.ImageUrl;
                if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                {
                    guna2PictureBox1.Image = Image.FromFile(imgPath);
                    guna2PictureBox1.Tag = imgPath; // Lưu đường dẫn gốc vào Tag
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy công thức để chỉnh sửa.");
                this.Close();
            }
        }

        /// <summary>
        /// NÂNG CẤP: Dùng RecipeService
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                // Gọi Service
                List<Category> categories = _recipeService.GetCategories();

                // Thêm 1 lựa chọn "Khác" (hoặc "Chọn danh mục")
                categories.Insert(0, new Category { CategoryId = 0, Name = "(Chọn danh mục)" });

                comboBox2.DataSource = categories;
                comboBox2.DisplayMember = "Name";
                comboBox2.ValueMember = "Name"; // Dùng 'Name' vì SP 'AddRecipe' của bạn nhận 'category_name'
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        // 🟢 Load độ khó (Giữ nguyên)
        private void LoadLevels()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new string[] { "Dễ", "Trung bình", "Khó" });
            comboBox1.SelectedIndex = 0;
        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// NÂNG CẤP: Dùng RecipeService (Nút Thêm/Lưu)
        /// </summary>
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // 1. Validate cơ bản
            string title = guna2TextBox1.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Vui lòng nhập tên món ăn!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Xử lý Category
            // Nếu chọn mục đầu tiên "(Chọn danh mục)" hoặc để trống -> Gán null hoặc mặc định "Khác"
            string categoryName = comboBox2.Text.Trim();
            if (comboBox2.SelectedIndex == 0 || string.IsNullOrEmpty(categoryName))
            {
                categoryName = "Khác"; // Hoặc null tùy bạn
            }

            // 3. Xử lý Ảnh (Quan trọng)
            string finalImagePath = null;

            // Kiểm tra Tag (Nơi lưu đường dẫn ảnh gốc khi chọn file mới hoặc load từ DB)
            if (guna2PictureBox1.Tag != null)
            {
                string sourcePath = guna2PictureBox1.Tag.ToString();

                // Trường hợp 1: Ảnh đã nằm trong thư mục ứng dụng (Do load từ DB khi Sửa)
                if (sourcePath.Contains(Application.StartupPath))
                {
                    finalImagePath = sourcePath; // Giữ nguyên
                }
                // Trường hợp 2: Ảnh mới chọn từ máy tính (Cần copy vào)
                else if (File.Exists(sourcePath))
                {
                    try
                    {
                        string destFolder = Path.Combine(Application.StartupPath, "Images");
                        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                        // Tạo tên file duy nhất
                        string destFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourcePath);
                        string destPath = Path.Combine(destFolder, destFileName);

                        File.Copy(sourcePath, destPath, true);
                        finalImagePath = destPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu ảnh: " + ex.Message);
                        // Nếu lỗi copy, vẫn cho lưu công thức nhưng không có ảnh
                    }
                }
            }

            // 4. Tạo đối tượng Recipe
            Recipe recipe = new Recipe
            {
                UserId = currentUserId,
                Title = title,
                TimeNeeded = guna2TextBox2.Text.Trim(),
                Difficulty = comboBox1.Text,
                CategoryName = categoryName,
                Ingredients = richTextBox1.Text.Trim(),
                Steps = richTextBox2.Text.Trim(),
                ImageUrl = finalImagePath
            };

            // 5. Gọi Service
            try
            {
                bool success;
                if (recipeId == null)
                {
                    // ➕ Thêm mới
                    success = _recipeService.AddNewRecipe(recipe);
                }
                else
                {
                    // ✏️ Chỉnh sửa
                    recipe.RecipeId = recipeId.Value; // Gán ID để biết update món nào
                    success = _recipeService.UpdateRecipe(recipe);
                }

                if (success)
                {
                    MessageBox.Show(recipeId == null ? "Đã thêm công thức thành công!" : "Đã cập nhật công thức!",
                                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK; // Báo cho form cha biết để reload list
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Đã xảy ra lỗi khi lưu vào cơ sở dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        // Click vào ảnh (Giữ nguyên, nhưng chỉ dùng 1 hàm)
        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image files|*.jpg;*.jpeg;*.png;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // imagePath = ofd.FileName; // Không cần biến global
                    guna2PictureBox1.Image = Image.FromFile(ofd.FileName); // Hiển thị ảnh
                    guna2PictureBox1.Tag = ofd.FileName; // Lưu đường dẫn GỐC vào Tag
                }
            }
        }

        // Nút 'Browse' (button1) - (Giữ nguyên)
        private void button1_Click(object sender, EventArgs e)
        {
            // Tái sử dụng logic của guna2PictureBox1_Click
            guna2PictureBox1_Click(sender, e);
        }

        // Nút 'Xóa ảnh' (button2) - (Giữ nguyên)
        private void button2_Click(object sender, EventArgs e)
        {
            guna2PictureBox1.Image = null;
            guna2PictureBox1.Tag = null;
            // imagePath = null; // Bỏ đi
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        // Nút 'Close' (guna2Button2) - (Giữ nguyên)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}