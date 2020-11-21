using System;
using System.Collections.Generic;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class includes methods to convert the raw json info from the API to construct Alert Objects.
    /// </summary>
    class Alert
    {
        public string Id;
        public string Date;
        public string Time;
        public string EventType;
        public string State;
        public string City;
        public string Severity;
        public string NWSHeadline;
        public string DescriptionKeywords;
        public string AreaDescription;
        public Alert(string id, string date, string time, string eventType, string state, string city, string serverity, string nwsHeadline, string descriptionKeywords, string areaDescription)
        {
            Id = id;
            Date = date;
            Time = time;
            EventType = eventType;
            State = state;
            City = city;
            Severity = serverity;
            NWSHeadline = nwsHeadline;
            DescriptionKeywords = descriptionKeywords;
            AreaDescription = areaDescription;
        }
        /// <summary>
        /// Converts the raw ID from the API into the correct format for the DB. Json tag: "@id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Truncated ID as a string.</returns>
        public static string ParseID(string id)
        {
            int numOfCharsToRemove = id.LastIndexOf("PROD-");
            return id.Remove(0, numOfCharsToRemove + 5).Trim();
        }
        /// <summary>
        /// Converts the raw Date from the API into the correct format for the DB. Json tag: "sent"
        /// </summary>
        /// <param name="sent"></param>
        /// <returns>Truncated Date as a string.</returns>
        public static string ParseDate(string sent)
        {
            sent = sent.Remove(0, 6);
            int numOfCharsToRemove = sent.LastIndexOf('T');
            return sent.Remove(numOfCharsToRemove).Trim();
        }
        /// <summary>
        /// Converts the raw Time from the API into the correct format for the DB. Json tag: "sent"
        /// </summary>
        /// <param name="sent"></param>
        /// <returns>Truncated Time as a string.</returns>
        public static string ParseTime(string sent)
        {
            int numOfCharsToRemove = sent.LastIndexOf('T');
            return sent.Remove(0,numOfCharsToRemove + 1).Trim();
        }
        /// <summary>
        /// Converts the raw State from the API into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="senderName"></param>
        /// <returns>State abbreviation as a string.</returns>
        public static string ParseState(string senderName, Dictionary<string, string> stateDictionary)
        {
            string ParsedString = senderName.Remove(0, 17);
            int NumOfCharsToDelete = ParsedString.LastIndexOf(' ');
            ParsedString = ParsedString.Remove(0, NumOfCharsToDelete + 1);

            // ParsedString now contains either the full states name OR the states abbreviation. 
            // Now iterate through the Dictionary to match values and make sure to output the states abbreviation.
            ParsedString = ParsedString.ToUpper();
            foreach (var keyValuePair in stateDictionary)
            {
                // Check if State is an abreviation 
                if (ParsedString == keyValuePair.Key.ToUpper())
                {
                    break;
                }
                // Check if State is the full name
                else if (ParsedString == keyValuePair.Value.ToUpper())
                {
                    ParsedString = keyValuePair.Key.ToUpper();
                }
            }
            return ParsedString.Trim();
        }
        /// <summary>
        /// Converts the raw state into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="senderName"></param>
        /// <returns>Truncated City as a string.</returns>
        public static string ParseCity(string senderName)
        {
            senderName = senderName.Remove(0,17);
            int NumOfCharsUntilState = senderName.LastIndexOf(' ');
            senderName = senderName.Substring(0, NumOfCharsUntilState);
            return senderName.ToUpper().Trim();
        }
        /// <summary>
        /// Iterates through a string to check for key words. Json tag: "NWSHeadline"
        /// </summary>
        /// <param name="description"></param>
        /// <returns>A string with descriptor words.</returns>
        public static string ParseDescriptionKeywords(string NWS_Headline)
        {
            string[] DescriptorWords = { "FOG", "GALE", "SNOW", "RAIN", "ICE", "STORM",
                                         "EARTHQUAKE", "TORNADO", "FLOOD", "HURRICANE", "CYCLONE", 
                                         "BLIZZARD", "HAIL", "WIND", "DUST", "FIRE", "WILDFIRE" };
            string[] SeperatedWords = NWS_Headline.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string CombinedDescriptorWords = "";
            foreach (var word in SeperatedWords)
            {
                foreach (var descriptorWord in DescriptorWords)
                {
                    if (descriptorWord == word && !CombinedDescriptorWords.Contains(descriptorWord))
                    {
                        CombinedDescriptorWords += descriptorWord + " ";
                    }
                }
            }
            // Check if the string was assigned any keywords. 
            // If not then specifically say its UNKNOWN to prevent a null database entry.
            if (string.IsNullOrEmpty(CombinedDescriptorWords))
            {
                CombinedDescriptorWords = "UNKNOWN";
            }
            return CombinedDescriptorWords;
        }
        /// <summary>
        /// Converts the raw Severity into the correct format for the DB. Json tag: "severity"
        /// </summary>
        /// <param name="severity"></param>
        /// <returns>Truncated Severity as a string.</returns>
        public static string ParseSeverity(string severity)
        {
            return severity.Remove(0,10);
        }
        /// <summary>
        /// Converts the raw EventType into the correct format for the DB. Json tag: "event"
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public static string ParseEvent(string eventType)
        {
            return eventType.Remove(0, 7);
        }
    }
}
