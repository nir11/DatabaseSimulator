using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DatabaseSimulator
{
    public partial class MainForm : Form
    {
        public LogFile logFile;
        public static string dbName = "";       // Holds the DB name after the user chose it
        public string connectionString = "Data Source=.;Initial Catalog=" + dbName +"; Integrated Security=True";
        public int panelsCounter_FilterPanel = 1;       // Counts the number of open filters in the Filter panel
        bool columnBtnForValue1IsActive = true, columnBtnForValue2IsActive = true, columnBtnForValue3IsActive = true, columnBtnForValue4IsActive = true, columnBtnForValue5IsActive = true;
        public bool isOperationDone = false;
        string filterQuery = "";
        private int _MouseIndex = -1;

        public MainForm()
        {
            InitializeComponent();
            sourcesListBox.HorizontalScrollbar = true;
            OpenListBoxValue1_FilterPanel.AutoSize = true;
            OpenListBoxValue2_FilterPanel.AutoSize = true;
            OpenListBoxValue3_FilterPanel.AutoSize = true;
            OpenListBoxValue4_FilterPanel.AutoSize = true;
            OpenListBoxValue5_FilterPanel.AutoSize = true;

            this.FormClosing += Form1_FormClosing;    
        }


        #region Main Form functions

        // Show the Log In window on startup
        private void Form1_Shown(object sender, EventArgs e)
        {
            LogInWindow logInWindow = new LogInWindow(this);
            logInWindow.Show();
        }

        // A function called when the user close the Simulator. It will delete the log file if no operation was done
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isOperationDone)
            {
                LogFile.deleteLogFileIfNoOperationWasDone();
            }
            Application.Exit();
        }

        #endregion

        #region Settings Button

        /* Open settings will open a new Dialog message - SettingMessageBox. 
            It will check if a any Database name has been selected and import its tables to the Sources list on the screen */
        public void settingsBtn_Click(object sender, EventArgs e)
        {
               DialogResult dr = SettingsWindow.Show();
            
            if (dr == DialogResult.OK)
            {
                resetJoinPanel();
                resetFilterPanel();
                dataGridView1.DataSource = null;
                connectionString = "Data Source=.;Initial Catalog=" + dbName + "; Integrated Security=True";        // Set the new connectionString

                //Fill the Sources list by the chosen database name
                string query = "SELECT name FROM sys.tables ORDER BY name";
                fillSourcesList(query);

                query = "SELECT name FROM sys.tables ORDER BY name";
                //reset comboboxes that holding source table data              
                fillComboBox(query, "name", sourceTableComboBox_JoinPanel);
                fillComboBox(query, "name", targetTableComboBox_JoinPanel);
                sourceTableComboBox_JoinPanel.Text = "בחר טבלה";
                targetTableComboBox_JoinPanel.Text = "בחר טבלה";

                if (!isOperationDone)
                {
                    LogFile.deleteLogFileIfNoOperationWasDone();
                }
                //Log file creates
                logFile = new LogFile(this, LogInWindow.userID, dbName);

            }
        }

        #endregion

        #region Sources List

        // Filling the Sources list with a query result
        public void fillSourcesList(string fillQuery)
        {
            if (sourcesListBox.Items.Count != 0)
            {
                sourcesListBox.Items.Clear();
            }

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlCommand sqlCommand = new SqlCommand(fillQuery, sqlCon);
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        //filling the sources list with the column name called "name"
                        sourcesListBox.Items.Add(sqlDataReader["name"].ToString());
                        // Update  filter query as inactive
                        filterQuery = "";
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

        // Handling the Sources list for every new table selection
        private void sourcesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            resetFilterPanel();
         
            // Set save table button invisible
            if (saveTableBtn.Visible)
            {
                saveTableBtn.Visible = false;
                if (textBoxForSaveTable.Visible)
                {
                    textBoxForSaveTable.Visible = false;
                    saveTableBtnIcon.Visible = false;
                }
            }

            string query;

            // Show the chosen source table in the gridview
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    query = "Select * from " + sourcesListBox.SelectedItem;
                    SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                    DataTable dataTable = new DataTable();
                    sqlData.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;

                    // Update  filter query as inactive
                    filterQuery = "";
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

            // Initializing Join panel with the new source table data
            query = "SELECT name FROM sys.tables ORDER BY name";
            sourceTableComboBox_JoinPanel.DataSource = sourcesListBox.DataSource;
            sourceTableComboBox_JoinPanel.ForeColor = Color.Black;
            sourceTableComboBox_JoinPanel.Text = sourcesListBox.Text;
            query = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" + sourcesListBox.SelectedItem + "'";
            fillComboBox(query, "COLUMN_NAME", columnsForSourceTableComboBox_JoinPanel);
            if (!columnsForSourceTableComboBox_JoinPanel.Items.Contains("בחר עמודה"))
            {
                columnsForSourceTableComboBox_JoinPanel.Items.Add("בחר עמודה");
            }          
            columnsForSourceTableComboBox_JoinPanel.Text = "בחר עמודה";            
            columnsForSourceTableComboBox_JoinPanel.ForeColor = Color.Silver;
            if(!targetTableComboBox_JoinPanel.Items.Contains("בחר טבלה"))
            {
                targetTableComboBox_JoinPanel.Items.Add("בחר טבלה");
            }
            targetTableComboBox_JoinPanel.Text = "בחר טבלה";
            targetTableComboBox_JoinPanel.ForeColor = Color.Silver;

            // Initializing Filter panel with the new source table data
            query = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" + sourcesListBox.SelectedItem.ToString() + "'";
            fillComboBox(query, "COLUMN_NAME", column1_FilterPanel);
            column1_FilterPanel.Items.Add("בחר עמודה");
            column1_FilterPanel.Text = "בחר עמודה";
            fillComboBox(query, "COLUMN_NAME", column2_FilterPanel);
            column2_FilterPanel.Items.Add("בחר עמודה");
            column2_FilterPanel.Text = "בחר עמודה";
            fillComboBox(query, "COLUMN_NAME", column3_FilterPanel);
            column3_FilterPanel.Items.Add("בחר עמודה");
            column3_FilterPanel.Text = "בחר עמודה";
            fillComboBox(query, "COLUMN_NAME", column4_FilterPanel);
            column4_FilterPanel.Items.Add("בחר עמודה");
            column4_FilterPanel.Text = "בחר עמודה";
            fillComboBox(query, "COLUMN_NAME", column5_FilterPanel);
            column5_FilterPanel.Items.Add("בחר עמודה");
            column5_FilterPanel.Text = "בחר עמודה";

        }

        #endregion

        #region ComboBox General Functions

        //This function fill a variety of comboBoxes for both Join & Filter operations.
        //The function gets a query to execute and fills the combobox with a required column name from the query result        
        public void fillComboBox(string query, string requiredColumnName, ComboBox currComboBox)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                    if (currComboBox.Items.Count > 0)
                    {
                        currComboBox.DataSource = null;
                        currComboBox.Items.Clear();
                        currComboBox.ResetText();
                    }

                    while (sqlReader.Read())
                    {
                        //column2ComboBox.Items.Clear();
                        //column2ComboBox.DataSource = null;
                        //column2ComboBox.Items.Clear();                        
                        currComboBox.Items.Add(sqlReader[requiredColumnName].ToString());
                    }
                }

                catch (Exception)
                {
                    /*Handle error*/
                }
                finally
                {
                    currComboBox.Text = "בחר עמודה";
                    sqlCon.Close();
                }
            }
        }

        //This function is called when a reset operation is in progress. 
        //It will reset the input comboBox and put its fitting placeHolder
        public void resetComboBoxItem(ComboBox item, string placeHolder)
        {
            item.DataSource = null;
            item.Items.Clear();
            item.ResetText();
            item.Items.Add(placeHolder);
            item.Text = placeHolder;
            item.ForeColor = Color.Silver;
        }

        #endregion

        #region Import CSV Button

        // Import the CSV file, store it in the database and show it on the screen 
        private void importBtn_Click(object sender, EventArgs e)
        {
            if (sourcesListBox.Items.Count == 0)
            {
                MessageBox.Show("You must open a database first!");
                return;     // Finish function
            }
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = ".csv";
                ofd.Filter = "Comma Separated (*.csv)|*.csv";
                ofd.ShowDialog();       // Open dialog window
                string csvTableName = System.IO.Path.GetFileName(ofd.FileName);
                csvTableName = csvTableName.Substring(0, csvTableName.Length - 4);      // Get the name of the CSV File
                string csvFilePath = ofd.FileName;                                      // Get the path of the CSV File
                CSVFile file = new CSVFile(this, csvFilePath, csvTableName);

                // Get the imported data from the CSV file into a DataTable element by invoking the function getDataFromCsvFile() from CSVFile Class
                DataTable importedData = file.getDataFromCsvFile();

                // Insert it into a new table in the database (get true if succeeded)
                if (file.insertDataTableIntoDatabase(importedData, csvTableName))
                {
                    // Initialization of Join panel elements
                    string query = "SELECT name FROM sys.tables ORDER BY name";
                    fillComboBox(query, "name", sourceTableComboBox_JoinPanel);
                    sourceTableComboBox_JoinPanel.Text = "בח ר טבלה";
                    fillComboBox(query, "name", targetTableComboBox_JoinPanel);
                    targetTableComboBox_JoinPanel.Text = "בחר טבלה";
                    fillSourcesList(query);

                    // Show a new source table (with the CSV file data) in the gridview
                    using (SqlConnection sqlCon = new SqlConnection(connectionString))
                    {
                        try
                        {
                            sqlCon.Open();
                            query = "Select * from " + csvTableName;
                            SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                            sqlData.Fill(importedData);
                            dataGridView1.DataSource = importedData;
                            sourcesListBox.SelectedItem = csvTableName;
                        }

                        catch (Exception ex)
                        {
                            /*Handle error*/
                            MessageBox.Show("Error - cannot open table\n" + ex.Message);
                        }
                        finally
                        {
                            sqlCon.Close();
                        }
                    }

                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error - cannot open this file\n" + exc.Message);
            }


        }

        #endregion

        #region Join Paenl Controls

        // Starting Join operation after user click
        private void joinBtn_Click(object sender, EventArgs e)
        {
            // User must choose a source table first
            if (sourcesListBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose a source table first!");
                return;
            }

            if (filterPanel.Visible)
            {
                filterPanel.Visible = false;
                runBtn_FilterPanel.Visible = false;
            }
            if (writeQueryPanel.Visible)
                writeQueryPanel.Visible = false;
            joinPanel.Visible = true;
            sourcesListBox_SelectedIndexChanged(sender, e);
            // Initializing of sourceTableComboBox and targetTableComboBox columns
            sourceTableComboBox_JoinPanel.Text = sourcesListBox.Text;
            targetTableComboBox_JoinPanel.DataSource = sourcesListBox.DataSource;
            

        }

        // Fills the Source table combobox and shows its columns in 
        private void sourceTableComboBox_JoinPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query;
            //Show the source table in the gridview
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    query = "Select * from " + sourceTableComboBox_JoinPanel.SelectedItem;
                    SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                    DataTable dataTable = new DataTable();
                    sqlData.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
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

            // Update column names for the chosen table
            query = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" + sourceTableComboBox_JoinPanel.SelectedItem.ToString() + "'";
            fillComboBox(query, "COLUMN_NAME", columnsForSourceTableComboBox_JoinPanel);

            //Update the choosen table in the sources list box
            sourcesListBox.SelectedItem = sourceTableComboBox_JoinPanel.SelectedItem;
        }

        // Fills the Target table combobox and shows its columns in 
        private void targetTableComboBox_JoinPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" + targetTableComboBox_JoinPanel.SelectedItem.ToString() + "'";
            if (columnsForTargetTableComboBox_JoinPanel.Items.Count > 0)
            {
                columnsForTargetTableComboBox_JoinPanel.DataSource = null;
                columnsForTargetTableComboBox_JoinPanel.Items.Clear();
                columnsForTargetTableComboBox_JoinPanel.ResetText();
            }
            // Update column names for the chosen table
            fillComboBox(query, "COLUMN_NAME", columnsForTargetTableComboBox_JoinPanel);
            columnsForTargetTableComboBox_JoinPanel.Items.Add("בחר עמודה");
            columnsForTargetTableComboBox_JoinPanel.Text = "בחר עמודה";
            columnsForTargetTableComboBox_JoinPanel.ForeColor = Color.Silver;
        }

        /*Clicking on the Run button will execute a query of the user join choices 
         and will provide it in the gridtable if it will produce a none enpty table*/
        private void runBtn_JoinPanel_Click(object sender, EventArgs e)
        {
            Join join = new Join(this);
            if (!join.checkIfAllFieldsAreFullInJoinPanel())
            {
                MessageBox.Show("Please fill all the required fields!!");
                return;
            }
            // Set query to execute
            string query = "select * from " + sourceTableComboBox_JoinPanel.SelectedItem + " inner join " + targetTableComboBox_JoinPanel.SelectedItem
                        + " on " + sourceTableComboBox_JoinPanel.SelectedItem + "." + columnsForSourceTableComboBox_JoinPanel.SelectedItem + " = " + targetTableComboBox_JoinPanel.SelectedItem + "." + columnsForTargetTableComboBox_JoinPanel.SelectedItem;

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                    DataTable dataTable = new DataTable();  // Holds the query result
                    sqlData.Fill(dataTable);
                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No results found!");

                        //Resert columns
                        columnsForSourceTableComboBox_JoinPanel.ResetText();
                        columnsForSourceTableComboBox_JoinPanel.Text = "בחר עמודה";
                        columnsForSourceTableComboBox_JoinPanel.ForeColor = Color.Silver;
                        columnsForTargetTableComboBox_JoinPanel.ResetText();
                        columnsForTargetTableComboBox_JoinPanel.Text = "בחר עמודה";
                        columnsForTargetTableComboBox_JoinPanel.ForeColor = Color.Silver;
                        return;     // Finish function
                    }

                    // Update  filter query as inactive
                    filterQuery = "";

                    // Fill the grid table with the result
                    dataGridView1.DataSource = dataTable;
                    saveTableBtn.Visible = true;
                    isOperationDone = true;
                    // Write Join records in the Log file
                    logFile.WritOperationInLogFile("Join", query, sourcesListBox.SelectedItem.ToString(), targetTableComboBox_JoinPanel.SelectedItem.ToString());                
                }

                catch (Exception)
                {
                    /*Handle error*/
                    MessageBox.Show("No results found!");
                    columnsForSourceTableComboBox_JoinPanel.ResetText();
                    columnsForSourceTableComboBox_JoinPanel.Text = "בחר עמודה";
                    columnsForSourceTableComboBox_JoinPanel.ForeColor = Color.Silver;
                    columnsForTargetTableComboBox_JoinPanel.ResetText();
                    columnsForTargetTableComboBox_JoinPanel.Text = "בחר עמודה";
                    columnsForTargetTableComboBox_JoinPanel.ForeColor = Color.Silver;
                }
                finally
                {
                    sqlCon.Close();
                }
            }
        }

        // Reset all elements of Join panel
        public void resetJoinPanel()
        {
            resetComboBoxItem(sourceTableComboBox_JoinPanel, "בחר טבלה");
            resetComboBoxItem(targetTableComboBox_JoinPanel, "בחר טבלה");
            resetComboBoxItem(columnsForSourceTableComboBox_JoinPanel, "בחר עמודה");
            resetComboBoxItem(columnsForTargetTableComboBox_JoinPanel, "בחר עמודה");
        }

        // Paint borders for Source table panel
        private void sourceTablePanel_JoinPanel_Paint(object sender, PaintEventArgs e)
        {
            if (sourceTablePanel_JoinPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 1;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              sourceTablePanel_JoinPanel.ClientSize.Width - thickness,
                                                              sourceTablePanel_JoinPanel.ClientSize.Height - thickness));
                }
            }
        }

        // Paint borders for Target table panel
        private void targetTablePanel_JoinPanel_Paint(object sender, PaintEventArgs e)
        {
            if (targetTablePaenl_JoinPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 1;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                               targetTablePaenl_JoinPanel.ClientSize.Width - thickness,
                                                               targetTablePaenl_JoinPanel.ClientSize.Height - thickness));
                }
            }
        }

        // Set on placeholder off sourceTableComboBox
        private void sourceTableComboBox_JoinPanel_Enter(object sender, EventArgs e)
        {
            sourceTableComboBox_JoinPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on sourceTableComboBox
        private void sourceTableComboBox_JoinPanel_Leave(object sender, EventArgs e)
        {
            if (sourceTableComboBox_JoinPanel.Text == "בחר טבלה")
            {
                sourceTableComboBox_JoinPanel.ForeColor = Color.Silver;
            }
        }

        // Set on placeholder off columnsForSourceTableComboBox
        private void columnsForSourceTableComboBox_JoinPanel_Enter(object sender, EventArgs e)
        {
            columnsForSourceTableComboBox_JoinPanel.ForeColor = Color.Black;

        }

        // Set on placeholder on columnsForSourceTableComboBox
        private void columnsForSourceTableComboBox_JoinPanel_Leave(object sender, EventArgs e)
        {
            if (columnsForSourceTableComboBox_JoinPanel.Text == "בחר עמודה")
            {
                columnsForSourceTableComboBox_JoinPanel.ForeColor = Color.Silver;
            }
        }

        // Set on placeholder off targetTableComboBox
        private void targetTableComboBox_JoinPanel_Enter(object sender, EventArgs e)
        {
            targetTableComboBox_JoinPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on targetTableComboBox
        private void targetTableComboBox_JoinPanel_Leave(object sender, EventArgs e)
        {
            if (targetTableComboBox_JoinPanel.Text == "בחר טבלה")
            {
                targetTableComboBox_JoinPanel.ForeColor = Color.Silver;
            }
        }

        // Set on placeholder off columnsForTargetTableComboBox
        private void columnsForTargetTableComboBox_JoinPanel_Enter(object sender, EventArgs e)
        {
            columnsForTargetTableComboBox_JoinPanel.ForeColor = Color.Black;

        }

        // Set on placeholder on columnsForTargetTableComboBox
        private void columnsForTargetTableComboBox_JoinPanel_Leave(object sender, EventArgs e)
        {
            if (columnsForTargetTableComboBox_JoinPanel.Text == "בחר עמודה")
            {
                columnsForTargetTableComboBox_JoinPanel.ForeColor = Color.Silver;
            }
        }


        #endregion Join Paenl

        #region Filter Paenl Controls

        // Starting Filter operation after user click
        private void filterBtn_Click(object sender, EventArgs e)
        {
            if (sourcesListBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose a source table first!");
                return;
            }
            if (joinPanel.Visible)
                joinPanel.Visible = false;
            if (writeQueryPanel.Visible)
                writeQueryPanel.Visible = false;
            filterPanel.Visible = true;
            runBtn_FilterPanel.Visible = true;
            sourcesListBox_SelectedIndexChanged(sender, e);

            // Initializing first filter panel 
            //condition1_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition1_FilterPanel.Text = "בחר תנאי";

            // Reset all dropDownLists
            /*resetColumnDropDownList(column1_FilterPanel);
            resetColumnDropDownList(column2_FilterPanel);
            resetColumnDropDownList(column3_FilterPanel);
            resetColumnDropDownList(column4_FilterPanel);
            resetColumnDropDownList(column5_FilterPanel);*/

            /*resetConditionDropDownList(condition1_FilterPanel);
            resetConditionDropDownList(condition2_FilterPanel);
            resetConditionDropDownList(condition3_FilterPanel);
            resetConditionDropDownList(condition4_FilterPanel);
            resetConditionDropDownList(condition5_FilterPanel);

            resetColumnDropDownList(value1ComboBox_FilterPanel);
            resetColumnDropDownList(value2ComboBox_FilterPanel);
            resetColumnDropDownList(value3ComboBox_FilterPanel);
            resetColumnDropDownList(value4ComboBox_FilterPanel);
            resetColumnDropDownList(value5ComboBox_FilterPanel);*/


            /*resetColumnDropDownList(value1ComboBox_FilterPanel);
            resetColumnDropDownList(value2ComboBox_FilterPanel);
            resetColumnDropDownList(value3ComboBox_FilterPanel);
            resetColumnDropDownList(value4ComboBox_FilterPanel);
            resetColumnDropDownList(value5ComboBox_FilterPanel);*/

        }

        private void resetColumnDropDownList(ComboBox dropDownList)
        {
            dropDownList.Items.Clear();
            if (!dropDownList.Items.Contains("בחר עמודה"))
            {
                dropDownList.Items.Add("בחר עמודה");
            }
            dropDownList.Text = "בחר עמודה";
            dropDownList.ForeColor = Color.Silver;
        }

        private void resetConditionDropDownList(ComboBox dropDownList)
        {
            dropDownList.DataSource = new List<string>(new string[] {"בחר תנאי", "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            dropDownList.Text = "בחר תנאי";
            dropDownList.ForeColor = Color.Silver;
        }

        /*Clicking on the Run button will execute a query of the user filter choices 
         and will provide it in the gridtable if it will produce a none enpty table*/
        private void runBtnFilterPanelClick(object sender, EventArgs e)
        {
            Filter filter = new Filter(this);
            if (!filter.checkIfAllFieldsAreFullInFilterPanel())
            {
                MessageBox.Show("Please fill all the required fields!!");
                return;
            }
            panelsCounter_FilterPanel = filter.getNumOfVisibleFilterPanels();
            string query;
            if (filterQuery == "")
            {
                // Creates the fitting query for user choices with the function addFilterToQuery that return a suitable query for every filter panel 
                query = "select * from " + sourcesListBox.SelectedItem + " where";
            }
            else
            {
                query = filterQuery + " AND";
            }

            switch (panelsCounter_FilterPanel)      // panelsCounter provide how many filters panels are chosen
            {
                case 1:
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column1_FilterPanel.SelectedItem.ToString(),
                            condition1_FilterPanel.SelectedItem.ToString(), value1ComboBox_FilterPanel.SelectedItem.ToString());
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column1_FilterPanel.SelectedItem.ToString(), condition1_FilterPanel.SelectedItem.ToString(), value1TextBox_FilterPanel.Text);
                    }
                    break;
                case 2:
                    //Filter1
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column1_FilterPanel.SelectedItem.ToString(),
                            condition1_FilterPanel.SelectedItem.ToString(), value1ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                     }
                    else
                    {
                        query += filter.addFilterToQuery(column1_FilterPanel.SelectedItem.ToString(), condition1_FilterPanel.SelectedItem.ToString(), value1TextBox_FilterPanel.Text);
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();

                    }
                    //Filter2
                    if (columnBtnForValue2IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column2_FilterPanel.SelectedItem.ToString(),
                          condition2_FilterPanel.SelectedItem.ToString(), value2ComboBox_FilterPanel.SelectedItem.ToString());
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column2_FilterPanel.SelectedItem.ToString(), condition2_FilterPanel.SelectedItem.ToString(), value2TextBox_FilterPanel.Text);

                    }
                    break;
                case 3:
                    //Filter1
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column1_FilterPanel.SelectedItem.ToString(),
                           condition1_FilterPanel.SelectedItem.ToString(), value1ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column1_FilterPanel.SelectedItem.ToString(), condition1_FilterPanel.SelectedItem.ToString(), value1TextBox_FilterPanel.Text);
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                    }
                    //Filter2
                    if (columnBtnForValue2IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column2_FilterPanel.SelectedItem.ToString(),
                         condition2_FilterPanel.SelectedItem.ToString(), value2ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();                 
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column2_FilterPanel.SelectedItem.ToString(), condition2_FilterPanel.SelectedItem.ToString(), value2TextBox_FilterPanel.Text);
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();                  
                    }
                    //Filter3
                    if (columnBtnForValue3IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column3_FilterPanel.SelectedItem.ToString(),
                     condition3_FilterPanel.SelectedItem.ToString(), value3ComboBox_FilterPanel.SelectedItem.ToString());
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column3_FilterPanel.SelectedItem.ToString(), condition3_FilterPanel.SelectedItem.ToString(), value3TextBox_FilterPanel.Text);
                    }
                    break;
                case 4:
                    //Filter1
                    if (columnBtnForValue1IsActive)
                        {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column1_FilterPanel.SelectedItem.ToString(),
                        condition1_FilterPanel.SelectedItem.ToString(), value1ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                        }
                    else
                    {
                        query += filter.addFilterToQuery(column1_FilterPanel.SelectedItem.ToString(), condition1_FilterPanel.SelectedItem.ToString(), value1TextBox_FilterPanel.Text);
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                    }
                    //Filter2
                    if (columnBtnForValue2IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column2_FilterPanel.SelectedItem.ToString(),
                        condition2_FilterPanel.SelectedItem.ToString(), value2ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column2_FilterPanel.SelectedItem.ToString(), condition2_FilterPanel.SelectedItem.ToString(), value2TextBox_FilterPanel.Text);
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();
                    
                    }
                    //Filter3
                    if (columnBtnForValue3IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column3_FilterPanel.SelectedItem.ToString(),
                        condition3_FilterPanel.SelectedItem.ToString(), value3ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator3_FilterPanel.SelectedItem.ToString();                 
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column3_FilterPanel.SelectedItem.ToString(), condition3_FilterPanel.SelectedItem.ToString(), value3TextBox_FilterPanel.Text);
                        query += " " + operator3_FilterPanel.SelectedItem.ToString();
                    }
                    //Filter4
                    if (columnBtnForValue4IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column4_FilterPanel.SelectedItem.ToString(),
                          condition4_FilterPanel.SelectedItem.ToString(), value4ComboBox_FilterPanel.SelectedItem.ToString());
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column4_FilterPanel.SelectedItem.ToString(), condition4_FilterPanel.SelectedItem.ToString(), value4TextBox_FilterPanel.Text);

                    }
                    break;
                case 5:
                    // Filter1
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column1_FilterPanel.SelectedItem.ToString(),
                     condition1_FilterPanel.SelectedItem.ToString(), value1ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                            }
                    else
                    {
                        query += filter.addFilterToQuery(column1_FilterPanel.SelectedItem.ToString(), condition1_FilterPanel.SelectedItem.ToString(), value1TextBox_FilterPanel.Text);
                        query += " " + operator1_FilterPanel.SelectedItem.ToString();
                    }
                    // Filter2
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column2_FilterPanel.SelectedItem.ToString(),
                       condition2_FilterPanel.SelectedItem.ToString(), value2ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();                     
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column2_FilterPanel.SelectedItem.ToString(), condition2_FilterPanel.SelectedItem.ToString(), value2TextBox_FilterPanel.Text);
                        query += " " + operator2_FilterPanel.SelectedItem.ToString();                     
                    }
                    // Filter3
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column3_FilterPanel.SelectedItem.ToString(),
                         condition3_FilterPanel.SelectedItem.ToString(), value3ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator3_FilterPanel.SelectedItem.ToString();
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column3_FilterPanel.SelectedItem.ToString(), condition3_FilterPanel.SelectedItem.ToString(), value3TextBox_FilterPanel.Text);
                        query += " " + operator3_FilterPanel.SelectedItem.ToString();                    
                    }
                    // Filter4
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column4_FilterPanel.SelectedItem.ToString(),
                        condition4_FilterPanel.SelectedItem.ToString(), value4ComboBox_FilterPanel.SelectedItem.ToString());
                        query += " " + operator4_FilterPanel.SelectedItem.ToString();                   
                    }
                    else
                    {
                        query += filter.addFilterToQuery(column4_FilterPanel.SelectedItem.ToString(), condition4_FilterPanel.SelectedItem.ToString(), value4TextBox_FilterPanel.Text);
                        query += " " + operator4_FilterPanel.SelectedItem.ToString();
                    }
                    // Filter5
                    if (columnBtnForValue1IsActive)
                    {
                        query = filter.addFilterToQueryForColumnInValueChoice(sourcesListBox.SelectedItem.ToString(), column5_FilterPanel.SelectedItem.ToString(),
                       condition5_FilterPanel.SelectedItem.ToString(), value5ComboBox_FilterPanel.SelectedItem.ToString());

                    }
                    else
                    {
                        query += filter.addFilterToQuery(column5_FilterPanel.SelectedItem.ToString(), condition5_FilterPanel.SelectedItem.ToString(), value5TextBox_FilterPanel.Text);
                    }
                    break;
            }

            //Show the filter table in the gridview for the current query
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                    DataTable dataTable = new DataTable();      // Hold the query result
                    sqlData.Fill(dataTable);
                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No results found!");
                        return;
                    }
                    dataGridView1.DataSource = dataTable;
                    saveTableBtn.Visible = true;
                    isOperationDone = true;

                    // Write Join records in the Log file
                    logFile.WritOperationInLogFile("Filter", query, sourcesListBox.SelectedItem.ToString(), null);
                    filterQuery = query;
                }

                catch (Exception)
                {
                    /*Handle error*/
                    MessageBox.Show("No results found!");
                }
                finally
                {
                    sqlCon.Close();
                }

            }
        }

        // Reset all elements of Filter panel
        public void resetFilterPanel()
        {
            // Reset all dropDownLists
            resetColumnDropDownList(column1_FilterPanel);
            resetColumnDropDownList(column2_FilterPanel);
            resetColumnDropDownList(column3_FilterPanel);
            resetColumnDropDownList(column4_FilterPanel);
            resetColumnDropDownList(column5_FilterPanel);

            resetConditionDropDownList(condition1_FilterPanel);
            resetConditionDropDownList(condition2_FilterPanel);
            resetConditionDropDownList(condition3_FilterPanel);
            resetConditionDropDownList(condition4_FilterPanel);
            resetConditionDropDownList(condition5_FilterPanel);

            resetColumnDropDownList(value1ComboBox_FilterPanel);
            resetColumnDropDownList(value2ComboBox_FilterPanel);
            resetColumnDropDownList(value3ComboBox_FilterPanel);
            resetColumnDropDownList(value4ComboBox_FilterPanel);
            resetColumnDropDownList(value5ComboBox_FilterPanel);


            /*

            resetComboBoxItem(column1_FilterPanel, "בחר עמודה");
            resetComboBoxItem(column2_FilterPanel, "בחר עמודה");
            resetComboBoxItem(column3_FilterPanel, "בחר עמודה");
            resetComboBoxItem(column4_FilterPanel, "בחר עמודה");
            resetComboBoxItem(column5_FilterPanel, "בחר עמודה");

            resetComboBoxItem(condition1_FilterPanel, "בחר תנאי");
            resetComboBoxItem(condition2_FilterPanel, "בחר תנאי");
            resetComboBoxItem(condition3_FilterPanel, "בחר תנאי");
            resetComboBoxItem(condition4_FilterPanel, "בחר תנאי");
            resetComboBoxItem(condition5_FilterPanel, "בחר תנאי");
            */
            /*value1_FilterPanel.Text = "הזן ערך";
            value1_FilterPanel.ForeColor = Color.Silver;
            value2_FilterPanel.Text = "הזן ערך";
            value2_FilterPanel.ForeColor = Color.Silver;
            value3_FilterPanel.Text = "הזן ערך";
            value3_FilterPanel.ForeColor = Color.Silver;
            value4_FilterPanel.Text = "הזן ערך";
            value4_FilterPanel.ForeColor = Color.Silver;
            value5_FilterPanel.Text = "הזן ערך";
            value5_FilterPanel.ForeColor = Color.Silver;*/


            panel2_FilterPanel.Visible = false;
            panel3_FilterPanel.Visible = false;
            panel4_FilterPanel.Visible = false;
            panel5_FilterPanel.Visible = false;

            operator1_FilterPanel.Visible = false;
            operator2_FilterPanel.Visible = false;
            operator3_FilterPanel.Visible = false;
            operator4_FilterPanel.Visible = false;

            xBtn1_FilterPanel.Visible = false;
            xBtn2_FilterPanel.Visible = false;
            xBtn3_FilterPanel.Visible = false;
            xBtn4_FilterPanel.Visible = false;
            addFilterBtn2_FilterPanel.Visible = false;
            addFilterBtn3_FilterPanel.Visible = false;
            addFilterBtn4_FilterPanel.Visible = false;

            addFilterBtn1_FilterPanel.Visible = true;

            filterPanel.AutoScroll = false; ;

            //set condition1 columns (=, >, <...)
            // condition1_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition1_FilterPanel.Text = "בחר תנאי";
            //set value1 placeholder
            //value1_FilterPanel.Text = "הזן ערך..";

        }



        //Handlers
        private void setOffPlaceholderForValueNumComboBox(ComboBox comboBox)
        {
            comboBox.Text = "";
            comboBox.ForeColor = Color.Black;
        }

        private void setOnPlaceholderForValueNumComboBox(ComboBox comboBox)
        {
            if (comboBox.Text == "הזן ערך")
            {
                comboBox.ForeColor = Color.Silver;
            }

            //comboBox.Text = "הזן ערך";
        }

        private void OpeListBoxValueNum(TextBox valueNumTextBox, ListBox listBoxValueNum)
        {
            valueNumTextBox.Text = "";
            valueNumTextBox.ForeColor = Color.Black;
            listBoxValueNum.Visible = true;
        }

        private void getSelectedValueFromListBoxValueNum(TextBox valueNumTextBox, ListBox listBoxValueNum, Panel panelForListBoxValueNum_FilterPanel)
        {
            valueNumTextBox.Text = listBoxValueNum.Text;
            valueNumTextBox.ForeColor = Color.Black;
            listBoxValueNum.Visible = false;
            panelForListBoxValueNum_FilterPanel.Visible = false;
        }

        private void switchFromValuToColumnForValueNum(Button columnBtnForValueNum, Button valueBtnForValueNum,
          ComboBox columnNum, TextBox valueNumTextBox, ComboBox valueNumComboBox)
        {
            switchBetweenColumnAndValueColorsForValueProperty(columnBtnForValueNum, valueBtnForValueNum);
            if (columnNum.SelectedItem == null)
            {
                //MessageBox.Show("You must choose a column first!");
                valueNumComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                return;
            }
            if (columnNum.SelectedItem.ToString() == "בחר עמודה")
            {
                //MessageBox.Show("You must choose a column first!");
                valueNumComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                return;
            }
            if (valueNumTextBox.Text != "הזן ערך")
            {
                valueNumTextBox.ForeColor = Color.Silver;
                valueNumTextBox.Text = "הזן ערך";
            }
            if (valueNumComboBox.Text == null)
            {
                valueNumComboBox.Text = "הזן ערך";
                valueNumComboBox.ForeColor = Color.Silver;
            }
            else if (valueNumComboBox.Text != "הזן ערך")
            {
                valueNumComboBox.Text = "הזן ערך";
                valueNumComboBox.ForeColor = Color.Silver;
            }
            valueNumTextBox.Visible = false;
            valueNumComboBox.Visible = true;
            valueNumComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            valueNumComboBox.BackColor = Color.White;
            valueNumComboBox.Cursor = Cursors.Hand;

            string query = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + sourcesListBox.SelectedItem + "' AND COLUMN_NAME = '" + columnNum.SelectedItem + "'";
            // Show a new source table (with the CSV file data) in the gridview
            string columnDataType = "";
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();

                    using (var command = new SqlCommand(query, sqlCon))
                    {
                        columnDataType = (string)command.ExecuteScalar();
                    }
                }

                catch (Exception ex)
                {
                    /*Handle error*/
                }
                finally
                {
                    sqlCon.Close();
                }
            }
            query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + sourcesListBox.SelectedItem + "' AND DATA_TYPE = '" + columnDataType + "' AND COLUMN_NAME != '" + columnNum.SelectedItem + "'";
            fillComboBox(query, "COLUMN_NAME", valueNumComboBox);
            if (!valueNumComboBox.Items.Contains("הזן ערך"))
            {
                valueNumComboBox.Items.Add("הזן ערך");
            }
            valueNumComboBox.Text = "הזן ערך";
        }

        private void switchFromColumnToValueForValueNum(Button valueBtnForValueNum, Button columnBtnForValue1_FilterPaneNum,
                  ComboBox columnNum, TextBox valueNumTextBox, ComboBox valueNumComboBox, ListBox OpenListBoxValueNum)
        {
            // Finish function if columnNum is empty
            switchBetweenColumnAndValueColorsForValueProperty(valueBtnForValueNum, columnBtnForValue1_FilterPaneNum);
            if (columnNum.SelectedItem == null)
            {
                return;
            }
            // Initialization of ValueNum controls
            valueNumTextBox.Visible = true;
            if (valueNumComboBox.Text == null)
            {
                valueNumComboBox.Text = "הזן ערך";
                valueNumComboBox.ForeColor = Color.Silver;
            }
            else if (valueNumTextBox.Text != "הזן ערך")
            {
                valueNumTextBox.Text = "הזן ערך";
                valueNumTextBox.ForeColor = Color.Silver;
            }
            if (valueNumTextBox.Text != "הזן ערך")
            {
                valueNumTextBox.ForeColor = Color.Silver;
            }
          
        }

        private void fllValueOptionsForValueNum(Panel panelForListBoxValueNum, ListBox OpenListBoxValueNum, ComboBox columnNum)
        {
              // Use a query to get the list from the database and show it in  OpenListBoxValueNum_FilterPanel
            OpenListBoxValueNum.Items.Clear();
            panelForListBoxValueNum.Visible = true;
            OpenListBoxValueNum.BringToFront();
            string query;
            if (filterQuery != "")
            {
                string search = "where";
                string currFilter = filterQuery.Substring(filterQuery.IndexOf(search) + search.Length);
                query = "select DISTINCT " + columnNum.SelectedItem + " from " + sourcesListBox.SelectedItem + " Where "
                    + columnNum.SelectedItem + " IS NOT NULL AND " + currFilter + " ORDER BY " + columnNum.SelectedItem;
            }
            else
            {
                query = "select DISTINCT " + columnNum.SelectedItem + " from " + sourcesListBox.SelectedItem + " Where "
                      + columnNum.SelectedItem + " IS NOT NULL ORDER BY " + columnNum.SelectedItem;
            }

            SqlDataReader myreader; ;
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlCommand SelectCommand = new SqlCommand(query, sqlCon);
                    myreader = SelectCommand.ExecuteReader();

                    List<String> values = new List<String>();
                    while (myreader.Read())
                    {
                        values.Add(myreader[0].ToString());
                    }

                    foreach (string currValue in values)
                    {
                        OpenListBoxValueNum.Items.Add(currValue);
                    }
                }

                catch (Exception ex)
                {
                    /*Handle error*/
                }
                finally
                {
                    sqlCon.Close();
                }


            }


        }

        // A function that switch the color between column and value for Value property
        private void switchBetweenColumnAndValueColorsForValueProperty(Button clickedBtn, Button secondBtn)
        {
            clickedBtn.BackColor = Color.FromArgb(92, 130, 205);
            clickedBtn.ForeColor = Color.Black;
            clickedBtn.Font = new Font(columnBtnForValue1_FilterPanel.Font, FontStyle.Bold);
            secondBtn.BackColor = Color.Silver;
            secondBtn.ForeColor = SystemColors.ButtonFace;
            secondBtn.Font = new Font(valueBtnForValue1_FilterPanel.Font, FontStyle.Regular);
        }

        const int WM_PARENTNOTIFY = 0x210;
        const int WM_LBUTTONDOWN = 0x201;
        // Close All ListBoxes when they lose focus
        protected override void WndProc(ref Message m)
        {
            /*if(OpenListBoxValue4_FilterPanel.Visible == true)
            {
                OpenListBoxValue4_FilterPanel.Hide();
            }*/
            if (m.Msg == WM_LBUTTONDOWN || (m.Msg == WM_PARENTNOTIFY && (int)m.WParam == WM_LBUTTONDOWN))
            {
                Point p = System.Windows.Forms.Control.MousePosition;

                if (!panelForListBoxValue1_FilterPanel.ClientRectangle.Contains(panelForListBoxValue1_FilterPanel.PointToClient(Cursor.Position)))
                {
                    panelForListBoxValue1_FilterPanel.Hide();
                }
                if (!panelForListBoxValue2_FilterPanel.ClientRectangle.Contains(panelForListBoxValue2_FilterPanel.PointToClient(Cursor.Position)))
                {
                    panelForListBoxValue2_FilterPanel.Hide();
                }
                if (!panelForListBoxValue3_FilterPanel.ClientRectangle.Contains(panelForListBoxValue3_FilterPanel.PointToClient(Cursor.Position)))
                {
                    panelForListBoxValue3_FilterPanel.Hide();
                }
                if (!panelForListBoxValue4_FilterPanel.ClientRectangle.Contains(panelForListBoxValue4_FilterPanel.PointToClient(Cursor.Position)))
                {
                    panelForListBoxValue4_FilterPanel.Hide();
                }
                if (!panelForListBoxValue5_FilterPanel.ClientRectangle.Contains(panelForListBoxValue5_FilterPanel.PointToScreen(Cursor.Position)))
                {
                    panelForListBoxValue5_FilterPanel.Hide();
                }
            }
            base.WndProc(ref m);
        }


      
      


        /*======================= Panel 1 Controls =======================*/

        // Paint borders for panel1
        private void panel1_FilterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (panel1_FilterPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 3;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel1_FilterPanel.ClientSize.Width - thickness,
                                                              panel1_FilterPanel.ClientSize.Height - thickness));
                }
            }
        }

        //Click on addFilterBtn1 open filter panel2 and Initializing it
        private void addFilterBtn1_FilterPanel_Click_1(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel++;
            operator1_FilterPanel.Visible = true;
            panel2_FilterPanel.Visible = true;
            xBtn1_FilterPanel.Visible = true;
            addFilterBtn2_FilterPanel.Visible = true;
            addFilterBtn1_FilterPanel.Visible = false;

            //set operator1 columns (AND / OR)
            operator1_FilterPanel.DataSource = new List<string>(new string[] { "AND", "OR" });
            operator1_FilterPanel.Text = "סוג הצירוף";

            //set condition2 columns (=, >, <...)
            //condition2_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition2_FilterPanel.Text = "בחר תנאי";

            //set value2 placeholder
            value2ComboBox_FilterPanel.Text = "הזן ערך..";
        }

        //Click on xBtn1 close filter panel2 
        private void xBtn1_FilterPanel_Click(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel--;
            operator1_FilterPanel.Visible = false;
            xBtn1_FilterPanel.Visible = false;
            panel2_FilterPanel.Visible = false;
            addFilterBtn2_FilterPanel.Visible = false;
            addFilterBtn1_FilterPanel.Visible = true;

        }

        // Set off placeholder on column1
        private void column1_FilterPanel_Enter(object sender, EventArgs e)
        {
            column1_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on column1
        private void column1_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (column1_FilterPanel.Text == "בחר עמודה")
            {
                column1_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on condition1
        private void condition1_FilterPanel_Enter(object sender, EventArgs e)
        {
            condition1_FilterPanel.ForeColor = Color.Black;

        }

        // Set on placeholder on condition1
        private void condition1_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (condition1_FilterPanel.Text == "בחר תנאי")
            {
                condition1_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Handle 'IS NOT NULL' if selected by condition1
        private void condition1_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (condition1_FilterPanel.SelectedItem.ToString() == "IS NOT NULL")
                {
                    value1ComboBox_FilterPanel.Visible = false;
                    value1TextBox_FilterPanel.Visible = false;
                    columnBtnForValue1_FilterPanel.Visible = false;
                    valueBtnForValue1_FilterPanel.Visible = false;
                }
                else if(!value1ComboBox_FilterPanel.Visible == true)
                {
                    value1ComboBox_FilterPanel.Visible = true;
                    value1TextBox_FilterPanel.Visible = true;
                    columnBtnForValue1_FilterPanel.Visible = true;
                    valueBtnForValue1_FilterPanel.Visible = true;
                }
            }
            catch (Exception) { }

        }




        // Set off placeholder on value1ComboBox
        private void value1ComboBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            setOffPlaceholderForValueNumComboBox(value1ComboBox_FilterPanel);
        }

        // Set on placeholder on value1ComboBox
        private void value1ComboBox_FilterPanel_Leave(object sender, EventArgs e)
        {
            setOnPlaceholderForValueNumComboBox(value1ComboBox_FilterPanel);

        }

        // Switch from value to column for Value1
        private void columnBtnForValue1_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue1IsActive = true;
            switchFromValuToColumnForValueNum(columnBtnForValue1_FilterPanel, valueBtnForValue1_FilterPanel,
            column1_FilterPanel, value1TextBox_FilterPanel, value1ComboBox_FilterPanel);        
            value1TextBox_FilterPanel.Visible = false;
            value1ComboBox_FilterPanel.Visible = true;
        }

      
        // Switch from column to value for Value1 and show a list of suggestions values from the table
        private void valueBtnForValue1_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue1IsActive = false;
            switchFromColumnToValueForValueNum(valueBtnForValue1_FilterPanel, columnBtnForValue1_FilterPanel,
                column1_FilterPanel, value1TextBox_FilterPanel, value1ComboBox_FilterPanel, OpenListBoxValue1_FilterPanel);
            value1TextBox_FilterPanel.Visible = true;
            value1ComboBox_FilterPanel.Visible = false;
        }

        
        // Initialization of Value1 
        private void column1_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
             if (!value1ComboBox_FilterPanel.Visible)
             {
                value1ComboBox_FilterPanel.Visible = true;
                value1TextBox_FilterPanel.Visible = true;
                columnBtnForValue1_FilterPanel.Visible = true;
                valueBtnForValue1_FilterPanel.Visible = true;
            }

            if (columnBtnForValue1IsActive)
            {
                valueBtnForValue1_FilterPanel_Click(sender, e);
                columnBtnForValue1_FilterPanel_Click(sender, e);
            }
            else
            {
                columnBtnForValue1_FilterPanel_Click(sender, e);
                valueBtnForValue1_FilterPanel_Click(sender, e);
            }
         
        }

      


        //Open ListBoxValue1_FilterPanel to get values for value1
        private void value1TextBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            fllValueOptionsForValueNum(panelForListBoxValue1_FilterPanel, OpenListBoxValue1_FilterPanel, column1_FilterPanel);
            OpeListBoxValueNum(value1TextBox_FilterPanel, OpenListBoxValue1_FilterPanel);
         }
            

        //Get ListBoxValue1_FilterPanel clicked item for value1
        private void OpenListBoxValue1_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            getSelectedValueFromListBoxValueNum(value1TextBox_FilterPanel, OpenListBoxValue1_FilterPanel, panelForListBoxValue1_FilterPanel);
        }

        // Open the ListBoxValue1 for every click inside value1TextBox 
        private void value1TextBox_FilterPanel_Click(object sender, EventArgs e)
        {
            value1TextBox_FilterPanel_Enter(sender, e);
        }


        /*======================= Panel 2 Controls =======================*/

        // Paint borders for panel2
        private void panel2_FilterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (panel2_FilterPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel2_FilterPanel.ClientSize.Width - thickness,
                                                              panel2_FilterPanel.ClientSize.Height - thickness));
                }
            }
        }

        //Click on addFilterBtn2 open filter panel3 and Initializing it
        private void addBtn2FilterPanel_Click_1(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel++;
            xBtn1_FilterPanel.Visible = false;
            operator2_FilterPanel.Visible = true;
            panel3_FilterPanel.Visible = true;
            xBtn2_FilterPanel.Visible = true;
            addFilterBtn3_FilterPanel.Visible = true;
            addFilterBtn2_FilterPanel.Visible = false;

            //set operator2 columns (AND / OR)
            operator2_FilterPanel.DataSource = new List<string>(new string[] { "AND", "OR" });
            operator2_FilterPanel.Text = "סוג הצירוף";

            //set condition3 columns (=, >, <...)
            //condition3_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition3.DataSource = conditionsList;
            //condition3_FilterPanel.Text = "בחר תנאי";

            //set value3 placeholder
            value3ComboBox_FilterPanel.Text = "הזן ערך..";
            filterPanel.AutoScroll = true;

        }

        //Click on xBtn2 close filter panel3 
        private void xBtn2_FilterPanel_Click(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel--;
            xBtn1_FilterPanel.Visible = true;
            xBtn2_FilterPanel.Visible = false;
            operator2_FilterPanel.Visible = false;
            panel3_FilterPanel.Visible = false;
            addFilterBtn3_FilterPanel.Visible = false;
            addFilterBtn2_FilterPanel.Visible = true;
        }

        // Set off placeholder on column2
        private void column2_FilterPanel_Enter(object sender, EventArgs e)
        {
            column2_FilterPanel.ForeColor = Color.Black;

        }

        // Set on placeholder on column2
        private void column2_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (column2_FilterPanel.Text == "בחר עמודה")
            {
                column2_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on condition2
        private void condition2_FilterPanel_Enter(object sender, EventArgs e)
        {
            condition2_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on condition2
        private void condition2_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (condition2_FilterPanel.Text == "בחר תנאי")
            {
                condition2_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Handle 'IS NOT NULL' if selected by condition2
        private void condition2_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (condition2_FilterPanel.SelectedItem.ToString() == "IS NOT NULL")
                {
                    value2ComboBox_FilterPanel.Visible = false;
                    value2TextBox_FilterPanel.Visible = false;
                    columnBtnForValue2_FilterPanel.Visible = false;
                    valueBtnForValue2_FilterPanel.Visible = false;
                }
                else if (!value2ComboBox_FilterPanel.Visible == true)
                {
                    value2ComboBox_FilterPanel.Visible = true;
                    value2TextBox_FilterPanel.Visible = true;
                    columnBtnForValue2_FilterPanel.Visible = true;
                    valueBtnForValue2_FilterPanel.Visible = true;
                }
            }
            catch (Exception) { }
        }

     


        // Set off placeholder on value2ComboBox
        private void value2ComboBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            setOffPlaceholderForValueNumComboBox(value2ComboBox_FilterPanel);
        }

        // Set on placeholder on value2ComboBox
        private void value2ComboBox_FilterPanel_Leave(object sender, EventArgs e)
        {
            setOnPlaceholderForValueNumComboBox(value2ComboBox_FilterPanel);
        }

        // Switch from value to column for Value2
        private void columnBtnForValue2_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue2IsActive = true;
            switchFromValuToColumnForValueNum(columnBtnForValue2_FilterPanel, valueBtnForValue2_FilterPanel,
            column2_FilterPanel, value2TextBox_FilterPanel, value2ComboBox_FilterPanel);
            value2TextBox_FilterPanel.Visible = false;
            value2ComboBox_FilterPanel.Visible = true;
        }

        // Switch from column to value for Value2 and show a list of suggestions values from the table
        private void valueBtnForValue2_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue2IsActive = false;
            switchFromColumnToValueForValueNum(valueBtnForValue2_FilterPanel, columnBtnForValue2_FilterPanel,
                column2_FilterPanel, value2TextBox_FilterPanel, value2ComboBox_FilterPanel, OpenListBoxValue2_FilterPanel);
            value2TextBox_FilterPanel.Visible = true;
            value2ComboBox_FilterPanel.Visible = false;
        }

        //Get ListBoxValue2_FilterPanel clicked item for value1
        private void OpenListBoxValue2_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            getSelectedValueFromListBoxValueNum(value2TextBox_FilterPanel, OpenListBoxValue2_FilterPanel, panelForListBoxValue2_FilterPanel);

        }

        //Open ListBoxValue2_FilterPanel to get values for value1
        private void value2TextBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            fllValueOptionsForValueNum(panelForListBoxValue2_FilterPanel,  OpenListBoxValue2_FilterPanel, column2_FilterPanel);
            OpeListBoxValueNum(value2TextBox_FilterPanel, OpenListBoxValue2_FilterPanel);
        }

        // Initialization of Value2
        private void column2_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!value2ComboBox_FilterPanel.Visible)
            {
                value2ComboBox_FilterPanel.Visible = true;
                value2TextBox_FilterPanel.Visible = true;
                columnBtnForValue2_FilterPanel.Visible = true;
                valueBtnForValue2_FilterPanel.Visible = true;
            }

            if (columnBtnForValue2IsActive)
            {
                valueBtnForValue2_FilterPanel_Click(sender, e);
                columnBtnForValue2_FilterPanel_Click(sender, e);
            }
            else
            {
                columnBtnForValue2_FilterPanel_Click(sender, e);
                valueBtnForValue2_FilterPanel_Click(sender, e);
            }
        }


        // Open the ListBoxValue2 for every click inside value2TextBox 
        private void value2TextBox_FilterPanel_Click(object sender, EventArgs e)
        {
            value2TextBox_FilterPanel_Enter(sender, e);
        }

       



        /*======================= Panel 3 Controls =======================*/

        // Paint borders for panel3
        private void panel3_FilterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (panel3_FilterPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel3_FilterPanel.ClientSize.Width - thickness,
                                                              panel3_FilterPanel.ClientSize.Height - thickness));
                }
            }
        }

        //Click on addFilterBtn3 open filter panel4 and Initializing it
        private void addBtn3_FilterPanel_Click_1(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel++;
            xBtn2_FilterPanel.Visible = false;
            operator3_FilterPanel.Visible = true;
            panel4_FilterPanel.Visible = true;
            xBtn3_FilterPanel.Visible = true;
            addFilterBtn3_FilterPanel.Visible = false;
            addFilterBtn4_FilterPanel.Visible = true;
   

            //set operator3 columns (AND / OR)
            //operator3.DataSource = operatorsList;
            operator3_FilterPanel.DataSource = new List<string>(new string[] { "AND", "OR" });
            operator3_FilterPanel.Text = "סוג הצירוף";

            //set condition4 columns (=, >, <...)
            //condition4_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition4_FilterPanel.Text = "בחר תנאי";

            //set value4 placeholder
            value4ComboBox_FilterPanel.Text = "הזן ערך..";

            // Handle scrolling operation for filter3
            filterPanel.AutoScroll = true;
            filterPanel.SetAutoScrollMargin(0, 120);
            ScrollToBottom(filterPanel);

        }

        //Click on xBtn3 close filter panel4 
        private void xBtn3_FilterPanel_Click(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel--;
            xBtn2_FilterPanel.Visible = true;
            xBtn3_FilterPanel.Visible = false;
            operator3_FilterPanel.Visible = false;
            panel4_FilterPanel.Visible = false;
            addFilterBtn3_FilterPanel.Visible = true;
            panel5_FilterPanel.Visible = false;
            addFilterBtn4_FilterPanel.Visible = false;

            // Set autoscroll if the user open the values list box
            filterPanel.AutoScroll = false; ;
        }

        // Set off placeholder on column3
        private void column3_FilterPanel_Enter(object sender, EventArgs e)
        {
            column3_FilterPanel.ForeColor = Color.Black;

        }

        // Set on placeholder on column3
        private void column3_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (column3_FilterPanel.Text == "בחר עמודה")
            {
                column3_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on condition3
        private void condition3_FilterPanel_Enter(object sender, EventArgs e)
        {
            condition3_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on condition3
        private void condition3_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (condition3_FilterPanel.Text == "בחר תנאי")
            {
                condition3_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on value3
        private void value3_FilterPanel_Enter(object sender, EventArgs e)
        {
            if (value3ComboBox_FilterPanel.Text == "הזן ערך..")
            {
                value3ComboBox_FilterPanel.Text = "";
                value3ComboBox_FilterPanel.ForeColor = Color.Black;
            }
        }

        // Set on placeholder on value3
        private void value3_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(value3ComboBox_FilterPanel.Text))
            {
                value3ComboBox_FilterPanel.Text = "הזן ערך..";
                value3ComboBox_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Handle 'IS NOT NULL' if selected by condition3
        private void condition3_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (condition3_FilterPanel.SelectedItem.ToString() == "IS NOT NULL")
                {
                    value3ComboBox_FilterPanel.Visible = false;
                    value3TextBox_FilterPanel.Visible = false;
                    columnBtnForValue3_FilterPanel.Visible = false;
                    valueBtnForValue3_FilterPanel.Visible = false;
                }
                else if (!value3ComboBox_FilterPanel.Visible == true)
                {
                    value3ComboBox_FilterPanel.Visible = true;
                    value3TextBox_FilterPanel.Visible = true;
                    columnBtnForValue3_FilterPanel.Visible = true;
                    valueBtnForValue3_FilterPanel.Visible = true;
                }
            }
            catch (Exception) { }

        }

        // Set off placeholder on value3ComboBox
        private void value3ComboBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            setOffPlaceholderForValueNumComboBox(value3ComboBox_FilterPanel);
        }

        // Set on placeholder on value3ComboBox
        private void value3ComboBox_FilterPanel_Leave(object sender, EventArgs e)
        {
            setOnPlaceholderForValueNumComboBox(value3ComboBox_FilterPanel);
        }

        // Switch from value to column for Value3
        private void columnBtnForValue3_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue3IsActive = true;
            switchFromValuToColumnForValueNum(columnBtnForValue3_FilterPanel, valueBtnForValue3_FilterPanel,
            column3_FilterPanel, value3TextBox_FilterPanel, value3ComboBox_FilterPanel);
            value3TextBox_FilterPanel.Visible = false;
            value3ComboBox_FilterPanel.Visible = true;       
        }

        // Switch from column to value for Value3 and show a list of suggestions values from the table
        private void valueBtnForValue3_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue3IsActive = false;
            switchFromColumnToValueForValueNum(valueBtnForValue3_FilterPanel, columnBtnForValue3_FilterPanel,
                column3_FilterPanel, value3TextBox_FilterPanel, value3ComboBox_FilterPanel, OpenListBoxValue3_FilterPanel);
            value3TextBox_FilterPanel.Visible = true;
            value3ComboBox_FilterPanel.Visible = false;
        }

        //Get ListBoxValue3_FilterPanel clicked item for value1
        private void OpenListBoxValue3_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            getSelectedValueFromListBoxValueNum(value3TextBox_FilterPanel, OpenListBoxValue3_FilterPanel, panelForListBoxValue3_FilterPanel);
        }

        //Open ListBoxValue3_FilterPanel to get values for value1
        private void value3TextBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            fllValueOptionsForValueNum(panelForListBoxValue3_FilterPanel, OpenListBoxValue3_FilterPanel, column3_FilterPanel);
            OpeListBoxValueNum(value3TextBox_FilterPanel, OpenListBoxValue3_FilterPanel);
        }

        // Initialization of Value3
        private void column3_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!value3ComboBox_FilterPanel.Visible)
            {
                value3ComboBox_FilterPanel.Visible = true;
                value3TextBox_FilterPanel.Visible = true;
                columnBtnForValue3_FilterPanel.Visible = true;
                valueBtnForValue3_FilterPanel.Visible = true;
            }

            if (columnBtnForValue3IsActive)
            {
                valueBtnForValue3_FilterPanel_Click(sender, e);
                columnBtnForValue3_FilterPanel_Click(sender, e);
            }
            else
            {
                columnBtnForValue3_FilterPanel_Click(sender, e);
                valueBtnForValue3_FilterPanel_Click(sender, e);
            }
        }


        // Open the ListBoxValue3 for every click inside value3TextBox 
        private void value3TextBox_FilterPanel_Click(object sender, EventArgs e)
        {
            value3TextBox_FilterPanel_Enter(sender, e);
        }

       

        /*======================= Panel 4 Controls =======================*/

        // Paint borders for panel4
        private void panel4_FilterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (panel4_FilterPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel4_FilterPanel.ClientSize.Width - thickness,
                                                              panel4_FilterPanel.ClientSize.Height - thickness));
                }
            }
        }

        //Click on addFilterBtn4 open filter panel5 and Initializing it
        private void addBtn4_FilterPanel_Click(object sender, EventArgs e)
        {
            filterPanel.SetAutoScrollMargin(0, 0);
            panelsCounter_FilterPanel++;
            xBtn3_FilterPanel.Visible = false;
            operator4_FilterPanel.Visible = true;
            panel5_FilterPanel.Visible = true;
            xBtn4_FilterPanel.Visible = true;
            addFilterBtn4_FilterPanel.Visible = false;

            //set operator4 columns (AND / OR)
            //operator3.DataSource = operatorsList;
            operator4_FilterPanel.DataSource = new List<string>(new string[] { "AND", "OR" });
            operator4_FilterPanel.Text = "סוג הצירוף";

            //set condition5 columns (=, >, <...)
            //condition5_FilterPanel.DataSource = new List<string>(new string[] { "=", "!=", ">", "<", ">=", "<=", "IS NOT NULL" });
            //condition5_FilterPanel.Text = "בחר תנאי";

            //set value4 placeholder
            value5ComboBox_FilterPanel.Text = "הזן ערך..";
            filterPanel.SetAutoScrollMargin(0, 120);

        }

        // Scrolling down the filter panel when filter4 is opening
        public void ScrollToBottom(Panel p)
        {
            using (Control c = new Control() { Parent = p, Dock = DockStyle.Bottom })
            {
                p.ScrollControlIntoView(c);
                c.Parent = null;
            }
        }

        //Click on xBtn4 close filter panel5
        private void xBtn4_FilterPanel_Click(object sender, EventArgs e)
        {
            panelsCounter_FilterPanel--;
            xBtn3_FilterPanel.Visible = true;
            xBtn4_FilterPanel.Visible = false;
            operator4_FilterPanel.Visible = false;
            panel5_FilterPanel.Visible = false;
            addFilterBtn4_FilterPanel.Visible = true;

        }

        // Set off placeholder on column4
        private void column4_FilterPanel_Enter(object sender, EventArgs e)
        {
            column4_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on column4
        private void column4_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (column4_FilterPanel.Text == "בחר עמודה")
            {
                column4_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on condition4
        private void condition4_FilterPanel_Enter(object sender, EventArgs e)
        {
            condition4_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on condition4
        private void condition4_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (condition4_FilterPanel.Text == "בחר תנאי")
            {
                condition4_FilterPanel.ForeColor = Color.Silver;
            }
        }
     
        // Handle 'IS NOT NULL' if selected by condition4
        private void condition4_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (condition4_FilterPanel.SelectedItem.ToString() == "IS NOT NULL")
                {
                    value4ComboBox_FilterPanel.Visible = false;
                    value4TextBox_FilterPanel.Visible = false;
                    columnBtnForValue4_FilterPanel.Visible = false;
                    valueBtnForValue4_FilterPanel.Visible = false;
                }
                else if (!value4ComboBox_FilterPanel.Visible == true)
                {
                    value4ComboBox_FilterPanel.Visible = true;
                    value4TextBox_FilterPanel.Visible = true;
                    columnBtnForValue4_FilterPanel.Visible = true;
                    valueBtnForValue4_FilterPanel.Visible = true;
                }
            }
            catch (Exception) { }

        }


        // Set off placeholder on value4ComboBox
        private void value4ComboBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            setOffPlaceholderForValueNumComboBox(value4ComboBox_FilterPanel);
        }

        // Set on placeholder on value4ComboBox
        private void value4ComboBox_FilterPanel_Leave(object sender, EventArgs e)
        {
            setOnPlaceholderForValueNumComboBox(value2ComboBox_FilterPanel);
        }

        // Switch from value to column for Value4
        private void columnBtnForValue4_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue4IsActive = true;
            switchFromValuToColumnForValueNum(columnBtnForValue4_FilterPanel, valueBtnForValue4_FilterPanel,
            column4_FilterPanel, value4TextBox_FilterPanel, value4ComboBox_FilterPanel);
            value4TextBox_FilterPanel.Visible = false;
            value4ComboBox_FilterPanel.Visible = true;

        }

        // Switch from column to value for Value4 and show a list of suggestions values from the table
        private void valueBtnForValue4_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue4IsActive = false;
            switchFromColumnToValueForValueNum(valueBtnForValue4_FilterPanel, columnBtnForValue4_FilterPanel,
                column4_FilterPanel, value4TextBox_FilterPanel, value4ComboBox_FilterPanel, OpenListBoxValue4_FilterPanel);
            value4TextBox_FilterPanel.Visible = true;
            value4ComboBox_FilterPanel.Visible = false;
        }

        //Get ListBoxValue4_FilterPanel clicked item for value4
        private void OpenListBoxValue4_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            getSelectedValueFromListBoxValueNum(value4TextBox_FilterPanel, OpenListBoxValue4_FilterPanel, panelForListBoxValue4_FilterPanel);

        }

        //Open ListBoxValue4_FilterPanel to get values for value4
        private void value4TextBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            fllValueOptionsForValueNum(panelForListBoxValue4_FilterPanel, OpenListBoxValue4_FilterPanel, column4_FilterPanel);
            OpeListBoxValueNum(value4TextBox_FilterPanel, OpenListBoxValue4_FilterPanel);
        }

        // Initialization of Value4
        private void column4_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!value4ComboBox_FilterPanel.Visible)
            {
                value4ComboBox_FilterPanel.Visible = true;
                value4TextBox_FilterPanel.Visible = true;
                columnBtnForValue4_FilterPanel.Visible = true;
                valueBtnForValue4_FilterPanel.Visible = true;
            }

            if (columnBtnForValue4IsActive)
            {
                valueBtnForValue4_FilterPanel_Click(sender, e);
                columnBtnForValue4_FilterPanel_Click(sender, e);
            }
            else
            {
                columnBtnForValue4_FilterPanel_Click(sender, e);
                valueBtnForValue4_FilterPanel_Click(sender, e);
            }
        }

        // Open the ListBoxValue4 for every click inside value4TextBox 
        private void value4TextBox_FilterPanel_Click(object sender, EventArgs e)
        {
            value4TextBox_FilterPanel_Enter(sender, e);
        }

       

        /*======================= Panel 5 Controls =======================*/

        // Paint borders for panel5
        private void panel5_FilterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (panel5_FilterPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 2;  //it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.White, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel5_FilterPanel.ClientSize.Width - thickness,
                                                              panel5_FilterPanel.ClientSize.Height - thickness));
                }
            }
        }

        // Set off placeholder on column5
        private void column5_FilterPanel_Enter(object sender, EventArgs e)
        {
            column5_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on column5
        private void column5_FilterPanel_Leave(object sender, EventArgs e)
        {
            if (column5_FilterPanel.Text == "בחר עמודה")
            {
                column5_FilterPanel.ForeColor = Color.Silver;
            }
        }

        // Set off placeholder on condition5
        private void condition5_FilterPanel_Enter(object sender, EventArgs e)
        {
            condition5_FilterPanel.ForeColor = Color.Black;
        }

        // Set on placeholder on condition5
        private void condition5FilterPanel_Leave(object sender, EventArgs e)
        {
            if (condition5_FilterPanel.Text == "בחר תנאי")
            {
                condition5_FilterPanel.ForeColor = Color.Silver;
            }
        }
        
        // Handle 'IS NOT NULL' if selected by condition5
        private void condition5_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (condition5_FilterPanel.SelectedItem.ToString() == "IS NOT NULL")
                {
                    value5ComboBox_FilterPanel.Visible = false;
                    value5TextBox_FilterPanel.Visible = false;
                    columnBtnForValue5_FilterPanel.Visible = false;
                    valueBtnForValue5_FilterPanel.Visible = false;
                }
                else if (!value5ComboBox_FilterPanel.Visible == true)
                {
                    value5ComboBox_FilterPanel.Visible = true;
                    value5TextBox_FilterPanel.Visible = true;
                    columnBtnForValue5_FilterPanel.Visible = true;
                    valueBtnForValue5_FilterPanel.Visible = true;
                }
            }
            catch (Exception) { }
        }


        // Set off placeholder on value5ComboBox
        private void value5ComboBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            setOffPlaceholderForValueNumComboBox(value5ComboBox_FilterPanel);
        }

        // Set on placeholder on value5ComboBox
        private void value5ComboBox_FilterPanel_Leave(object sender, EventArgs e)
        {
            setOnPlaceholderForValueNumComboBox(value5ComboBox_FilterPanel);
        }

        // Switch from value to column for Value5
        private void columnBtnForValue5_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue5IsActive = true;
            switchFromValuToColumnForValueNum(columnBtnForValue5_FilterPanel, valueBtnForValue5_FilterPanel,
            column5_FilterPanel, value5TextBox_FilterPanel, value5ComboBox_FilterPanel);
            value5TextBox_FilterPanel.Visible = false;
            value5ComboBox_FilterPanel.Visible = true;

        }

        // Switch from column to value for Value5 and show a list of suggestions values from the table
        private void valueBtnForValue5_FilterPanel_Click(object sender, EventArgs e)
        {
            columnBtnForValue5IsActive = false;
            switchFromColumnToValueForValueNum(valueBtnForValue5_FilterPanel, columnBtnForValue5_FilterPanel,
                column5_FilterPanel, value5TextBox_FilterPanel, value5ComboBox_FilterPanel, OpenListBoxValue5_FilterPanel);
            value5TextBox_FilterPanel.Visible = true;
            value5ComboBox_FilterPanel.Visible = false;
        }

        //Get ListBoxValue5_FilterPanel clicked item for value5
        private void OpenListBoxValue5_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            getSelectedValueFromListBoxValueNum(value5TextBox_FilterPanel, OpenListBoxValue5_FilterPanel, panelForListBoxValue5_FilterPanel);

        }

        //Open ListBoxValue5_FilterPanel to get values for value5
        private void value5TextBox_FilterPanel_Enter(object sender, EventArgs e)
        {
            fllValueOptionsForValueNum(panelForListBoxValue5_FilterPanel, OpenListBoxValue5_FilterPanel, column5_FilterPanel);
            OpeListBoxValueNum(value5TextBox_FilterPanel, OpenListBoxValue5_FilterPanel);
        }

        // Initialization of Value5
        private void column5_FilterPanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!value5ComboBox_FilterPanel.Visible)
            {
                value5ComboBox_FilterPanel.Visible = true;
                value5TextBox_FilterPanel.Visible = true;
                columnBtnForValue5_FilterPanel.Visible = true;
                valueBtnForValue5_FilterPanel.Visible = true;
            }

            if (columnBtnForValue5IsActive)
            {
                valueBtnForValue5_FilterPanel_Click(sender, e);
                columnBtnForValue5_FilterPanel_Click(sender, e);
            }
            else
            {
                columnBtnForValue5_FilterPanel_Click(sender, e);
                valueBtnForValue5_FilterPanel_Click(sender, e);
            }
        }

        // Open the ListBoxValue5 for every click inside value5TextBox 
        private void value5TextBox_FilterPanel_Click(object sender, EventArgs e)
        {
            value5TextBox_FilterPanel_Enter(sender, e);
        }

        #endregion Filter Paenl

        #region Save Table Panel Conrtols

        // Save table button click will open/close the textbox and save icon for every click
        private void saveTableBtn_Click(object sender, EventArgs e)
        {
            if (saveTableBtnIcon.Visible == false && textBoxForSaveTable.Visible == false)
            {
                saveTableBtnIcon.Visible = true;
                textBoxForSaveTable.Visible = true;
            }
            else
            {
                saveTableBtnIcon.Visible = false;
                textBoxForSaveTable.Visible = false;
            }
        }

        // Response to enter key inisde the textbox
        private void textBoxForSaveTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                saveTableBtnIcon_Click(sender, e);
            }
        }

        private void writeQueryBtn_Click(object sender, EventArgs e)
        {
            if (joinPanel.Visible)
                joinPanel.Visible = false;
            if (filterPanel.Visible)
                filterPanel.Visible = false;
            writeQueryPanel.Visible = true;
        }

        private void runBtn_writeQueryPanel_Click(object sender, EventArgs e)
        {

            string str = richTextBox_writeQueryPanel.Text;
            string query = str.Replace("\n", " ");

            //Show the source table in the gridview
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlData = new SqlDataAdapter(query, sqlCon);
                    DataTable dataTable = new DataTable();
                    sqlData.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;

                    saveTableBtn.Visible = true;
                    isOperationDone = true;
                    // Write Join records in the Log file
                    logFile.WritOperationInLogFile("Write Query", query, null, null);

                }

                catch (Exception)
                {
                    /*Handle error*/
                    MessageBox.Show("No results found!");
                }
                finally
                {
                    sqlCon.Close();
                }
            }
        }

        private void richTextBox_writeQueryPanel_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            richTextBox_writeQueryPanel.Height = e.NewRectangle.Height;
        }

        private void clearBtn_writeQueryPaneln_Click(object sender, EventArgs e)
        {
            richTextBox_writeQueryPanel.Text = "";
        }

        // Click the save icon will save a new table by the name entered in the saving textbox
        private void saveTableBtnIcon_Click(object sender, EventArgs e)
        {
            // Check if the new Table Name isn't empty or hilding a placeholder
            if (textBoxForSaveTable.Text != "הזן שם טבלה.." && textBoxForSaveTable.Text != "")
            {
                // Check if the new Table Name isn't in Hebrew
                if (Regex.IsMatch(textBoxForSaveTable.Text, @"^[א-ת]+$"))
                {
                    MessageBox.Show("Please insert an English Table Name!");
                    textBoxForSaveTable.Text = "הזן שם טבלה..";
                    textBoxForSaveTable.ForeColor = Color.Silver;
                    return;     // Finish function
                }

                // Use CSVFile Class
                CSVFile file = new CSVFile(this, null, null);
                //DataTable dt = new DataTable();


                //dt = (DataTable)dataGridView1.DataSource;       // Importing all the data of the showing grid table into a DataTable element

                DataTable dt = GetDataTableFromDGV(dataGridView1);

                // Use CSVFile Class function - insertDataTableIntoDatabase() to insert the DataTable into the database (return true if succeeded)
                if (file.insertDataTableIntoDatabase(dt, textBoxForSaveTable.Text))
                {
                    // Initialization of Join panel elements
                    MessageBox.Show("הטבלה נשמרה בהצלחה!\n");
                    string query = "SELECT name FROM sys.tables ORDER BY name";
                    fillComboBox(query, "name", sourceTableComboBox_JoinPanel);
                    sourceTableComboBox_JoinPanel.Text = "בחר טבלה";
                    fillComboBox(query, "name", targetTableComboBox_JoinPanel);
                    targetTableComboBox_JoinPanel.Text = "בחר טבלה";                                  
                    fillSourcesList(query);                                      // Initialization of fillSourcesList in order to get the new table in it
                    sourcesListBox.SelectedItem = textBoxForSaveTable.Text;     // And select the new Table
                    textBoxForSaveTable.Text = "הזן שם טבלה..";
                    textBoxForSaveTable.ForeColor = Color.Silver; saveTableBtn.Visible = false;
                    textBoxForSaveTable.Visible = false;
                    saveTableBtnIcon.Visible = false;
                }
            }
        }

        // Get a DataTable from the girdview on the screen
        private DataTable GetDataTableFromDGV(DataGridView dgv)
        {
            var dt = new DataTable();
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                if (column.Visible)
                {
                    // You could potentially name the column based on the DGV column name (beware of dupes)
                    // or assign a type based on the data type of the data bound to this DGV column.
                    dt.Columns.Add();
                }
            }
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                dt.Columns[i].ColumnName = dataGridView1.Columns[i].HeaderText;
            }

            object[] cellValues = new object[dgv.Columns.Count];
            foreach (DataGridViewRow row in dgv.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    cellValues[i] = row.Cells[i].Value;
                }
                dt.Rows.Add(cellValues);
            }

            return dt;
        }

        // Set on placeholder off textBoxForSaveTable
        private void textBoxForSaveTable_Enter(object sender, EventArgs e)
        {
            if (textBoxForSaveTable.Text == "הזן שם טבלה..")
            {
                textBoxForSaveTable.Text = "";
                textBoxForSaveTable.ForeColor = Color.Black;
            }
        }

        // Set off placeholder off textBoxForSaveTable
        private void textBoxForSaveTable_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxForSaveTable.Text))
            {
                textBoxForSaveTable.Text = "הזן שם טבלה..";
                textBoxForSaveTable.ForeColor = Color.Silver;
            }
        }
        #endregion
         
      
    }
}
