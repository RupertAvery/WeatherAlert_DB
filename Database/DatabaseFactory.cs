namespace WeatherAlert_DB.Database
{

    // This is just one way to do this.
    // 
    public static class DatabaseFactory
    {
        private static ISQLiteDataAccess _instance;
        private static bool _isUsingDummyDb;

        public static bool IsUsingDummyDB
        {
            get => _isUsingDummyDb;
            set {
                _isUsingDummyDb = value;
                // force GetDatabase() to return a new value the next time we query
                _instance = null;
            }
        }

        public static ISQLiteDataAccess GetDatabase()
        {
            if (_instance == null)
            {
                if (IsUsingDummyDB)
                {
                    _instance = new DebugSQLiteDataAccess();
                }
                else
                {
                    _instance = new LiveSQLiteDataAccess();
                }
            }
            return _instance;
        }
    }
}