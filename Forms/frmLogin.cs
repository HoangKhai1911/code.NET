using System;
using System.IO;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmLogin : Form
    {
        private readonly string userFile = "users.txt";

        public frmLogin()
        {
            InitializeComponent();

            // Attach event handlers (in case Designer didn't)
            guna2Button1.Click += BtnLogin_Click;   // "Log in" button
            linkLabel1.LinkClicked += LinkLabel1_LinkClicked; // "Register" link
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = guna2TextBox1.Text.Trim(); // username input
            string password = guna2TextBox2.Text.Trim(); // password input

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ValidateUser(username, password))
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open HomePage (no logout here per your requirement)
                frmHomePage home = new frmHomePage();
                home.Show();

                // Hide login form
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateUser(string username, string password)
        {
            if (!File.Exists(userFile)) return false;

            foreach (var line in File.ReadAllLines(userFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length >= 2 &&
                    parts[0].Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    parts[1] == password)
                {
                    return true;
                }
            }
            return false;
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open signup form
            frmSignup signup = new frmSignup();
            signup.Show();
            this.Hide();
        }
    }
}