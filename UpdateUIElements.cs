using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using WeatherAlert_DB.Database;

namespace WeatherAlert_DB
{

    /// <summary>
    /// This class updates the MainWindow with information from the DB.
    /// </summary>
    public class UpdateUIElements
    {
        private static bool IsDispatcherTimerActive = false;
        // -------------------------------------------
        // - EVENT VIEWER SECTION                    -
        // -------------------------------------------

        private static ISQLiteDataAccess sqLiteDataAccess => DatabaseFactory.GetDatabase();

        /// <summary>
        /// This updates all the controls in the MainWindow for the EventViewer section.
        /// </summary>
        public static void PopulateAllEventViewControls(
            ListView listView, TextBox eventIDTextBox, DatePicker datePickerStart,
            DatePicker datePickerEnd, ComboBox eventTypeComboBox, ComboBox stateComboBox,
            ComboBox severityComboBox, ListBox keywordsListBox, StatusBar statusBar)
        {
            // Make sure valid chars are entered in the EventID TextBox.
            // If they are valid, then update and display the controls
            // to the user.
            if (EnsureValidCharsInEventIDTextBox(eventIDTextBox))
            {
                RefreshAndFilterEventList(listView, eventIDTextBox, datePickerStart,
                                          datePickerEnd, eventTypeComboBox, stateComboBox,
                                          severityComboBox ,keywordsListBox);
                UpdateUIEventType(listView, eventTypeComboBox);
                UpdateUIStates(stateComboBox);
                UpdateUIKeywords(keywordsListBox);
                UpdateUISeverity(listView, severityComboBox);
                UpdateUIStatusBar(statusBar, listView);
                ApiLoopHandler.StartApiTimerLoop();
            }
        }

        /// <summary>
        /// Tell the Main Window to refresh the event viewer.
        /// </summary>
        public static void ForceEventViewerRefresh()
        {
            var window = (MainWindow)Application.Current.MainWindow;
            window.UpdateEventViewUI();
        }

        // -------------------------------------------
        // - EVENTVIEWER FILTER BY SECTION           -
        // -------------------------------------------

        private static void UpdateUIKeywords(ListBox listBox)
        {
            // This section adds checkboxes to the empty listbox in the FilterBy Keywords section
            if (listBox.Items.Count == 0)
            {
                foreach (var keyword in AlertHelper.DescriptorWords)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = keyword;
                    listBox.Items.Add(checkBox);
                }
            }
        }

        private static void UpdateUIStates(ComboBox statesComboBox)
        {
            // Populate the combobox with all the states names
            foreach (var State in AlertHelper.StateDictionary.Values)
            {
                statesComboBox.Items.Add(State);
            }
            AddBlankItemToComboBox(statesComboBox);
        }

