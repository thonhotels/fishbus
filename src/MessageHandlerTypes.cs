using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Thon.Hotels.FishBus;

public static class MessageHandlerTypes
{
    public static IEnumerable<Type> GetAll(Assembly assembly) =>
        GetAllTypesImplementingOpenGenericType(typeof(IHandleMessage<>), new[] {assembly});

    public static IEnumerable<Type> GetAll(Assembly[] assemblies) =>
        GetAllTypesImplementingOpenGenericType(typeof(IHandleMessage<>), assemblies);

    private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly[] assemblies)
    {
        return assemblies.SelectMany(a => a.GetTypes())
            .SelectMany(x => x.GetInterfaces(), (handlerType, interfaceType) => new { handlerType, interfaceType })
            .Select(t => new { type = t, baseType = t.handlerType.BaseType })
            .Where(t =>
                t.baseType != null && t.baseType.IsGenericType &&
                openGenericType.IsAssignableFrom(t.baseType.GetGenericTypeDefinition()) ||
                t.type.interfaceType.IsGenericType &&
                openGenericType.IsAssignableFrom(t.type.interfaceType.GetGenericTypeDefinition()))
            .Select(t => t.type.handlerType)
            .Distinct();
    }
}