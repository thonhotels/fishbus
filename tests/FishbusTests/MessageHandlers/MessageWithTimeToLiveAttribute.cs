using System;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    [MessageLabel("ValidMessageWithTimeToLiveAttribute")]
    public class ValidMessageWithTimeToLiveAttribute
    {
        [TimeToLiveAttribute]
        public TimeSpan TimeToLive { get; set; }
    }

    [MessageLabel("InvalidMessageWithMoteThanOneTimeToLiveAttribute")]
    public class InvalidMessageWithMoreThanOneTimeToLiveAttribute
    {
        [TimeToLiveAttribute]
        public TimeSpan TimeToLive { get; set; }

        [TimeToLiveAttribute]
        public TimeSpan TimeToLive2 { get; set; }
    }

    [MessageLabel("InvalidMessageWithTimeToLiveAttributeAsString")]
    public class InvalidMessageWithTimeToLiveAttributeAsString
    {
        [TimeToLiveAttribute]
        public string TimeToLiveAsString { get; set; }
    }

    [MessageLabel("MessageWithNoTimeToLiveAttribute")]
    public class MessageWithNoTimeToLiveAttribute
    {
    }
}