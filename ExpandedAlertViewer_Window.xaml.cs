using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace WeatherAlert_DB
{
    /// <summary>
    /// Interaction logic for ExpandedAlertViewer_Window.xaml
    /// </summary>
    public partial class ExpandedAlertViewer_Window : Window
    {
        private static Alert AlertRef;
        public ExpandedAlertViewer_Window(Alert alert)
        {
            AlertRef = alert;
            InitializeComponent();
        }

        private void SetAllControlsDataContext()
        {
            // Set the data binding foreach textblock to the passed in Alert object.
            foreach (TextBlock textblock in ExpandedViewer_Grid.Children.OfType<TextBlock>())
            {
                textblock.DataContext = AlertRef;
            }
            // Set the data binding foreach textbox to the passed in Alert object.
            foreach (TextBox textbox in ExpandedViewer_Grid.Children.OfType<TextBox>())
            {
                textbox.DataContext = AlertRef;
            }
        }

        private void ExpandedViewer_Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetAllControlsDataContext();
        }
    }
}
