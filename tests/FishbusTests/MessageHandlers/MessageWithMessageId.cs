using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    [MessageLabel("A.Message.With.MessageId")]
    public class MessageWithMessageId
    {
        [MessageId]
        public string Id { get; set; }
    }

    [MessageLabel("A.Message.With.Too.Many.MessageIds")]
    public class MessageWithTooManyMessageIds
    {
        [MessageId]
        public string Id { get; set; }
        [MessageId]
        public string AnothterId { get; set; }
    }

    [MessageLabel("A.Message.Without.MessageId")]
    public class MessageWithoutMessageId
    {
        public string Id { get; set; }
        public string AnothterId { get; set; }
    }
}