        private static void AddBlankItemToComboBox(ComboBox comboBox)
        {
            // Adds a blank entry for user to deselect the filter at the top of the combobox
            if (comboBox.Items.Count > 0)
            {
                if (comboBox.Items[0].ToString() != "")
                {
                    comboBox.Items.Insert(0, "");
                }
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
            AddBlankItemToComboBox(eventTypeComboBox);
        }

        private static bool EnsureValidCharsInEventIDTextBox(TextBox eventIDTextBox)
        {
            // Ensure that only valid chars are entered. 
            // If they are not display dialog to user.
            bool ContainsValidChars = true;
            char[] ValidChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
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
        private static List<string> KeywordsFromCheckBoxs(ListBox listBox)
        {
            List<string> KeywordCheckBoxStrings = new List<string>();
            foreach (var item in listBox.Items)
            {
                if (item is CheckBox box && (bool)box.IsChecked)
                {
                    KeywordCheckBoxStrings.Add(box.Content.ToString());
                }
            }
            return KeywordCheckBoxStrings;
        }

        private static void UpdateUISeverity(ListView listView, ComboBox eventTypeComboBox)
        {
            // Grab the objects from the current ListView and populate the Severity Type combobox.
            foreach (var item in listView.Items)
            {
                Alert alert = ((Alert)item);
                if (!eventTypeComboBox.Items.Contains(alert.Severity))
                {
                    eventTypeComboBox.Items.Add(alert.Severity);
                }
            }
            AddBlankItemToComboBox(eventTypeComboBox);
        }

        private static void RefreshAndFilterEventList(
            ListView listView, TextBox eventIDTextBox, DatePicker datePickerStart,
            DatePicker datePickerEnd, ComboBox eventTypeComboBox, ComboBox stateComboBox,
            ComboBox severityComboBox, ListBox keywordsListBox)
        {
            // Make a new list with all the current objects in the DB to reference.

            // Make a reversed StateDictionary to compare the values to the keys
            Dictionary<String, String> ReversedStateDictionary = new Dictionary<string, string>();
            foreach (var state in AlertHelper.StateDictionary)
            {
                ReversedStateDictionary.Add(state.Value, state.Key);
            }

            var alertFilter = new AlertFilter
            {
                EventID = eventIDTextBox.Text,
                StartDate = datePickerStart.SelectedDate,
                EndDate = datePickerEnd.SelectedDate,
                EventType = eventTypeComboBox.Text,
                State = string.IsNullOrEmpty(stateComboBox.Text) ? null : ReversedStateDictionary[stateComboBox.Text],
                Severity = severityComboBox.Text,
                Keywords = KeywordsFromCheckBoxs(keywordsListBox)
            };

            IEnumerable<Alert> AlertList = sqLiteDataAccess.Select(alertFilter);


            // Finally add all the Alert objects that were filtered to the ListView
            // And clear old records
            listView.Items.Clear();
            foreach (var Alert in AlertList)
            {
                listView.Items.Add(Alert);
            }
        }

        private static void UpdateUIStatusBar(StatusBar statusBar, ListView listView)
        {
            // Update all the items in the Status Bar

            // Grab all items in the statusbar
            List<StatusBarItem> ControlsInStatusBar = new List<StatusBarItem>();
            foreach (var item in statusBar.Items)
            {
                ControlsInStatusBar.Add((StatusBarItem)item);
            }

            // Handle the first StatusBar Item
            int NumberOfAllRecords = (int)sqLiteDataAccess.Count();
            int NumberOfShownRecords = listView.Items.Count;
            ControlsInStatusBar[0].Content = $"Records Shown: {NumberOfShownRecords}/{NumberOfAllRecords}";

            // Handle the second StatusBar Item
            StartDispatcherTimer(ControlsInStatusBar[1]);
        }
        private static void StartDispatcherTimer(StatusBarItem statusBarItem)
        {
            // Starts a timer to update the Sync Timer in the StatusBar
            if (!IsDispatcherTimerActive)
            {
                IsDispatcherTimerActive = true;
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(delegate (object sender, EventArgs e) { DispatcherTimer_Tick(sender, e, statusBarItem); });
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
            }
        }
        private static void DispatcherTimer_Tick(object sender, EventArgs e, StatusBarItem statusBarItem)
        {
            UpdateStatusBarSyncTimer(statusBarItem);
            ApiLoopHandler.ApiTimeSpan = ApiLoopHandler.ApiTimeSpan.Subtract(new TimeSpan(0, 0, 1));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        private static void UpdateStatusBarSyncTimer(StatusBarItem statusBarItem)
        {
            // Check if the user is using the DummyDB before displaying StatusBar Sync info to user
            if (!DatabaseFactory.IsUsingDummyDB)
            {
                // Checks the ApiTimeSpan status and updates the Statusbar item with info on the Sync Status
                // The timespan gets set to -1 if the DB is syncing
                if (ApiLoopHandler.ApiTimeSpan.TotalMilliseconds < 0)
                {
                    statusBarItem.Content = "Sync Status: Syncing";
                }
                else
                {
                    statusBarItem.Content = FormatSyncStatus();
                }
            }
            else
            {
                statusBarItem.Content = "Sync Status: Disabled";
            }
        }
        private static string FormatSyncStatus()
        {
            // Returns the minutes left unless the minutes are under 1. Then it returns seconds instead.
            if (ApiLoopHandler.ApiTimeSpan.TotalMinutes < 1)
            {
                return $"Sync Status: {ApiLoopHandler.ApiTimeSpan.Seconds}s";
            }
            else
            {
                return $"Sync Status: {ApiLoopHandler.ApiTimeSpan.Minutes}m";
            }
        }

        // -------------------------------------------
        // - GRAPH VIEW SECTION                      -
        // -------------------------------------------
    }
}
