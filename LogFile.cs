using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseSimulator
{
    public class LogFile
    {
        private MainForm mainForm;    
        private string m_exePath = string.Empty;
        StreamWriter txtWriter;
        static string path;

        public LogFile(MainForm form1, string userID, string dbName)
        {
            // store the Form1 object reference
            mainForm = form1;
            CreateLogFile(userID, dbName);
        }

        public void CreateLogFile(string userID, string dbName)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = System.IO.Directory.GetFiles(m_exePath, "*.txt");
            if (files.Length != 0)
            {
                string resultString;
                int filesCounter = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    resultString = Regex.Match(files[i], @"\d+").Value;
                    if(filesCounter < Int32.Parse(Regex.Match(resultString, @"\d+").Value))
                    {
                        filesCounter = Int32.Parse(Regex.Match(resultString, @"\d+").Value);
                    }
                }
                filesCounter++;
                path = m_exePath + "\\" + "log_file" + filesCounter + ".txt";
            }
            else
            {
                path = m_exePath + "\\" + "log_file1.txt";
              
            }
            txtWriter = File.AppendText(path);
            txtWriter.WriteLine("\rID: " + userID);
            txtWriter.WriteLine("DataBase: " + dbName);
            txtWriter.Close();
           
        }

        public void WritOperationInLogFile(string operationName, string query, string source, string target)
        {
            using (txtWriter = new StreamWriter(path, true))
            {
                try
                {
                    txtWriter.WriteLine("\r\nOperation: " + operationName);
                    txtWriter.WriteLine("Timestamp: {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                    txtWriter.WriteLine("Query: " + "\"" + query +  "\"");
                    if (source == null)
                    {
                        txtWriter.WriteLine("Source: null");
                    }
                    else
                    {
                        txtWriter.WriteLine("Source: " + "\"" + source + "\"");
                    }
                    if (target == null)
                    {
                        txtWriter.WriteLine("Target: null");
                    }
                    else
                    {
                        txtWriter.WriteLine("Target: " + "\"" + target + "\"");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
                finally
                {
                }
            }
        }

        public static void deleteLogFileIfNoOperationWasDone()
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    //Do something
                }
            }

              
        }
    }    
}
