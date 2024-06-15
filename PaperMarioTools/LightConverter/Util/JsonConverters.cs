using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace LightConverter
{
    public class VectorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object); // Can convert any object type
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Object)
            {
                if (token["W"] != null)
                    return token.ToObject<Vector4>();
                if (token["Z"] != null)
                    return token.ToObject<Vector3>();
                if (token["Y"] != null)
                    return token.ToObject<Vector2>();
            }
            return token.ToObject(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Vector4 vector4)
            {
                JObject obj = new JObject
            {
                { "X", vector4.X },
                { "Y", vector4.Y },
                { "Z", vector4.Z },
                { "W", vector4.W }
            };
                obj.WriteTo(writer);
            }
            else if (value is Vector3 vector3)
            {
                JObject obj = new JObject
                {
                { "X", vector3.X },
                { "Y", vector3.Y },
                { "Z", vector3.Z },
                };
                obj.WriteTo(writer);
            }
            else if (value is Vector2 vecto2)
            {
                JObject obj = new JObject
                {
                { "X", vecto2.X },
                { "Y", vecto2.Y },
                };
                obj.WriteTo(writer);
            }
            else
                writer.WriteValue(value);
        }
    }

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            return new Vector3((float)obj["X"], (float)obj["Y"], (float)obj["Z"]);
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject obj = new JObject
        {
            { "X", value.X },
            { "Y", value.Y },
            { "Z", value.Z }
        };
            obj.WriteTo(writer);
        }
    }

    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            return new Vector4((float)obj["X"], (float)obj["Y"], (float)obj["Z"], (float)obj["W"]);
        }

        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            JObject obj = new JObject
        {
            { "X", value.X },
            { "Y", value.Y },
            { "Z", value.Z },
            { "W", value.W }
        };
            obj.WriteTo(writer);
        }
    }
}
