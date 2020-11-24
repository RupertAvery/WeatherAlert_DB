using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class handles the primary applications logic.
    /// </summary>
    class MainLogicHandler
    {
        // Check if app just started if so force a new Sync request
        private static async void CallApiEvent(Object source, ElapsedEventArgs e)
        {
            // Check if user is using DummyDb instead. 
            // If so prevent API Calls here.
            if (!SQLite_Data_Access.IsUsingDummyDB)
            {
                SyncInfoToDB();
            }
            else
            {
                LogHandler Log = new LogHandler("Skipped API request: User is using DummyDB instead.");
                Log.WriteLogFile();
            }
        }
        /// <summary>
        /// Start an auto resetting timer to request the API.
        /// </summary>
        public static void StartApiTimer()
        {
            // Request API every 15 minutes.
            Timer ApiTimer = new Timer(900000);
            ApiTimer.AutoReset = true;
            ApiTimer.Elapsed += new ElapsedEventHandler(CallApiEvent);
            ApiTimer.Start();

            // Request API on application start after short delay
            Timer ApiTimerStartup = new Timer(60000);
            ApiTimerStartup.AutoReset = false;
            ApiTimerStartup.Elapsed += new ElapsedEventHandler(CallApiEvent);
            ApiTimerStartup.Start();
        }
        private static void SyncInfoToDB()
        {
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
                // This is used to make sure the index gets calculated correctly
                bool WasThereA_NwsHeadline = false;

                // Have to check line by line incase some parameters wasn't sent
                for (int CurrentIndex = 0; CurrentIndex < 8; ++CurrentIndex)
                {

                    // Iterate through all entries and scan for certain keywords
                    if (AlertInfoList[CurrentIndex].StartsWith("@id:"))
                    {
                        // Grab ID
                        ValuesForObjectInstantiation[0] = Alert.ParseID(AlertInfoList[0]);
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
                    // Reset bool
                    WasThereA_NwsHeadline = false;

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
                    // Prevent NULL DB entry and specifically set the entries to null
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
        }
    }
}
