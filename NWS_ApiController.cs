using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Net;

namespace WeatherAlert_DB
{
    /// <summary>
    /// Used to contact the NWS API.
    /// </summary>
    class NWS_ApiController
    {
        public static HttpClient client = new HttpClient();
        /// <summary>
        /// Calls a request to the NWS API.
        /// </summary>
        /// <returns>String with all reader content from the GET/HTTP request.</returns>
        private static string RequestNWSApi(string httpRequest)
        {
            // In order to request API access header has to be declared.
            client.DefaultRequestHeaders.UserAgent.Add(System.Net.Http.Headers.ProductInfoHeaderValue.Parse("(Weather_DB107425625345672, NA)"));
            // Read from GET request 
            try
            {
                var httpResponse = client.GetStreamAsync(httpRequest);
                StreamReader rdr = new StreamReader(httpResponse.Result);
                var content = rdr.ReadToEnd();

                // Log info
                var Log = new LogHandler("Services requested successfully.");
                Log.WriteLogFile();
                return content;
            }
            catch (WebException e)
            {
                // Catch the exception and generate a Log entry with the exception 
                MessageBox.Show("NWS Services currently unavailable." +
                                 "\nException has been logged." +
                                 "\nPlease try again later.");

                // Log info
                var Log = new LogHandler("ERROR: NWS Services could not be requested.", e);
                Log.WriteLogFile();
                return "";
            }
            
            
        }
        private static List<string> ParseReaderStringForKeywords(string[] keywordsToSearchFor, string readerTxt)
        {
            // Split the long Json request in a temp string array. Declare a list to return.
            string[] SplitLines = readerTxt.Split(',');
            List<string> ReturnedStringList = new List<string>();

            // Iterate through the reader info and seperate lines by keywords
            foreach (var line in SplitLines)
            {
                foreach (var keyword in keywordsToSearchFor)
                {
                    if (line.Contains(keyword))
                    {
                        // Extract only the useful information and remove any extra characters before storing in a List.
                        string CleanedLine = "";
                        foreach (char c in line)
                        {
                            if (!(c == '"' || c == '\\'|| c == ']' || c == '['))
                            {
                                CleanedLine += c;
                            }
                        }
                        CleanedLine = CleanedLine.Replace("\n", "");
                        int NumCharsToDelete = CleanedLine.IndexOf(keyword);
                        if (NumCharsToDelete > 0)
                        {
                            CleanedLine = CleanedLine.Remove(0, NumCharsToDelete);
                        }
                        ReturnedStringList.Add(CleanedLine.Replace("  ",""));
                    }
                }
            }
            return ReturnedStringList;
        }
        /// <summary>
        /// Sends a NWS Alert GET request.
        /// </summary>
        /// <returns>Filtered List of Alert data.</returns>
        public static List<string> ReturnApiCall()
        {
            // Setup the API request and return the filtered output as a string list for later use.
            string Request = "https://api.weather.gov/alerts/active?status=actual&message_type=alert&certainty=observed";
            string[] Keywords = { "@id\":", "sent\":", "event\":", "senderName\":", "severity\":", "NWSheadline\":", "areaDesc\":" };
            return ParseReaderStringForKeywords(Keywords,RequestNWSApi(Request));
        }
    }
}
