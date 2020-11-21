using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SQLite;

namespace WeatherAlert_DB
{
    class SQLite_Data_Access
    {
        // Is the user wanting to use the DummyDB
        public static bool IsUsingDummyDB = true;
        // DB ConnectionStrings currently available to the application.
        public enum ConnectionString
        {
            MainDB,
            DummyDB,
        }
        // Retrieve the connection string from config for DB.
        private static string LoadConnectionString(ConnectionString connectionStringDB)
        {
            return ConfigurationManager.ConnectionStrings[connectionStringDB.ToString()].ConnectionString;
        }

        /// <summary>
        /// Queries ALL Alerts from a DB.
        /// </summary>
        /// <returns>List of Alerts</returns>
        public static List<Alert> SelectAll_DB(ConnectionString connectionStringDB)
        {
            using (IDbConnection connection = new  SQLiteConnection(LoadConnectionString(connectionStringDB)))
            {
                List<Alert> alerts = new List<Alert>();

                // Build sequel query
                SQLiteCommand CMD = new SQLiteCommand();
                CMD.Connection = (SQLiteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = "SELECT * FROM Alerts";
                CMD.Connection.Open();
                SQLiteDataReader rdr = CMD.ExecuteReader();

                // Read from DB
                while (rdr.Read())
                {
                    var Alert = new Alert(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2),
                                           rdr.GetString(3), rdr.GetString(4), rdr.GetString(5),
                                           rdr.GetString(6), rdr.GetString(7), rdr.GetString(8), rdr.GetString(9));
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
        public static void InsertIn_DB(Alert alert)
        {
            using (IDbConnection connection = new SQLiteConnection(LoadConnectionString(ConnectionString.MainDB)))
            {
                SQLiteCommand CMD = new SQLiteCommand();
                CMD.Connection = (SQLiteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.Parameters.AddWithValue("@Id", alert.Id);
                CMD.Parameters.AddWithValue("@Date", alert.Date);
                CMD.Parameters.AddWithValue("@Time", alert.Time);
                CMD.Parameters.AddWithValue("@EventType", alert.EventType);
                CMD.Parameters.AddWithValue("@State", alert.State);
                CMD.Parameters.AddWithValue("@City", alert.City);
                CMD.Parameters.AddWithValue("@Severity", alert.Severity);
                CMD.Parameters.AddWithValue("@NWSHeadline", alert.NWSHeadline);
                CMD.Parameters.AddWithValue("@DescriptionKeywords", alert.DescriptionKeywords);
                CMD.Parameters.AddWithValue("@AreaDescription", alert.AreaDescription);
                CMD.CommandText = @"INSERT INTO Alerts (Id,Date,Time,EventType,State,City,Severity,NWSHeadline,DescriptionKeywords,AreaDescription) 
                                    values (@Id,@Date,@Time,@EventType,@State,@City,@Severity,@NWSHeadline,@DescriptionKeywords,@AreaDescription)";
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();

    }
        }
        /// <summary>
        /// UPDATE an Alert object in DB.
        /// </summary>
        public static void UpdateIn_DB(string id)
        {
            using (IDbConnection connection = new SQLiteConnection(LoadConnectionString(ConnectionString.MainDB)))
            {
                throw new NotImplementedException("To be completed later");
                SQLiteCommand CMD = new SQLiteCommand();
                CMD.Connection = (SQLiteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = "UPDATE WHERE";
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }
        /// <summary>
        /// Deletes ALL Alerts from the DB.
        /// </summary>
        public static void DeleteAllIn_DB()
        {
            using (IDbConnection connection = new SQLiteConnection(LoadConnectionString(ConnectionString.MainDB)))
            {
                SQLiteCommand CMD = new SQLiteCommand();
                CMD.Connection = (SQLiteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = "DELETE FROM Alerts";
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }
        /// <summary>
        /// Delete an Alert from the DB.
        /// </summary>
        public static void DeleteIn_DB(string alertId)
        {
            using (IDbConnection connection = new SQLiteConnection(LoadConnectionString(ConnectionString.MainDB)))
            {
                SQLiteCommand CMD = new SQLiteCommand();
                CMD.Connection = (SQLiteConnection)connection;
                CMD.CommandType = CommandType.Text;
                CMD.CommandText = string.Format("DELETE FROM Alerts WHERE Id='{0}'", alertId);
                CMD.Connection.Open();
                CMD.ExecuteNonQuery();
                CMD.Connection.Close();
            }
        }
    }
}
