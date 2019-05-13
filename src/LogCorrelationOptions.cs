using System;

namespace Thon.Hotels.FishBus
{
    public class LogCorrelationOptions
    {
        public string LogPropertyName { get; set; }
        public string MessagePropertyName { get; set; }
        public Action<string> SetCorrelationLogId { get; set; }
    }
}