using System;

namespace Thon.Hotels.FishBus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageSubjectAttribute : Attribute
    {
        public string Subject { get; }

        public MessageSubjectAttribute(string subject)
        {
            Subject = subject;
        }
    }
}