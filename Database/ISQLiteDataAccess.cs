using System.Collections.Generic;

namespace WeatherAlert_DB.Database
{
    public interface ISQLiteDataAccess
    {
        long Count();

        /// <summary>
        /// Queries ALL Alerts from a DB.
        /// </summary>
        /// <returns>List of Alerts</returns>
        IEnumerable<Alert> Select(AlertFilter filter);

        /// <summary>
        /// Inserts objects in the Database.
        /// </summary>
        /// <param name="alert"></param>
        /// <returns>Returns false if object already exists. If object was added it returns true.</returns>
        bool Insert(Alert alert);

        /// <summary>
        /// Check if record already exists.
        /// </summary>
        /// <param name="alert"></param>
        /// <returns>True if Record exists, false if it doesn't</returns>
        bool AlertExists(Alert alert);

        /// <summary>
        /// UPDATE an Alert object in DB.
        /// </summary>
        void Update(Alert alert);

        /// <summary>
        /// Deletes ALL Alerts from the DB.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Delete an Alert from the DB.
        /// </summary>
        void Delete(string alertId);
    }
}