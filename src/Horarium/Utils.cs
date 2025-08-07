using System;
using System.Reflection;
using System.Text.Json;
using Cronos;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Horarium
{
    internal static class Utils
    {
        public static string ToJson(this object obj, Type type, JsonSerializerOptions jsonSerializerSettings)
        {
           return  JsonSerializer.Serialize(obj, type, jsonSerializerSettings);
        }

        public static object FromJson(this string json, Type type, JsonSerializerOptions jsonSerializerSettings)
        {
            return JsonSerializer.Deserialize(json, type, jsonSerializerSettings);
        }

        public static string AssemblyQualifiedNameWithoutVersion(this Type type)
        {
            string retValue = type.FullName + ", " + type.GetTypeInfo().Assembly.GetName().Name;
            return retValue;
        }
        
        public static DateTime? ParseAndGetNextOccurrence(string cron)
        {
            var expression = CronExpression.Parse(cron, CronFormat.IncludeSeconds);
            
            return expression.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local);
        }
    }
}