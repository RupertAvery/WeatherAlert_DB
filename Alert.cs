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
        public string Description;
        public string DescriptionKeywords;
        public string AreaDescription;
        private static Dictionary<string, string> StateDictionary = new Dictionary<string,string>
        { { "AL", "ALABAMA" }, { "AK", "ALASKA" }, { "AZ", "ARIZONA" }, { "AR", "ARKANSAS" }, 
          { "CA", "CALIFORNIA" }, { "CO", "COLORADO" }, { "CT", "CONNECTICUT" }, { "DE", "DELAWARE" }, 
          { "FL", "FLORIDA" }, { "GA", "GEORGIA" }, { "HI", "HAWAII" }, { "ID", "IDAHO" },
          { "IL", "ILLINOIS" }, { "IN", "INDIANA" }, { "IA", "IOWA" }, { "KS", "KANSAS" },
          { "KY", "KENTUCKY" }, { "LA", "LOUISIANA" }, { "ME", "MAINE" }, { "MD", "MARYLAND" },
          { "MA", "MASSACHUSETTS" }, { "MI", "MICHIGAN" }, { "MN", "MINNESOTA" }, { "MS", "MISSISSIPPI" },
          { "MO", "MISSOURI" }, { "MT", "MONTANA" }, { "NE", "NEBRASKA" }, { "NV", "NEVADA" },
          { "NH", "NEW HAMPSHIRE" }, { "NJ", "NEW JERSEY" }, { "NM", "NEW MEXICO" }, { "NY", "NEW YORK" },
          { "NC", "NORTH CAROLINA" }, { "ND", "NORTH DAKOTA" }, { "OH", "OHIO" }, { "OK", "OKLAHOMA" },
          { "OR", "OREGON" }, { "PA", "PENNSYLVANIA" }, { "RI", "RHODE ISLAND" }, { "SC", "SOUTH CAROLINA" },
          { "SD", "SOUTH DAKOTA" }, { "TN", "TENNESSEE" }, { "TX", "TEXAS" }, { "UT", "UTAH" },
          { "VT", "VERMONT" }, { "VA", "VIRGINIA" }, { "WA", "WASHINGTON" }, { "WV", "WEST VIRGINIA" },
          { "WI", "WISCONSIN" }, { "WY", "WYOMING" }
        };
        private static string[] DescriptorWords = 
        {     "FOG", "GALE", "SNOW", "RAIN", "ICE", "STORM",
              "EARTHQUAKE", "TORNADO", "FLOOD", "HURRICANE", "CYCLONE",
              "BLIZZARD", "HAIL", "WIND", "DUST", "FIRE", "WILDFIRE",
              "SLUSH", "SLUSHY", "ADVISORY", "SLEET", "FREEZING", "CLOUDY",
              "WATER LEVEL", "WAVE", "SHOWER", "THUNDER", "LIGHTNING"
        };
        public Alert(string id, string date, string time, string eventType, string state, string city, string severity, string nwsHeadline, string description, string descriptionKeywords, string areaDescription)
        {
            Id = id;
            Date = date;
            Time = time;
            EventType = eventType;
            State = state;
            City = city;
            Severity = severity;
            NWSHeadline = nwsHeadline;
            Description = description;
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
            return id.Remove(0, numOfCharsToRemove + 5).Trim(',');
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
            return sent.Remove(numOfCharsToRemove).Trim(',');
        }
        /// <summary>
        /// Converts the raw Time from the API into the correct format for the DB. Json tag: "sent"
        /// </summary>
        /// <param name="sent"></param>
        /// <returns>Truncated Time as a string.</returns>
        public static string ParseTime(string sent)
        {
            int numOfCharsToRemove = sent.LastIndexOf('T');
            return sent.Remove(0,numOfCharsToRemove + 1).Trim(',');
        }
        /// <summary>
        /// Converts the raw State from the API into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="senderName"></param>
        /// <returns>State abbreviation as a string.</returns>
        public static string ParseState(string senderName)
        {
            string ParsedString = senderName.Remove(0, 17);
            int NumOfCharsToDelete = ParsedString.LastIndexOf(' ');
            ParsedString = ParsedString.Remove(0, NumOfCharsToDelete + 1);

            // ParsedString now contains either the full states name OR the states abbreviation. 
            // Now iterate through the Dictionary to match values and make sure to output the states abbreviation.
            ParsedString = ParsedString.ToUpper();
            foreach (var keyValuePair in StateDictionary)
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
            return ParsedString.Trim(',');
        }
        /// <summary>
        /// Converts the raw state into the correct format for the DB. Json tag: "senderName"
        /// </summary>
        /// <param name="senderName"></param>
        /// <returns>Truncated City as a string.</returns>
        public static string ParseCity(string senderName)
        {
            senderName = senderName.Remove(0,16).Trim(',');
            int NumOfCharsUntilState = senderName.LastIndexOf(' ');
            senderName = senderName.Substring(0, NumOfCharsUntilState);
            return senderName.ToUpper();
        }
        /// <summary>
        /// Converts the raw state into the correct format for the DB. Json tag: "description"
        /// </summary>
        /// <param name="description"></param>
        /// <returns>Truncated Description as a string.</returns>
        public static string ParseDescription(string description)
        {
            return description.Remove(0,13).Trim(',');
        }

        /// <summary>
        /// Iterates through a string to check for key words. Json tag: "NWSHeadline"
        /// </summary>
        /// <param name="description"></param>
        /// <returns>A string with descriptor words.</returns>
        public static string ParseForDescriptiveKeywords(string description)
        {

            string[] SeperatedWords = description.ToString().ToUpper().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                CombinedDescriptorWords = "UNKNOWN ";
            }
            return CombinedDescriptorWords;
        }
        /// <summary>
        /// Converts the raw Headline into the correct format for the DB. Json tag: "NWSheadline"
        /// </summary>
        /// <param name="NWSheadline"></param>
        /// <returns>Truncated NWSHeadline as a string.</returns>
        public static string ParseNWSHeadline(string NWSheadline)
        {
            return NWSheadline.Remove(0,13).Trim(',');
        }
        /// <summary>
        /// Converts the raw Severity into the correct format for the DB. Json tag: "severity"
        /// </summary>
        /// <param name="severity"></param>
        /// <returns>Truncated Severity as a string.</returns>
        public static string ParseSeverity(string severity)
        {
            return severity.Remove(0,10).Trim(',');
        }
        /// <summary>
        /// Converts the raw EventType into the correct format for the DB. Json tag: "event"
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public static string ParseEvent(string eventType)
        {
            return eventType.Remove(0, 7).Trim(',');
        }
        public static string ParseAreaDescription(string areaDesc)
        {
            return areaDesc.Remove(0, 10).Trim(',');
        }
        /// <summary>
        /// Takes a string and ensures no duplicate entries were found. Also removes UNKNOWN if entries are found.
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns>Filtered string of Keywords</returns>
        public static string CleanDescriptiveKeywords(string keywords)
        {
            // Check for duplicate keywords and removes them
            string[] KeywordsArray = keywords.Split(' ');
            string ReturnedString = "";
            for (int WordAsIndex = 0; WordAsIndex < KeywordsArray.Length; WordAsIndex++)
            {
                if (!ReturnedString.Contains(KeywordsArray[WordAsIndex]))
                {
                    ReturnedString += KeywordsArray[WordAsIndex] + " ";
                }
            }
            // Check if string contains more than just the word UNKNOWN.
            // This prevents outputting UNKNOWN if there are other keywords with it
            if (ReturnedString.Contains("UNKNOWN") && ReturnedString.Length > 8)
            {
                ReturnedString = ReturnedString.Replace("UNKNOWN", "");
            }
            return ReturnedString;
        }
    }
}
