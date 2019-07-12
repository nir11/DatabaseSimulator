using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseSimulator
{
    public partial class LogInWindow : Form
    {
        MainForm mainForm;
        bool IsLogInBtnClcked = false;
        public static string userID;


        // Constructor
        public LogInWindow(MainForm form1)
        {
            InitializeComponent();
            mainForm = form1;
            this.FormClosing += Form1_FormClosing;
            logInTextBox.KeyDown += new KeyEventHandler(logInTextBox_KeyDown);
        }

        // Log In with user's ID and open the Settings window
        private void logInBtn_Click(object sender, EventArgs e)
        {
            if (!Int32.TryParse(logInTextBox.Text, out int value))
            {
                MessageBox.Show("Please fill only numbers!");
                return;
            }
            IsLogInBtnClcked = true;
            userID = logInTextBox.Text;
            this.Close();
            mainForm.settingsBtn_Click(sender, e);
        }

        // Handling Enter clicking
        private void logInTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                logInBtn_Click(sender, e);
            }
        }


        // Close the project ater the user click the X button
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!IsLogInBtnClcked)
            {
                if (e.CloseReason == CloseReason.WindowsShutDown) return;
                // Confirm user wants to close
                DialogResult result = MessageBox.Show(this, "Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.FormClosing -= Form1_FormClosing;
                    Application.Exit();
                }
            }
            
        }

    }
}
