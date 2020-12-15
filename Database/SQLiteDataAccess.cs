using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace WeatherAlert_DB.Database
{
    public abstract class SQLiteDataAccess : ISQLiteDataAccess
    {
        private readonly string _connectionString;

        protected SQLiteDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual long Count()
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                // Build sequel query
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT(*) FROM Alerts";
                    command.Connection.Open();
                    var result = (long)command.ExecuteScalar();
                    command.Connection.Close();
                    return result;
                }
            }
        }

        /// <summary>
        /// Queries ALL Alerts from a DB.
        /// </summary>
        /// <returns>List of Alerts</returns>
        public virtual IEnumerable<Alert> Select(AlertFilter filter)
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                // Build sequel query
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;

                    var whereClauses = new List<string>();

                    if (!string.IsNullOrEmpty(filter.EventID))
                    {
                        command.Parameters.AddWithValue("EventID", filter.EventID);
                        whereClauses.Add("Id LIKE '%@EventID%'");
                    }
                    if (filter.StartDate != null && filter.EndDate != null)
                    {
                        command.Parameters.AddWithValue("StartDate", filter.StartDate);
                        command.Parameters.AddWithValue("EndDate", filter.EndDate);
                        whereClauses.Add("Date BETWEEN @StartDate AND @EndDate");
                    }
                    if (!string.IsNullOrEmpty(filter.EventType))
                    {
                        command.Parameters.AddWithValue("EventType", filter.EventType);
                        whereClauses.Add("EventType = @EventType");
                    }
                    if (!string.IsNullOrEmpty(filter.State))
                    {
                        command.Parameters.AddWithValue("State", filter.State);
                        whereClauses.Add("State = @State");
                    }
                    if (!string.IsNullOrEmpty(filter.Severity))
                    {
                        command.Parameters.AddWithValue("Severity", filter.Severity);
                        whereClauses.Add("Severity = @Severity");
                    }
                    if (filter.Keywords.Any())
                    {
                        var keywordsFilter = new List<string>();
                        foreach (var Keyword in filter.Keywords)
                        {
                            keywordsFilter.Add($"DescriptionKeywords LIKE '%{Keyword}%'");
                        }
                        whereClauses.Add($"{string.Join(" OR ", keywordsFilter.Select(Paren))}");
                    }

                    command.CommandText = $"SELECT * FROM Alerts {(whereClauses.Any() ? " WHERE " : "")}{string.Join(" AND ", whereClauses.Select(Paren))}";

                    string Paren(string value)
                    {
                        return $"({value})";
                    }

                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Read from DB
                        while (reader.Read())
                        {
                            yield return new Alert(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                                reader.GetString(3), reader.GetString(4), reader.GetString(5),
                                reader.GetString(6), reader.GetString(7), reader.GetString(8),
                                reader.GetString(9), reader.GetString(10));
                        }
                        reader.Close();
                        command.Connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Inserts objects in the Database.
        /// </summary>
        /// <param name="alert"></param>
        /// <returns>Returns false if object already exists. If object was added it returns true.</returns>
        public virtual bool Insert(Alert alert)
        {
            // Check if DB entry already exists
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", alert.Id);
                    command.Parameters.AddWithValue("@Date", alert.Date);
                    command.Parameters.AddWithValue("@Time", alert.Time);
                    command.Parameters.AddWithValue("@EventType", alert.EventType);
                    command.Parameters.AddWithValue("@State", alert.State);
                    command.Parameters.AddWithValue("@City", alert.City);
                    command.Parameters.AddWithValue("@Severity", alert.Severity);
                    command.Parameters.AddWithValue("@NWSHeadline", alert.NWSHeadline);
                    command.Parameters.AddWithValue("@Description", alert.Description);
                    command.Parameters.AddWithValue("@DescriptionKeywords", alert.DescriptionKeywords);
                    command.Parameters.AddWithValue("@AreaDescription", alert.AreaDescription);
                    command.CommandText =
                        @"INSERT INTO Alerts (Id,Date,Time,EventType,State,City,Severity,NWSHeadline,Description,DescriptionKeywords,AreaDescription) 
 values (@Id,@Date,@Time,@EventType,@State,@City,@Severity,@NWSHeadline,@Description,@DescriptionKeywords,@AreaDescription)
 WHERE NOT EXISTS (SELECT Id FROM Alerts WHERE Id = @id)";
                    command.Connection.Open();
                    var result = (int)command.ExecuteNonQuery();
                    command.Connection.Close();
                    return result == 1;
                }
            }
        }
        /// <summary>
        /// Check if record already exists.
        /// </summary>
        /// <param name="alert"></param>
        /// <returns>True if Record exists, false if it doesn't</returns>
        public bool AlertExists(Alert alert)
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                // Build sequel query
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", alert.Id.Replace('-', '_'));
                    command.CommandText = "SELECT Id FROM Alerts WHERE Id = @Id";
                    command.Connection.Open();
                    var id = (string)command.ExecuteScalar();
                    command.Connection.Close();
                    return id != null && id == alert.Id;
                }
            }
        }

        /// <summary>
        /// UPDATE an Alert object in DB.
        /// </summary>
        public virtual void Update(Alert alert)
        {
            throw new NotImplementedException("To be completed later");
        }

        /// <summary>
        /// Deletes ALL Alerts from the DB.
        /// </summary>
        public virtual void DeleteAll()
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "DELETE FROM Alerts";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Delete an Alert from the DB.
        /// </summary>
        public virtual void Delete(string alertId)
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = (SQLiteConnection)connection;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", alertId.Replace('-', '_'));
                    command.CommandText = "DELETE FROM Alerts WHERE Id = @Id";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }
    }
}