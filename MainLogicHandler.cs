using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class handles the primary applications logic.
    /// </summary>
    class MainLogicHandler
    {
        private static async void CallApiEvent(Object source, ElapsedEventArgs e)
        {
           InsertInfoToDB();
        }
        /// <summary>
        /// Start an auto resetting timer to request the API.
        /// </summary>
        public static void StartApiTimer()
        {
            int HourInMillsec = 3600000;
            int MinuteinMillisec = 60000;
            Timer ApiTimer = new Timer(10000);
            ApiTimer.AutoReset = false;
            ApiTimer.Elapsed += new ElapsedEventHandler(CallApiEvent);
            ApiTimer.Start();
        }
        private static void InsertInfoToDB()
        {
            var AlertInfoList = NWS_ApiController.ReturnApiCall();

            // DEBUG CODE 
            string OUTPUTTOLOG = "\n";
            foreach (var DEBUG in AlertInfoList)
            {

                OUTPUTTOLOG += DEBUG + "\n";

            }
            LogHandler Log = new LogHandler(OUTPUTTOLOG);
                Log.WriteLogFile();

            while (AlertInfoList.Count > 0 && AlertInfoList != null)
            {
                // Place to temporarily store values to construct Alert Objects
                string[] ValuesForObjectInstantiation = new string[11];

                // As each parameter is found add one to delete this number of indexes later.
                int LinesTriggered = 0;

                // Check a bool to see if this executed for an object construction
                // This is used to make sure the index gets calculated correctly
                bool WasThereA_NwsHeadline = false;

                // Have to check line by line incase some parameters wasn't sent
                for (int CurrentIndex = 0; CurrentIndex < 9; CurrentIndex++)
                {
                    // Check if the index is at risk of becoming Out of Index and catch it here
                    // This only happens at the end of the list
                    if (AlertInfoList.Count == CurrentIndex) { break; }
                    
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
                        LinesTriggered++;
                    }
                    else if (AlertInfoList[CurrentIndex].StartsWith("NWSheadline:"))
                    {
                        WasThereA_NwsHeadline = true;
                        // Grab NwsHeadline & DescriptionKeywords
                        ValuesForObjectInstantiation[7] = Alert.ParseNWSHeadline(AlertInfoList[7]);
                        ValuesForObjectInstantiation[9] = Alert.ParseForDescriptionKeywords(AlertInfoList[7])
                                                            + " " + Alert.ParseForDescriptionKeywords(AlertInfoList[6]);
                        LinesTriggered++;
                    }

                    if (WasThereA_NwsHeadline && CurrentIndex == 8)
                    {
                        // Reset bool
                        WasThereA_NwsHeadline = false;

                        // Create a new Alert Object and store it in the DB. Insert all the info from the temp array into the object. 
                        Alert alert = new Alert(ValuesForObjectInstantiation[0], ValuesForObjectInstantiation[1],
                            ValuesForObjectInstantiation[2], ValuesForObjectInstantiation[3], ValuesForObjectInstantiation[4],
                            ValuesForObjectInstantiation[5], ValuesForObjectInstantiation[6], ValuesForObjectInstantiation[7],
                            ValuesForObjectInstantiation[8], ValuesForObjectInstantiation[9], ValuesForObjectInstantiation[10]);
                        SQLite_Data_Access.InsertIn_DB(alert);

                        //Remove all elements that were used in the AlertInfoList for the creation of this object
                        AlertInfoList.RemoveRange(0, LinesTriggered - 1);
                    }
                    // Check for certain properties that may not have been sent
                    else if (WasThereA_NwsHeadline == false && CurrentIndex == 8)
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
                            ValuesForObjectInstantiation[8], ValuesForObjectInstantiation[9], ValuesForObjectInstantiation[10]);
                        SQLite_Data_Access.InsertIn_DB(alert);

                        //Remove all elements that were used in the AlertInfoList for the creation of this object
                        AlertInfoList.RemoveRange(0, LinesTriggered - 1);
                    }

                }
            }
        }
    }
}
