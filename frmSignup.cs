using System;
using System.IO;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmSignup : Form
    {
        private readonly string userFile = "users.txt"; // file that stores accounts

        public frmSignup()
        {
            InitializeComponent();

            // Attach event handlers (in case Designer didn't)
            guna2Button1.Click += BtnSignUp_Click;        // "Sign up" button
            linkLabel1.LinkClicked += LinkLabel1_LinkClicked; // "Go to log in" link
        }

        private void BtnSignUp_Click(object sender, EventArgs e)
        {
            string username = guna2TextBox1.Text.Trim(); // username field
            string password = guna2TextBox3.Text.Trim(); // password field

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UserExists(username))
            {
                MessageBox.Show("Username already exists. Please choose another one.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Append new user as: username,password
            File.AppendAllText(userFile, $"{username},{password}{Environment.NewLine}");

            MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Open login form and close signup
            frmLogin login = new frmLogin();
            login.Show();
            this.Close();
        }

        private bool UserExists(string username)
        {
            if (!File.Exists(userFile)) return false;

            foreach (var line in File.ReadAllLines(userFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length >= 1 && parts[0].Equals(username, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open login form
            frmLogin login = new frmLogin();
            login.Show();
            this.Close();
        }
    }
}