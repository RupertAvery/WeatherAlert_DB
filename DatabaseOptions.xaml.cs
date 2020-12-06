using Microsoft.Win32;
using System.Media;
using System.Windows;
using System.IO;


namespace WeatherAlert_DB
{
    /// <summary>
    /// Interaction logic for DatabaseOptions.xaml
    /// </summary>
    public partial class DatabaseOptions : Window
    {
        public DatabaseOptions()
        {
            InitializeComponent();
            // If user is wanting to use the DummyDB Force check this box.
            if (SQLite_Data_Access.IsUsingDummyDB) { DummyDB_Checkbox.IsChecked = true; }
        }
        private void ImportDB_Button_Click(object sender, RoutedEventArgs e)
        {
            // Allow user to pick a DB file to import to the existing DB.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database Files (*.db)|*.db";
            if (openFileDialog.ShowDialog() == true)
            {
                throw new System.NotImplementedException("DB Import functionality not implemented yet.");

                // Log info
                var Log = new LogHandler("DB Import requested.");
                Log.WriteLogFile();
            }

        }
        private void ExportDB_Button_Click(object sender, RoutedEventArgs e)
        {
            // Allow user to pick a DB file to export to the existing DB.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Database Files (*.db)|*.db";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // If user attempts to Overwrite catch it here.
                    File.Copy("Alert_DB.db", saveFileDialog.FileName, false);

                    // Log info
                    var Log = new LogHandler("Exported DB.");
                    Log.WriteLogFile();
                }
                catch (IOException exception)
                {
                    InformUserDialog areYouSureDialog = 
                        new InformUserDialog("OPERATION DENIED. Exception logged. \n\n" +
                                              "Name cannot match or overwrite current DB.", "Ok", 
                                              "Please try a different name or destination.");
                    areYouSureDialog.Owner = this;
                    areYouSureDialog.ShowDialog();

                    // Log info
                    var Log = new LogHandler("Export DB Denied.", exception);
                    Log.WriteLogFile();
                } 
            }
        }
        private void EditDB_Button_Click(object sender, RoutedEventArgs e)
        {
            // Allow user to edit the DB after confirming prompt.
            InformUserDialog areYouSureDialog =
               new InformUserDialog("Are you sure you wish to edit the DB?", "Continue",
                                         "WARNING: Improper modifications to this database can cause " +
                                         "corrupted data or put the database in a nonrecoverable state. " +
                                         "Continue with caution.");
            areYouSureDialog.Owner = this;
            if ((bool)areYouSureDialog.ShowDialog())
            {
                // Log info
                var Log = new LogHandler("Edit DB called.");
                Log.WriteLogFile();

                SQLite_Data_Access.UpdateIn_DB("");
                this.Close();
            }
        }
        private void DeleteDB_Button_Click(object sender, RoutedEventArgs e)
        {
            // Give the user one last chance to change their mind before they reset the DB.
            SystemSounds.Exclamation.Play();
            InformUserDialog areYouSureDialog = 
                new InformUserDialog("Are you sure you wish to reset the DB?", "DELETE", 
                                          "WARNING: This action CLEARS ALL RECORDS in the database " +
                                          "and this cannot be undone. Are you absolutely sure?");
            areYouSureDialog.Owner = this;     
            if ((bool)areYouSureDialog.ShowDialog())
            {
                SQLite_Data_Access.DeleteAllIn_DB();

                // Log info
                var Log = new LogHandler("WIPED all DB entries.");
                Log.WriteLogFile();
                this.Close();
            }
        }
        private void DummyDB_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            SQLite_Data_Access.IsUsingDummyDB = true;

            // Log info
            var Log = new LogHandler("Switched to DummyDB.");
            Log.WriteLogFile();
        }
        private void DummyDB_Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            SQLite_Data_Access.IsUsingDummyDB = false;

            // Log info
            var Log = new LogHandler("Switched to MainDB.");
            Log.WriteLogFile();
        }
        private void SyncDB_Button_Click(object sender, RoutedEventArgs e)
        {
            ApiLoopHandler.SingleApiTimer(30000);
        }
    }
}
