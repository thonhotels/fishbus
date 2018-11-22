using System;
using System.Linq;

namespace Thon.Hotels.FishBus
{
    public static class ConnectionStringSplitter
    {
        public static (string connectionString, string entityPath) Split(string fullConnectionString)
        {
            var connectionStringList = fullConnectionString.Split(';');
            var connectionString = String.Join(";", connectionStringList.Where(s => !s.Contains("EntityPath")));
            var entityPath = connectionStringList
                .Where(s => s.Contains("EntityPath"))
                .LastOrDefault()?.Split('=').Last();
            return (connectionString, entityPath);
        }
    }
}
