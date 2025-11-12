// frmIngredients.cs
//co chuc nang 10 cua phuc
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WinCook
{
    public class frmIngredients : Form
    {
        private readonly int _recipeId;

        private DataGridView dgv;
        private TextBox txtName;
        private TextBox txtQty;
        private Button btnAdd, btnUpdate, btnDelete, btnClose;

        public frmIngredients(int recipeId)
        {
            _recipeId = recipeId;
            Text = $"Ingredients • Recipe #{_recipeId}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 700;
            Height = 480;
            BackColor = Color.White;

            BuildUI();
            Load += (s, e) => LoadData();
        }

        private void BuildUI()
        {
            // Grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 320,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "IngredientID", HeaderText = "ID", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", Width = 350 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", Width = 200 });
            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    txtName.Text = dgv.Rows[e.RowIndex].Cells["Name"].Value?.ToString();
                    txtQty.Text = dgv.Rows[e.RowIndex].Cells["Quantity"].Value?.ToString();
                }
            };

            // Inputs
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

            var lblName = new Label { Text = "Name:", AutoSize = true, Top = 8, Left = 10 };
            txtName = new TextBox { Top = 4, Left = 80, Width = 260 };

            var lblQty = new Label { Text = "Quantity:", AutoSize = true, Top = 8, Left = 360 };
            txtQty = new TextBox { Top = 4, Left = 440, Width = 180 };

            btnAdd = new Button { Text = "Add", Width = 90, Height = 32, Left = 80, Top = 44 };
            btnUpdate = new Button { Text = "Update", Width = 90, Height = 32, Left = 180, Top = 44 };
            btnDelete = new Button { Text = "Delete", Width = 90, Height = 32, Left = 280, Top = 44 };
            btnClose = new Button { Text = "Close", Width = 90, Height = 32, Left = 530, Top = 44 };

            btnAdd.Click += (s, e) => DoAdd();
            btnUpdate.Click += (s, e) => DoUpdate();
            btnDelete.Click += (s, e) => DoDelete();
            btnClose.Click += (s, e) => Close();

            pnl.Controls.AddRange(new Control[] { lblName, txtName, lblQty, txtQty, btnAdd, btnUpdate, btnDelete, btnClose });

            Controls.Add(pnl);
            Controls.Add(dgv);
        }

        private void LoadData()
        {
            dgv.Rows.Clear();
            using (var conn = new SqlConnection(DBHelper.ConnectionString))
            using (var da = new SqlDataAdapter("SELECT ingredient_id, name, quantity FROM Ingredients WHERE recipe_id=@id ORDER BY ingredient_id", conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@id", _recipeId);
                var dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow r in dt.Rows)
                {
                    dgv.Rows.Add(r["ingredient_id"], r["name"], r["quantity"]);
                }
            }
        }

        private void DoAdd()
        {
            var name = txtName.Text.Trim();
            var qty = txtQty.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Ingredient name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = new SqlConnection(DBHelper.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Ingredients(recipe_id, name, quantity) VALUES(@rid, @n, @q)";
                cmd.Parameters.AddWithValue("@rid", _recipeId);
                cmd.Parameters.AddWithValue("@n", (object)name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@q", (object)qty ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            txtName.Clear();
            txtQty.Clear();
            LoadData();
        }

        private int? CurrentIngredientId()
        {
            if (dgv.CurrentRow == null) return null;
            if (dgv.CurrentRow.Cells["IngredientID"].Value == null) return null;
            if (int.TryParse(dgv.CurrentRow.Cells["IngredientID"].Value.ToString(), out int id)) return id;
            return null;
        }

        private void DoUpdate()
        {
            var id = CurrentIngredientId();
            if (id == null) { MessageBox.Show("Select a row to update."); return; }

            var name = txtName.Text.Trim();
            var qty = txtQty.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Ingredient name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = new SqlConnection(DBHelper.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE Ingredients SET name=@n, quantity=@q WHERE ingredient_id=@id";
                cmd.Parameters.AddWithValue("@n", (object)name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@q", (object)qty ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadData();
        }

        private void DoDelete()
        {
            var id = CurrentIngredientId();
            if (id == null) { MessageBox.Show("Select a row to delete."); return; }

            if (MessageBox.Show("Delete this ingredient?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            using (var conn = new SqlConnection(DBHelper.ConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"DELETE FROM Ingredients WHERE ingredient_id=@id";
                cmd.Parameters.AddWithValue("@id", id.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadData();
        }
    }
}
