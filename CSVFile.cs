using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseSimulator
{
    public class CSVFile
    {
        private MainForm mainForm;
        private string csvFilePath;
        private string csvTableName;

        // Constructor
        public CSVFile(MainForm form1, string path, string name)
        {
            // store the Form1 object reference
            mainForm = form1;
            csvFilePath = path;
            csvTableName = name;
        }

        #region CSV file functions

        // This function will store the data from the csv file in a DataTable and return it
        public DataTable getDataFromCsvFile()
        {
           
            DataTable importedData = new DataTable();
            using (StreamReader sr = new StreamReader(csvFilePath))
            {
                int count = 0;
                string firstLine = sr.ReadLine();
                string str1 = string.Join("", firstLine.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                string[] strArray = str1.Split(',');

                var dt = new DataTable();
                string[] headersType = new string[strArray.Length];

                // Insert the headers to the DataTable - dt
                for (int i = 0; i < strArray.Length; i++)
                {
                        dt.Columns.Add(strArray[i]);  
                }
                firstLine = sr.ReadLine();
                strArray = firstLine.Split(',');

                // get all thw columns types by checking the second row (the data itself) and insert them into an array
                for (int i = 0; i < strArray.Length; i++)
                {
                    dt.Columns[i].DataType = getColumnType(strArray[i]);

                    headersType[i] = dt.Columns[i].DataType.ToString();     // An array string that holds all the table types
                }


                // Insert all the data into the DataTable - dt
                var sr2 = new StreamReader(csvFilePath);
                string line = sr2.ReadLine();
                string[] itemArray;
                Filter filter = new Filter(mainForm);
                do
                {
                    itemArray = line.Split(',');
                    if (count == 0)
                    {
                        count++;
                        line = sr2.ReadLine();
                        continue;
                    }
                    else
                    {
                        DataRow row = dt.NewRow();
                        for (int i = 0; i < itemArray.Length; i++)
                        {
                            if (filter.checkIfInputIsInDateTimeformat(itemArray[i]))
                            {
                                itemArray[i] = addTimeToDateTime(itemArray[i]);
                            }
                        }
                        row.ItemArray = itemArray;
                        dt.Rows.Add(row);
                    }
                    line = sr2.ReadLine();

                } while (!string.IsNullOrEmpty(line));


                // Creates a new DataTable - dtCloned, and insert the headers types into every column of it
                DataTable dtCloned = dt.Clone();
                dtCloned.Columns[0].DataType = typeof(Int32);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dtCloned.Columns[i].DataType = Type.GetType(headersType[i]);
                }

                // Copy dt into dtCloned
                foreach (DataRow row in dt.Rows)
                {
                    dtCloned.ImportRow(row);
                }

                // Set the final data
                importedData = dtCloned;
    
                return importedData;
            }
        }

        // If a csv column contains a date without a time, this function will add a time (00:00:00) for every cell in order to give this column a DateTime type
        public string addTimeToDateTime(string item)
        {
            DateTime myDate = new DateTime();
            try
            {
                myDate = DateTime.Parse(item, new CultureInfo("nl - NL", true));

                //myDate = DateTime.Parse(item, new CultureInfo("en-US", true));
            }
            catch (Exception){
                //myDate = DateTime.Parse(item, new CultureInfo("nl - NL", true));
            }


            if (myDate.TimeOfDay.TotalSeconds != 0)
            {
                //String has Date and Time
                return item;
            }
            else
            {
                //String has only Date Portion    
                myDate.Add(new TimeSpan(0, 0, 0, 0));
            }
            string newItem = myDate.ToString("dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            return newItem;
        }

        // Get a specific column data and return its type
        public System.Type getColumnType(string currColumn)
        {            
            Filter filter = new Filter(mainForm);
            int i;
            decimal dec;
            DateTime dt;

            byte by;
            if (int.TryParse(currColumn, out i)) return typeof(int);
            if (decimal.TryParse(currColumn, out dec)) return typeof(decimal);

            if (DateTime.TryParse(currColumn, out dt) || filter.checkIfInputIsInDateTimeformat(currColumn))
            {
                return typeof(DateTime);
                }
            if (byte.TryParse(currColumn, out by)) return typeof(byte);
            else
                return typeof(string);
            /*
            switch (currColumn)
            {
                case "System.Int32":
                    return typeof(int);
                case "System.Int64":
                    return typeof(int);
                case "System.Int16":
                    return typeof(int);
                case "System.Byte":
                    return typeof(byte);
                case "System.Decimal":
                    return typeof(decimal);
                case "System.DateTime":
                    return typeof(DateTime);
                case "System.String":
                default:
                    return typeof(string);
           }*/
        }


        // Get a DataTable and store it into the database with by input name
        public bool insertDataTableIntoDatabase(DataTable importedData, string tableName)
        {

            using (SqlConnection sqlCon = new SqlConnection(mainForm.connectionString))
            {
                string query = CreateTable(importedData, tableName);
                sqlCon.Open();

                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlCon))
                {
                    try
                    {
                        //SqlCommand cmd = new SqlCommand(query, sqlCon);
                        //cmd.ExecuteNonQuery();

                        //var bc = new SqlBulkCopy(sqlCon, SqlBulkCopyOptions.TableLock, null)
                        //{
                        //    DestinationTableName = tableName,
                        //    BatchSize = importedData.Rows.Count
                        //};
                        //bc.WriteToServer(importedData);

                        SqlCommand cmd = new SqlCommand(query, sqlCon);
                        cmd.ExecuteNonQuery();

                        sqlBulkCopy.DestinationTableName = tableName;
                        sqlBulkCopy.WriteToServer(importedData);

                        

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("The file could not be read.\n " + e.Message);
                        return false;
                    }
                    finally
                    {
                        sqlCon.Close();
                    }
                }
            }
            return true;
        }

        //Get the right query for creating a new table from a DataTable 
        public string CreateTable(DataTable table, string tableName)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += " [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Single":
                        sqlsc += " float ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.Boolean":
                    sqlsc += " bit ";
                    break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + ")";
        }
        



        #endregion CSV file functions


    }

}
