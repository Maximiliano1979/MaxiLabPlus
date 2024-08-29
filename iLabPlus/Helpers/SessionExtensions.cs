using Microsoft.AspNetCore.Http;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static iLabPlus.Helpers.SessionExtensions;

namespace iLabPlus.Helpers
{
    public static class SessionExtensions
    {
        // Define una clase para representar la estructura de los objetos en tu JSON
        public class Mensaje
        {
            public int Role { get; set; }
            public string Content { get; set; }
            public object ToolCalls { get; set; }
            public object ToolCallId { get; set; }
            public object Function { get; set; }
            public object Name { get; set; }
        }

        public static void SetObject(this ISession session, string key, object value)
        {
            var xxx = JsonConvert.SerializeObject(value);

            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static List<Message> GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null) return new List<Message>();

            // Deserializar directamente al tipo deseado si es compatible
            var mensajes = JsonConvert.DeserializeObject<List<Mensaje>>(value);
            var messages = mensajes.Select(m => new Message((Role)m.Role, m.Content)).ToList();

            return messages;
        }


        public static void SetObjectStr(this ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        public static string GetObjectStr<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null) return null;

            return value;
        }




    }
}
