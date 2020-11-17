using System;
using System.Collections.Generic;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class constructs an Alert Object and includes sever. 
    /// </summary>
    class Alert
    {
        string Id;
        string Date;
        string EventType;
        string State;
        string City;
        string Severity;
        string NWSHeadline; 
        string DescriptionKeywords;

        public Alert(string id, string date, string eventType, string state, string city, string serverity, string nwsHeadline, string descriptionKeywords)
        {
            Id = id;
            Date = date;
            EventType = eventType;
            State = state;
            City = city;
            Severity = serverity;
            NWSHeadline = nwsHeadline;
            DescriptionKeywords = descriptionKeywords;
        }
        /// <summary>
        /// Converts the raw ID from the API into the correct format for the DB. Json tag: "@id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Truncated ID as a string.</returns>
        public static string ParseID(string id)
        {
            return id.Replace("https://api.weather.gov/alerts/NWS-IDP-PROD-", "");
        }
        /// <summary>
        /// Converts the raw Date from the API into the correct format for the DB. Json tag: "sent"
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Truncated Date as a string.</returns>
        public static string ParseDate(string date)
        {
            return date.Substring(0,10);
        }
        /// <summary>
        /// Converts the raw State from the API into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="state"></param>
        /// <returns>State abbreviation as a string.</returns>
        public static string ParseState(string state, Dictionary<string, string> stateDictionary)
        {
            state.Remove(0, 4);
            string ParsedString = "";
            bool LoopedOnce = false;
            foreach (char C in state)
            {
                if (C == ' ')
                {
                    LoopedOnce = true;
                }
                if (LoopedOnce)
                {
                    ParsedString += C;
                }
            }
            // ParsedString now contains either the full state name OR the states abbreviation. 
            // Now iterate through the Dictionary to match values and make sure to output the states abbreviation.
            ParsedString = ParsedString.ToUpper();
            foreach (var keyValuePair in stateDictionary)
            {
                // Check if State is an abreviation 
                if (ParsedString == keyValuePair.Key)
                {
                    break;
                }
                // Check if State is the Name
                else if (ParsedString == keyValuePair.Value)
                {
                    ParsedString = keyValuePair.Key.ToUpper();
                }
            }

            return ParsedString;
        }
        /// <summary>
        /// Converts the raw state into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="city"></param>
        /// <returns>Truncated City as a string.</returns>
        public static string ParseCity(string city)
        {
            city.Remove(0,4);
            string ParsedString = "";
            foreach (char C in city)
            {
                ParsedString += C;
                if (C == ' ')
                {
                    break;
                }
            }
            return ParsedString.ToUpper();
        }
        /// <summary>
        /// Iterates through a string to check for key words. Json tag: "NWSHeadline"
        /// </summary>
        /// <param name="description"></param>
        /// <returns>A string with descriptor words.</returns>
        public static string ParseDescriptionKeywords(string description)
        {
            string[] DescriptorWords = { "FOG", "GALE", "SNOW", "RAIN", "ICE", "STORM", "EARTHQUAKE", "TORNADO", "FLOOD", "HURRICANE", "CYCLONE", "BLIZZARD", "HAIL", "WIND", "DUST", "FIRE", "WILDFIRE" };
            string[] SeperatedWords = description.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
            // Check if the string was assigned any keywords. If not then specifically say its UNKNOWN to prevent a null database entry.
            if (string.IsNullOrEmpty(CombinedDescriptorWords))
            {
                CombinedDescriptorWords = "UNKNOWN";
            }
            return CombinedDescriptorWords;
        }
    }
}
