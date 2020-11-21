using System.Windows;
using System;
using System.Collections.Generic;

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

        }

        private void DatabaseOptions_Button_Click(object sender, RoutedEventArgs e)
        {
            //Show user the DB Options Window
            DatabaseOptions databaseOptions = new DatabaseOptions();
            databaseOptions.Owner = this;
            databaseOptions.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If user attempts to close this MainWindow then close all other currently open windows and exit application.
            foreach (Window windows in this.OwnedWindows)
            {
                windows.Close();
            }
        }
    }
}
