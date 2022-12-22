using System;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers;

[MessageSubject("ValidMessageWithTimeToLiveAttribute")]
public class ValidMessageWithTimeToLiveAttribute
{
    [TimeToLive]
    public TimeSpan TimeToLive { get; set; }
}

[MessageSubject("InvalidMessageWithMoteThanOneTimeToLiveAttribute")]
public class InvalidMessageWithMoreThanOneTimeToLiveAttribute
{
    [TimeToLive]
    public TimeSpan TimeToLive { get; set; }

    [TimeToLive]
    public TimeSpan TimeToLive2 { get; set; }
}

[MessageSubject("InvalidMessageWithTimeToLiveAttributeAsString")]
public class InvalidMessageWithTimeToLiveAttributeAsString
{
    [TimeToLive]
    public string TimeToLiveAsString { get; set; }
}

[MessageSubject("MessageWithNoTimeToLiveAttribute")]
public class MessageWithNoTimeToLiveAttribute
{
}