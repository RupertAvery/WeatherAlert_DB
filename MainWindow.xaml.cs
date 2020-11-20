using System.Windows;


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

            //Degub Code
            string[] StringsArry = { "@id", "sent", "event","senderName", "severity", "NWSheadline" };
            var List = NWS_ApiController.ParseReaderStringForKeywords(StringsArry, NWS_ApiController.RequestNWSApi("https://api.weather.gov/alerts/active?status=actual&message_type=alert&certainty=observed"));
            string tmp = "";
            foreach (var item in List)
            {
                tmp += item.ToString();
            }
            MessageBox.Show(tmp);
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
