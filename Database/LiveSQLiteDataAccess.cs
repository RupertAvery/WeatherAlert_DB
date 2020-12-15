using System.Configuration;

namespace WeatherAlert_DB.Database
{
    public class LiveSQLiteDataAccess : SQLiteDataAccess
    {
        public LiveSQLiteDataAccess() : base(ConfigurationManager.ConnectionStrings["MainDB"].ConnectionString)
        {
        }
    }
}