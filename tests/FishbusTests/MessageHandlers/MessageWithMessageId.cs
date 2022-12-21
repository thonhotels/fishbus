using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    [MessageSubject("A.Message.With.MessageId")]
    public class MessageWithMessageId
    {
        [MessageId]
        public string Id { get; set; }
    }

    [MessageSubject("A.Message.With.Too.Many.MessageIds")]
    public class MessageWithTooManyMessageIds
    {
        [MessageId]
        public string Id { get; set; }
        [MessageId]
        public string AnothterId { get; set; }
    }

    [MessageSubject("A.Message.Without.MessageId")]
    public class MessageWithoutMessageId
    {
        public string Id { get; set; }
        public string AnothterId { get; set; }
    }
}