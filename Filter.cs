using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseSimulator
{
    class Filter
    {

        private MainForm mainForm;

        public Filter(MainForm form1)
        {
            // store the Form1 object reference
            mainForm = form1;
        }

        #region Filter Functions

        /* A function that get a specific filter panel details (column, condition, value) and return the suitable query for them. 
            Example for a query inputs:  " 'column' 'condition' 'value' " 
             */
        public string addFilterToQuery(string column, string condition, string value)
        {

            string str = "";

            //Checking if the value input is in a Datetime format in order to set a convert query to 'column'
            if (checkIfInputIsInDateTimeformat(value))
            {
                str += " convert(DATETIME, " + column + " , 103) ";
            }
            else
            str = " " + column + " " + condition;       // Regular query

        
            // Adding the value to the query if the condition != 'IS NOT NULL' (otherwise there is no value in the query)
            if (condition != "IS NOT NULL")
            {
                // Check if the value is in a Datetime format in order to add a convert query to 'value'
                if (checkIfInputIsInDateTimeformat(value))
                {
                    str += condition + " CONVERT(DATETIME, '" + value + "', 103)";
                }

                // Check if the input value is in an Float format in order to add it as an Int value to the query
                else if (Regex.IsMatch(value, @"^[0-9]*(?:\.[0-9]*)?$"))
                {
                    str += " convert(float, " + value + ")";
                }

                // Check if the input value is in an Int format in order to add it as an Int value to the query
                else if (Regex.IsMatch(value, @"^[0-9]+$"))
                {
                    str += " " + value;
                }

                // Check if the input value is in an String format in order to add it as an String value to the query
                else if (Regex.IsMatch(value, @"^[A-Za-z0-9][A-Za-z0-9\s]*[A-Za-z0-9]|[A-Za-z0-9]+$") || Regex.IsMatch(value, @"^[א-ת][א-ת\s]*[א-ת]+$"))
                {
                    str += " '" + value + "'";
                }

                // Use it as a string format as default
                else
                {
                    str += " '" + value + "'";
                }
            }
            return str;
        }

        // A function that fills the comboBox for values when the user choose a column name 
        public string addFilterToQueryForColumnInValueChoice(string sourceTable, string column1, string condition, string column2)
        {
            string query;
            if (condition != "IS NOT NULL")
            {
                query = "select * from " + sourceTable + " where " + column1 + " " + condition + " " + column2;
            }
            else
            {
                query = "select * from " + sourceTable + " where " + column1 + " " + condition;
            }
            return query;
        }

        // After the user flick on Run button in the Filter panel, this function will invoke and check if all fields have been successfully filled
        public bool checkIfAllFieldsAreFullInFilterPanel()
        {
            switch (mainForm.panelsCounter_FilterPanel)         // Checking how many filter panels are open in order to handle the number of required fields
            {
                case 1:
                    if (mainForm.value1ComboBox_FilterPanel.Visible)
                    {
                        if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף")
                            return false;
                    }
                    else if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף")
                        return false;
                    break;
                case 2:
                    if (mainForm.value2ComboBox_FilterPanel.Visible)
                    {
                        if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.value1ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.value2ComboBox_FilterPanel.Text == "הזן ערך..")
                            return false;
                    }
                    else if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null)
                        return false;
                    break;

                case 3:
                    if (mainForm.value3ComboBox_FilterPanel.Visible)
                    {
                        if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.value1ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.value2ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null || mainForm.value3ComboBox_FilterPanel.Text == "הזן ערך..")
                            return false;
                    }
                    else if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null)
                        return false;
                    break;
                case 4:
                    if (mainForm.value4ComboBox_FilterPanel.Visible)
                    {
                        if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.value1ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.value2ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null || mainForm.value3ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator3_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column4_FilterPanel.SelectedItem == null || mainForm.condition4_FilterPanel.SelectedItem == null || mainForm.value4ComboBox_FilterPanel.Text == "הזן ערך..")
                            return false;
                    }
                    else if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null || mainForm.operator3_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column4_FilterPanel.SelectedItem == null || mainForm.condition4_FilterPanel.SelectedItem == null)
                        return false;
                    break;
                case 5:
                    if (mainForm.value4ComboBox_FilterPanel.Visible)
                    {
                        if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.value1ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.value2ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null || mainForm.value3ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator3_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column4_FilterPanel.SelectedItem == null || mainForm.condition4_FilterPanel.SelectedItem == null || mainForm.value4ComboBox_FilterPanel.Text == "הזן ערך.." || mainForm.operator4_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column5_FilterPanel.SelectedItem == null || mainForm.condition5_FilterPanel.SelectedItem == null || mainForm.value5ComboBox_FilterPanel.Text == "הזן ערך..")
                            return false;
                    }
                    else if (mainForm.column1_FilterPanel.SelectedItem == null || mainForm.condition1_FilterPanel.SelectedItem == null || mainForm.operator1_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column2_FilterPanel.SelectedItem == null || mainForm.condition2_FilterPanel.SelectedItem == null || mainForm.operator2_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column3_FilterPanel.SelectedItem == null || mainForm.condition3_FilterPanel.SelectedItem == null || mainForm.operator3_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column4_FilterPanel.SelectedItem == null || mainForm.condition4_FilterPanel.SelectedItem == null || mainForm.operator4_FilterPanel.Text == "סוג הצירוף"
                            || mainForm.column5_FilterPanel.SelectedItem == null || mainForm.condition5_FilterPanel.SelectedItem == null)
                        return false;
                    break;
            }
            return true;
        }

        // Checking if the input string value is in a datetime format (Israeli format)
        public bool checkIfInputIsInDateTimeformat(string input)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(input, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "dd-M-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d-M-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "dd.M.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d.M.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "dd/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || DateTime.TryParseExact(input, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime)
              || (Regex.IsMatch(input, @"^\d{2,2}/\d{2,2}/\d{4,4} \d{2,2}:\d{2,2}:\d{2,2}"))
              || Regex.IsMatch(input, @"^(?:0?[1-9]|1[0-2])[./-](?:[012]?[0-9]|3[01])[./-](?:[0-9]{2}){1,2}$")
              || Regex.IsMatch(input,@"(\d+(?:\-\d+){2}\s\d+(?::\d+){2}\.\d+)\s([\s\S]*?)(?=$|\d+(?:\-\d+){2}\s\d+(?::\d+){2}\.\d+)"))
            {
                return true;
            }
            return false;
        }

        public int getNumOfVisibleFilterPanels()
        {
            int count = 1;
            if(mainForm.panel2_FilterPanel.Visible == true)
            {
                count++;
            }
            if (mainForm.panel3_FilterPanel.Visible == true)
            {
                count++;
            }
            if (mainForm.panel4_FilterPanel.Visible == true)
            {
                count++;
            }
            if (mainForm.panel4_FilterPanel.Visible == true)
            {
                count++;
            }
            return count;
        }

        #endregion Filter Functions

    }
}
