using System.Configuration;

namespace WeatherAlert_DB.Database
{
    public class DebugSQLiteDataAccess : SQLiteDataAccess
    {
        public DebugSQLiteDataAccess() : base(ConfigurationManager.ConnectionStrings["DummyDB"].ConnectionString)
        {
        }

        public override void Delete(string alertId)
        {
            // Prevent updates to this DB
        }

        public override void Update(Alert alert)
        {
            // Prevent updates to this DB
        }

        public override void DeleteAll()
        {
            // Prevent updates to this DB
        }

        public override bool Insert(Alert alert)
        {
            // Prevent updates to this DB
            return true;
        }
    }
}