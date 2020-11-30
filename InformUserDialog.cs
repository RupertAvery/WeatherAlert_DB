using System.Windows;

namespace WeatherAlert_DB
{
    /// <summary>
    /// Interaction logic for AreYouSureDialog.xaml
    /// </summary>
    public partial class InformUserDialog : Window
    {
        /// <summary>
        /// Open a new Dialog to ask user if they want to continue.
        /// The string informUser is displayed to user.
        /// The string actionButtonTxt is the confirm button content text.
        /// The string WarningTxt is optional and is displayed in italics underneath prompt text.
        /// </summary>
        public InformUserDialog(string informUserPrompt, string actionButtonTxt, string WarningTxt = "")
        {
            // Set window controls to passed in parameters
            InitializeComponent();
            PromptUser_Textblock.Text = informUserPrompt;
            Action_Button.Content = actionButtonTxt;
            WarningTxt_Textblock.Text = WarningTxt;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            // If user presses cancel close this window and return false.
            this.DialogResult = false;
            this.Close();
        }

        private void Action_Button_Click(object sender, RoutedEventArgs e)
        {
            // If user presses the confirm button close this window and return true.
            this.DialogResult = true;
            this.Close();
        }
    }
}
