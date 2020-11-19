using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Microsoft.Data.Sqlite;

namespace WeatherAlert_DB
{
    class SQLite_Data_Access
    {
        // Connection strings currently available to the application.
        public enum ConnectionString
        {
            MainDB,
            DummyDB,
        }
        // Retrieve the connection string from config for DB.
        private static string LoadConnectionString(ConnectionString connectionString)
        {
            return ConfigurationManager.ConnectionStrings[connectionString.ToString()].ConnectionString;
        }

        /// <summary>
        /// Queries ALL Alerts from DB.
        /// </summary>
        /// <returns>List of Alerts</returns>
        public static List<Alert> SelectAll_DB(ConnectionString connectionstringDB)
        {
            using (IDbConnection connection = new SqliteConnection(LoadConnectionString(connectionstringDB)))
            {
                List<Alert> alerts = new List<Alert>();

                // Build sequel query
                SqliteCommand CMD = new SqliteCommand();
                CMD.Connection = (SqliteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = "SELECT * FROM Alerts";
                CMD.Connection.Open();
                SqliteDataReader rdr = CMD.ExecuteReader();

                // Read from DB
                while (rdr.Read())
                {
                    var Alert = new Alert(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7));
                    alerts.Add(Alert);
                }
                rdr.Close();
                CMD.Connection.Close();
                return alerts;
            }
        }
        /// <summary>
        /// INSERT Alert objects in DB.
        /// </summary>
        public static void InsertIn_DB(ConnectionString connectionStringDB,Alert alert)
        {
            using (IDbConnection connection = new SqliteConnection(LoadConnectionString(connectionStringDB)))
            {
                SqliteCommand CMD = new SqliteCommand();
                CMD.Connection = (SqliteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.Parameters.AddWithValue("@Id", alert.Id);
                CMD.Parameters.AddWithValue("@Date", alert.Date);
                CMD.Parameters.AddWithValue("@EventType", alert.EventType);
                CMD.Parameters.AddWithValue("@State", alert.State);
                CMD.Parameters.AddWithValue("@City", alert.City);
                CMD.Parameters.AddWithValue("@Severity", alert.Severity);
                CMD.Parameters.AddWithValue("@NWSHeadline", alert.NWSHeadline);
                CMD.Parameters.AddWithValue("@DescriptionKeywords", alert.DescriptionKeywords);
                CMD.CommandText = @"INSERT INTO Alerts (Id,Date,EventType,State,City,Severity,NWSHeadline,DescriptionKeywords) 
                                    values (@Id,@Date,@EventType,@State,@City,@Severity,@NWSHeadline,@DescriptionKeywords)";
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }
        /// <summary>
        /// UPDATE an Alert object in DB.
        /// </summary>
        public static void UpdateIn_DB()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Deletes ALL Alerts from the DB.
        /// </summary>
        public static void DeleteAllIn_DB(ConnectionString connectionStringDB)
        {
            using (IDbConnection connection = new SqliteConnection(LoadConnectionString(connectionStringDB)))
            {
                SqliteCommand CMD = new SqliteCommand();
                CMD.Connection = (SqliteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = string.Format("DELETE FROM Alerts");
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }
        /// <summary>
        /// Delete an Alert from the DB.
        /// </summary>
        public static void DeleteIn_DB(ConnectionString connectionStringDB, string alertId)
        {
            using (IDbConnection connection = new SqliteConnection(LoadConnectionString(connectionStringDB)))
            {
                SqliteCommand CMD = new SqliteCommand();
                CMD.Connection = (SqliteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = string.Format("DELETE FROM Alerts WHERE Id='{0}'", alertId);
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }


        
    }
}
