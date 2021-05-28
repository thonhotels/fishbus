using System;

namespace Thon.Hotels.FishBus
{
    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("Use MessageSubject attribute")]
    public class MessageLabelAttribute : Attribute
    {
        public string Label { get; }

        public MessageLabelAttribute(string label)
        {
            Label = label;
        }
    }
}