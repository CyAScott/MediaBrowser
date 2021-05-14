using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MediaBrowser.Attributes;
using MediaBrowser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Configuration
{
    [Installer(Group = nameof(JsonSerializerInstaller), Priority = int.MaxValue)]
    public class JsonSerializerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Converters.Add(new RoleSetJsonSerializer());
        }
    }

    public class RoleSetJsonSerializer : JsonConverter<RoleSet>
    {
        public override RoleSet ReadJson(JsonReader reader, Type objectType, RoleSet existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
            {
                reader.Skip();
                return null;
            }

            return new RoleSet(JArray.Load(reader)
                .Where(role => role == null || role.Type == JTokenType.String)
                .Select(role => role.Value<string>()));
        }

        public override void WriteJson(JsonWriter writer, RoleSet value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartArray();

                foreach (var role in value)
                {
                    writer.WriteValue(role);
                }

                writer.WriteEndArray();
            }
        }
    }
}
