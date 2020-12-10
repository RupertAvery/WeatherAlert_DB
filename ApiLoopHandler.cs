using System;
using System.Timers;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class handles the GET Request Logic on a timer event.
    /// </summary>
    static class ApiLoopHandler
    { 
        // Declare a single repeating timer to request the NWS Api after the elapsed time
        private static Timer ApiTimer = new Timer(900000);
        public static TimeSpan ApiTimeSpan = new TimeSpan(0, 0, 0, 0, (int)ApiTimer.Interval);

        private static async void CallApiEvent(object source, ElapsedEventArgs e)
        {
            // Check if user is using DummyDb instead. 
            // If so prevent API Calls here.
            if (!SQLite_Data_Access.IsUsingDummyDB)
            {
                while (SyncInfoToDB())
                {
                    ApiTimeSpan = new TimeSpan(0,0,0,0,-1);
                }
                // Reset API Timer
                ApiTimeSpan = new TimeSpan(0, 0, 0, 0, (int)ApiTimer.Interval);
            }
            else
            {
                LogHandler Log = new LogHandler("Skipped API request: User is using DummyDB instead.");
                Log.WriteLogFile();
            }
        }
        /// <summary>
        /// Start an auto resetting Timer to request the API. 
        /// This is meant to continuously request the API while the app is running.
        /// </summary>
        public static void StartApiTimerLoop()
        {
            if (!ApiTimer.Enabled)
            {
                // Request API every 15 minutes.
                ApiTimer.AutoReset = true;
                ApiTimer.Elapsed += new ElapsedEventHandler(CallApiEvent);
                ApiTimer.Start();
            }
        }
        /// <summary>
        /// A single use Timer to call a request to the API.
        /// </summary>
        /// <param name="delayInMilliSec"></param>
        public static void SingleApiTimer(int delayInMilliSec)
        {
            // Request API on application start after short delay
            Timer ApiTimerStartup = new Timer(delayInMilliSec);
            ApiTimerStartup.AutoReset = false;
            ApiTimerStartup.Elapsed += new ElapsedEventHandler(CallApiEvent);
            ApiTimerStartup.Start();
        }
        private static bool SyncInfoToDB()
        {
            // Declare bool to check if the data is still being entered or it is done
            bool IsSyncing = true;

            // Call log to write to later.
            LogHandler AlertLog = new LogHandler("Succesfully synced records.\nDuplicates skipped:");
            
            // Call and Read from GET request.
            var AlertInfoList = NWS_ApiController.ReturnApiCall();

            while (AlertInfoList.Count > 0 && AlertInfoList != null)
            {
                // Place to temporarily store values to construct Alert Objects
                string[] ValuesForObjectInstantiation = new string[11];

                // As each parameter is found add one to delete this number of indexes later.
                int LinesTriggered = 0;

                // Check if a Headline was found since it may not always be sent
                // These are used to make sure the index gets calculated correctly
                bool WasThereA_NwsHeadline = false;
                bool HasIdAlreadyBeenFound = false;

                // Have to check line by line incase some parameters wasn't sent
                for (int CurrentIndex = 0; CurrentIndex < 8; ++CurrentIndex)
                {

                    // Iterate through all entries and scan for certain keywords
                    if (AlertInfoList[CurrentIndex].StartsWith("@id:") && !HasIdAlreadyBeenFound)
                    {
                        // Grab ID
                        ValuesForObjectInstantiation[0] = Alert.ParseID(AlertInfoList[0]);
                        HasIdAlreadyBeenFound = true;
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("areaDesc:"))
                    {
                        // Grab Area Description
                        ValuesForObjectInstantiation[10] = Alert.ParseAreaDescription(AlertInfoList[1]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("sent:"))
                    {
                        // Grab Date & Time
                        ValuesForObjectInstantiation[1] = Alert.ParseDate(AlertInfoList[2]);
                        ValuesForObjectInstantiation[2] = Alert.ParseTime(AlertInfoList[2]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("severity:"))
                    {
                        // Grab Severity
                        ValuesForObjectInstantiation[6] = Alert.ParseSeverity(AlertInfoList[3]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("event:"))
                    {
                        // Grab Event
                        ValuesForObjectInstantiation[3] = Alert.ParseEvent(AlertInfoList[4]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("senderName:"))
                    {
                        // Grab State & City
                        ValuesForObjectInstantiation[4] = Alert.ParseState(AlertInfoList[5]);
                        ValuesForObjectInstantiation[5] = Alert.ParseCity(AlertInfoList[5]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("description:"))
                    {
                        ValuesForObjectInstantiation[8] = Alert.ParseDescription(AlertInfoList[6]);
                        ValuesForObjectInstantiation[9] += Alert.ParseForDescriptiveKeywords(AlertInfoList[6]);
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("NWSheadline:"))
                    {
                        WasThereA_NwsHeadline = true;
                        // Grab NwsHeadline & DescriptionKeywords
                        ValuesForObjectInstantiation[7] = Alert.ParseNWSHeadline(AlertInfoList[7]);
                        ValuesForObjectInstantiation[9] += Alert.ParseForDescriptiveKeywords(AlertInfoList[7]);                           
                        LinesTriggered++;
                    }
                }

                if (WasThereA_NwsHeadline && LinesTriggered == 8)
                {
                    // Create a new Alert Object and store it in the DB. Insert all the info from the temp array into the object. 
                    Alert alert = new Alert(ValuesForObjectInstantiation[0], ValuesForObjectInstantiation[1],
                        ValuesForObjectInstantiation[2], ValuesForObjectInstantiation[3], ValuesForObjectInstantiation[4],
                        ValuesForObjectInstantiation[5], ValuesForObjectInstantiation[6], ValuesForObjectInstantiation[7],
                        ValuesForObjectInstantiation[8], Alert.CleanDescriptiveKeywords(ValuesForObjectInstantiation[9]), ValuesForObjectInstantiation[10]);

                    // Construct the objects and for each skipped object out it to log
                    if (!SQLite_Data_Access.InsertIn_DB(alert))
                    {
                        AlertLog.LogMessage += $" ,{ValuesForObjectInstantiation[0]}";
                        AlertLog.NumOfObjects++;
                    }

                    //Remove and reset all elements that were used in the AlertInfoList for the creation of this object
                    AlertInfoList.RemoveRange(0, LinesTriggered);
                }

                // Check for certain properties that may not have been sent
                else if (WasThereA_NwsHeadline == false && LinesTriggered == 7)
                {
                    // Prevent NULL DB entry and specifically set the entries to 
                    if (ValuesForObjectInstantiation[7] == null)
                    {
                        ValuesForObjectInstantiation[7] = "NOT SPECIFIED";
                        ValuesForObjectInstantiation[9] = "UNKNOWN";
                    }

                    // Create a new Alert Object and store it in the DB. Insert all the info from the temp array into the object. 
                    Alert alert = new Alert(ValuesForObjectInstantiation[0], ValuesForObjectInstantiation[1],
                        ValuesForObjectInstantiation[2], ValuesForObjectInstantiation[3], ValuesForObjectInstantiation[4],
                        ValuesForObjectInstantiation[5], ValuesForObjectInstantiation[6], ValuesForObjectInstantiation[7],
                        ValuesForObjectInstantiation[8], Alert.CleanDescriptiveKeywords(ValuesForObjectInstantiation[9]), ValuesForObjectInstantiation[10]);

                    // Construct the objects and for each skipped object out it to log
                    if (!SQLite_Data_Access.InsertIn_DB(alert))
                    {
                        AlertLog.LogMessage += $" {ValuesForObjectInstantiation[0]},";
                        AlertLog.NumOfObjects++;
                    }

                    //Remove and reset all elements that were used in the AlertInfoList for the creation of this object
                    AlertInfoList.RemoveRange(0, LinesTriggered);
                }
            }
            // Output AlertLog
            AlertLog.LogMessage += "\nTotal Skipped Alerts: " + AlertLog.NumOfObjects;
            AlertLog.WriteLogFile();

            // return the bool value
            IsSyncing = false;
            return IsSyncing;
        }
    }
}
