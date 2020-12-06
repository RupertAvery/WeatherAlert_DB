using System.Windows;
using System;

namespace WeatherAlert_DB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //debug
            UpdateEventViewUI();
        }

        private void DatabaseOptions_Button_Click(object sender, RoutedEventArgs e)
        {
            //Show user the DB Options Window
            DatabaseOptions databaseOptions = new DatabaseOptions();
            databaseOptions.Owner = this;
            databaseOptions.ShowDialog();
        }
        /// <summary>
        /// Refresh and Display control data to the user for the entire EventViewer section.
        /// </summary>
        private void UpdateEventViewUI()
        {
            UpdateUIElements.PopulateAllEventViewControls(EventView_ListView, EV_EventID_TextBox, EV_DateStart_DatePicker,
                EV_DateEnd_DatePicker, EV_EventType_ComboBox, EV_State_ComboBox, EV_Keywords_ListBox, SQLite_Data_Access.ConnectionString.MainDB);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If user attempts to close this MainWindow then close all other currently open windows and exit application.
            foreach (Window windows in this.OwnedWindows)
            {
                windows.Close();
            }
        }

        private void EV_EventID_TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateEventViewUI();
        }

        private void EV_DateStart_DatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            UpdateEventViewUI();
        }

        private void EV_DateEnd_DatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            UpdateEventViewUI();
        }

        private void EV_EventType_ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UpdateEventViewUI();
        }

        private void EV_State_ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UpdateEventViewUI();
        }
    }
    
}
