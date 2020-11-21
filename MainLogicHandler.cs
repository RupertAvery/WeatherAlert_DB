using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WeatherAlert_DB
{
    /// <summary>
    /// This class handles the primary applications logic.
    /// </summary>
    class MainLogicHandler
    {
        private static async void CallApiEvent(Object source, ElapsedEventArgs e)
        {
            InsertInfoToDB();
        }
        /// <summary>
        /// Start an auto resetting timer to request the API.
        /// </summary>
        public static void StartApiTimer()
        {
            Timer ApiTimer = new Timer(3600000);
            ApiTimer.AutoReset = true;
            ApiTimer.Elapsed += new ElapsedEventHandler(CallApiEvent);
            ApiTimer.Start();
        }
        private static void InsertInfoToDB()
        {
            var AlertInfoList = NWS_ApiController.ReturnApiCall();
        }
    }
}
