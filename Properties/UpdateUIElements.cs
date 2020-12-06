using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WeatherAlert_DB
{
    
    /// <summary>
    /// This class updates the MainWindow with information from the DB.
    /// </summary>
    public class UpdateUIElements
    {
        // EVENT VIEWER SECTION
        /// <summary>
        /// This updates all the controls in the MainWindow for the EventViewer section.
        /// </summary>
        public static void PopulateAllEventViewControls(
            ListView listView, TextBox eventIDTextBox, DatePicker datePickerStart,
            DatePicker datePickerEnd, ComboBox eventTypeComboBox, ComboBox stateComboBox,
            ListBox keywordsListBox, SQLite_Data_Access.ConnectionString connectionString)
        {
            // Make sure valid chars are entered in the EventID TextBox.
            // If they are valid, then update and display the controls
            // to the user.
            if (EnsureValidCharsInEventIDTextBox(eventIDTextBox))
            {
                RefreshAndFilterEventList(listView, eventIDTextBox, datePickerStart,
                                      datePickerEnd, eventTypeComboBox, stateComboBox,
                                      keywordsListBox, connectionString);
                UpdateUIEventType(listView, eventTypeComboBox);
                UpdateUIStates(stateComboBox);
                UpdateUIKeywords(keywordsListBox);
            }
        }
        // FILTER BY SECTION
        private static void UpdateUIKeywords(ListBox listBox)
        {
            if (listBox.Items.Count == 0)
            {
                foreach (var keyword in Alert.DescriptorWords)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = keyword;
                    listBox.Items.Add(checkBox);
                }
            }    
        }
        private static void UpdateUIStates(ComboBox statesComboBox)
        {
            foreach (var State in Alert.StateDictionary.Values)
            {
                statesComboBox.Items.Add(State);
            }
            // Add a blank entry for user to deselect the filter
            if (statesComboBox.Items[0].ToString() != "")
            {
                statesComboBox.Items.Insert(0, "");
            }    
        }
        private static void UpdateUIEventType(ListView listView, ComboBox eventTypeComboBox)
        {
            // Grab the objects from the current ListView and populate the Event Type combobox.
            foreach (var item in listView.Items)
            {
                Alert alert = ((Alert)item);
                if (!eventTypeComboBox.Items.Contains(alert.EventType))
                {
                    eventTypeComboBox.Items.Add(alert.EventType);
                }
            }
            // Add a blank entry for user to deselect the filter
            if (eventTypeComboBox.Items[0].ToString() != "")
            {
                eventTypeComboBox.Items.Insert(0, "");
            }
        }
        private static bool EnsureValidCharsInEventIDTextBox(TextBox eventIDTextBox)
        {
            // Ensure that only valid chars are entered. 
            // If they are not display dialog to user.
            bool ContainsValidChars = true;
            char[] ValidChars = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
            foreach (char c in eventIDTextBox.Text)
            {
                if (!ValidChars.Contains(c))
                {
                    eventIDTextBox.Text = eventIDTextBox.Text.Remove(eventIDTextBox.Text.Length - 1);

                    InformUserDialog informUserDialog = 
                        new InformUserDialog("Please enter the correct characters.", 
                                             "OK", "Valid characters: (0,1,2,3,4,5,6,7,8,9,-)");
                    informUserDialog.Owner = Window.GetWindow(eventIDTextBox);
                    informUserDialog.ShowDialog();
                    ContainsValidChars = false;
                }
            }
            // Put the caret back at the end of the textbox for the user
            // and return bool value.
            eventIDTextBox.CaretIndex = eventIDTextBox.Text.Length;
            return ContainsValidChars;
        }
        private static string KeywordStringFromCheckBoxs(ListBox listBox)
        {
            string KeywordString = "";
            foreach (var item in listBox.Items)
            {
                if (item is CheckBox box && (bool)box.IsChecked)
                {
                    KeywordString += box.Content.ToString() + " ";
                }
            }
            return KeywordString;
        }
        private static void RefreshAndFilterEventList(
            ListView listView, TextBox eventIDTextBox, DatePicker datePickerStart,
            DatePicker datePickerEnd, ComboBox eventTypeComboBox, ComboBox stateComboBox, 
            ListBox keywordsListBox, SQLite_Data_Access.ConnectionString connectionString)
        {
            // Make a new list with all the current objects in the DB to reference.
            List<Alert> AlertList = SQLite_Data_Access.SelectAll_DB(connectionString);

            // Make a reversed StateDictionary to compare the values to the keys
            Dictionary<String, String> ReversedStateDictionary = new Dictionary<string, string>();
            foreach (var state in Alert.StateDictionary)
            {
                ReversedStateDictionary.Add(state.Value,state.Key);
            }
            
            // Build LINQ query based on current values
            if (!string.IsNullOrEmpty(eventIDTextBox.Text))
            {
                AlertList = AlertList.Where(alert => alert.Id.Contains(eventIDTextBox.Text)).ToList();
            }
            if (datePickerStart.SelectedDate != null && datePickerEnd.SelectedDate != null)
            {
                AlertList = AlertList.Where(alert => DateTime.Parse(alert.Date) >= datePickerStart.SelectedDate && DateTime.Parse(alert.Date) <= datePickerEnd.SelectedDate).ToList();
            }
            if (!string.IsNullOrEmpty(eventTypeComboBox.Text))
            {
                AlertList = AlertList.Where(alert => alert.EventType == eventTypeComboBox.Text).ToList();
            }
            if (!string.IsNullOrEmpty(stateComboBox.Text))
            {
                AlertList = AlertList.Where(alert => alert.State == ReversedStateDictionary[stateComboBox.Text]).ToList();
            }
            if (!string.IsNullOrEmpty(KeywordStringFromCheckBoxs(keywordsListBox)))
            {
                AlertList = AlertList.Where(alert => alert.DescriptionKeywords.Contains(KeywordStringFromCheckBoxs(keywordsListBox))).ToList();
            }

            // Finally add all the Alert objects that were filtered to the ListView
            // And clear old records
            listView.Items.Clear();
            foreach (var Alert in AlertList)
            {
                listView.Items.Add(Alert);
            }
        }
        // GRAPH VIEW SECTION
    }
}
