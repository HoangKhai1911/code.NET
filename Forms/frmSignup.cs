using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCook
{
    public partial class frmSignup : Form
    {
        public frmSignup()
        {
            InitializeComponent();
            guna2Button1.Click += BtnSignUp_Click;        // "Sign up" button
            linkLabel1.LinkClicked += LinkLabel1_LinkClicked; // "Go to log in" link
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
