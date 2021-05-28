using System.Collections.Generic;

namespace Thon.Hotels.FishBus.Options
{
    public class MessageSource
    {
        public MessageSource()
        {
            ConnectionString = "";
            MaxConcurrentCalls = 1;
            Namespace = "";
            EntityName = "";
            CredentialType = "";
        }

        public string ConnectionString { get; set; }
        public int MaxConcurrentCalls { get; set; }
        public string Namespace { get; set; }
        public string EntityName { get; set; }
        public string CredentialType { get; set; }
    }
    public class Subscription : MessageSource
    {
        public string Name { get; set; }
    }

    public class Queue : MessageSource
    {
    }

    public class MessageSources
    {
        public ICollection<Subscription> Subscriptions { get; } = new List<Subscription>();
        public ICollection<Queue> Queues { get; } = new List<Queue>();
    }
}
