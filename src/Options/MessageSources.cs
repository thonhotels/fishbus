using System.Collections.Generic;

namespace Thon.Hotels.FishBus.Options
{
    public class Subscription
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }

    public class Queue
    {
        public string ConnectionString { get; set; }
    }

    public class MessageSources
    {
        public ICollection<Subscription> Subscriptions { get; } = new List<Subscription>();
        public ICollection<Queue> Queues { get; } = new List<Queue>();
    }
}
