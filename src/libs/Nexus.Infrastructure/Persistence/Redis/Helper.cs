using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Infrastructure.Persistence.Redis
{
    internal static class Helper
    {
        internal static string GetHashValue(HashEntry[] hashEntries, string fieldName)
        {
            return hashEntries.FirstOrDefault(e => e.Name == fieldName).Value.ToString();
        }

        internal static int GetHashValueAsInt(HashEntry[] hashEntries, string fieldName)
        {
            var value = GetHashValue(hashEntries, fieldName);
            return int.TryParse(value, out var result) ? result : 0;
        }

        internal static T GetHashValueAsEnum<T>(HashEntry[] hashEntries, string fieldName) where T : struct, Enum
        {
            var value = GetHashValue(hashEntries, fieldName);
            return Enum.TryParse<T>(value, out var result) ? result : default(T);
        }
    }
}
