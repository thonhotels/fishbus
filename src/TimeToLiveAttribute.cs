using System;

namespace Thon.Hotels.FishBus
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TimeToLiveAttribute : Attribute
    {
    }
}