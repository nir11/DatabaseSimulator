using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSimulator
{
    class Join
    {
        private MainForm mainForm;

        // Constructor
        public Join(MainForm form1)
        {
            // store the Form1 object reference
            mainForm = form1;
        }
      
        // A function that invoked after the user clicked the Run button in Join panel
        public bool checkIfAllFieldsAreFullInJoinPanel()
        {
            if (mainForm.sourceTableComboBox_JoinPanel.SelectedItem == null || mainForm.columnsForSourceTableComboBox_JoinPanel.SelectedItem == null
                || mainForm.targetTableComboBox_JoinPanel.SelectedItem == null || mainForm.columnsForTargetTableComboBox_JoinPanel.SelectedItem == null
                || mainForm.sourceTableComboBox_JoinPanel.Text == "בחר עמודה" || mainForm.columnsForSourceTableComboBox_JoinPanel.Text == "בחר עמודה"
                || mainForm.targetTableComboBox_JoinPanel.Text == "בחר עמודה" || mainForm.columnsForTargetTableComboBox_JoinPanel.Text == "בחר עמודה")
            {
                return false;
            }
            return true;           
        }
        

    }
}
