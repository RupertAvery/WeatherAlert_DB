using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WeatherAlert_DB
{
    /// <summary>
    /// Provides Logging utility to a text file.
    /// </summary>
    class LogHandler
    {
        string CurrentFilePath;
        public string LogMessage;
        public int NumOfObjects;
        DateTime DateTimeNow;
        Exception LoggedException;
        
        public LogHandler(string logMessage, Exception e = null)
        {
            LogMessage += logMessage;
            DateTimeNow = DateTime.Now;
            LoggedException = e;
            CurrentFilePath = Environment.CurrentDirectory + "\\Weather_DB_LOG.txt";
        }
        /// <summary>
        /// Checks current working directory to make sure Log File exists. If it doesnt then it creates one.
        /// </summary>
        private void CheckForLogFile()
        {
            if (!File.Exists(CurrentFilePath))
            {
                File.WriteAllText(CurrentFilePath,
                    $"---------------------------------------------------------\n" +
                    $"| Log File Successfully Generated @ {DateTimeNow.ToShortDateString()} " +
                    $"{DateTimeNow.ToShortTimeString()} |\n" +
                    $"---------------------------------------------------------\n");
            }
        }
        /// <summary>
        /// Writes this log entry to the Log File.
        /// </summary>
        public void WriteLogFile()
        {
            CheckForLogFile();
            if (LoggedException != null)
            {
                File.AppendAllText(CurrentFilePath,
                     String.Format("\n[Date: {0} Time: {1}]\n" +
                                   "Log Details: {2}\n" +
                                   "EXCEPTION: {3}\n", DateTimeNow.ToShortDateString(), 
                                   DateTimeNow.ToShortTimeString(), LogMessage, LoggedException.Message));
            }
            else
            {
                File.AppendAllText(CurrentFilePath,
                     String.Format("\n[Date: {0} Time: {1}]\n" +
                                   "Log Details: {2}\n", DateTimeNow.ToShortDateString(), 
                                   DateTimeNow.ToShortTimeString(), LogMessage));
            } 
        }
    }
}
