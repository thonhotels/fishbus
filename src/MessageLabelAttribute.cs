using System;

namespace Thon.Hotels.FishBus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageLabelAttribute : Attribute
    {
        public string Label { get; }

        public MessageLabelAttribute(string label)
        {
            Label = label;
        }
    }
}