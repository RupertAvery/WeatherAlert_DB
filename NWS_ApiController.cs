using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherAlert_DB
{
    class NWS_ApiController
    {
        public static HttpClient client = new HttpClient();
        /// <summary>
        /// Calls a request to the NWS API.
        /// </summary>
        /// <returns>String with all reader content from the GET/HTTP request.</returns>
        public static string RequestNWSApi(string httpRequest)
        {
            // In order to request API access header has to be declared.
            client.DefaultRequestHeaders.UserAgent.Add(System.Net.Http.Headers.ProductInfoHeaderValue.Parse("(Weather_DB107425625345672, NA)"));
            try
            {
                var httpResponse = client.GetStreamAsync(httpRequest);
                StreamReader rdr = new StreamReader(httpResponse.Result);
                var content = rdr.ReadToEnd();
                return content;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("NWS Services currently unavailable. Please try again later.");
                throw;
            }
            
            
        }
        public static List<string> ParseReaderStringForKeywords(string[] keywordsToSearchFor, string readerTxt)
        {
            // Split the long Json request in a temp string array.
            string[] SplitLines = readerTxt.Split(',');
            // This is the returned list
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
    }
}
