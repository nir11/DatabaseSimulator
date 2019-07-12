using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseSimulator
{

    public partial class SettingsWindow : Form
    {
        static SettingsWindow messageBox;
        static DialogResult result = DialogResult.No;

        // Constructor
        public SettingsWindow()
        {
            InitializeComponent();
                 
        }      

        public static DialogResult Show()
        {

            messageBox = new SettingsWindow();       
            messageBox.ShowDialog();
            return result;
        }


        // Handling OK button click
        private void btnOk_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            try
            {
                if (databasesListComboBox.SelectedItem == null)
                {
                MessageBox.Show("Please choose a Database");
                return;
                }      
                else
                {                  
                    result = DialogResult.OK;
                    MainForm.dbName = databasesListComboBox.SelectedItem.ToString();
                    messageBox.Close();
                }
              
            }
            catch (Exception)
            {
                MessageBox.Show("Please choose a Database");
            }
        }

        // Show Databases list after the window is loading
        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            result = DialogResult.No;
            MainForm mainForm = new MainForm();
            //string currConnection = "Data Source=.;Initial Catalog=" + "sysdatabases" + "; Integrated Security=True";
            //handle databases list
            using (var sqlCon = new SqlConnection("Data Source=.; Integrated Security=True;"))
            {
                try
                {
                    sqlCon.Open();
                    string query = "SELECT name FROM master.dbo.sysdatabases ORDER BY name";

                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                    if (databasesListComboBox.Items.Count > 0)
                    {
                        databasesListComboBox.DataSource = null;
                        databasesListComboBox.Items.Clear();
                        databasesListComboBox.ResetText();
                    }

                    while (sqlReader.Read())
                    {
                        //column2ComboBox.Items.Clear();
                        //column2ComboBox.DataSource = null;
                        //column2ComboBox.Items.Clear();                        
                        databasesListComboBox.Items.Add(sqlReader["name"].ToString());
                    }
                  
                }

                catch (Exception)
                {
                    /*Handle error*/
                }
                finally
                {
                    sqlCon.Close();
                }


            }
        }     
  
    }
}
