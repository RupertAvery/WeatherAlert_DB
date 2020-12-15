using System;
using System.Collections.Generic;

namespace WeatherAlert_DB.Database
{
    public class AlertFilter
    {
        public string EventID;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EventType { get; set; }
        public string State { get; set; }
        public string Severity { get; set; }
        public IEnumerable<string> Keywords { get; set; }
    }
}
