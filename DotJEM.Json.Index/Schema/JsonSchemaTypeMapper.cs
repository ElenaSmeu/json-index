using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace DotJEM.Json.Index.Schema
{
    public static class JsonSchemaTypeMapper
    {
        private static readonly IDictionary<JTokenType, JsonSchemaType> map;

        static JsonSchemaTypeMapper()
        {
            map = new Dictionary<JTokenType, JsonSchemaType>();
            map[JTokenType.None] = JsonSchemaType.None;
            map[JTokenType.Object] = JsonSchemaType.Object;
            map[JTokenType.Array] = JsonSchemaType.Array;
            map[JTokenType.Integer] = JsonSchemaType.Integer;
            map[JTokenType.Float] = JsonSchemaType.Float;
            map[JTokenType.String] = JsonSchemaType.String;
            map[JTokenType.Boolean] = JsonSchemaType.Boolean;
            map[JTokenType.Null] = JsonSchemaType.Null;

            //NOTE: Unsupported types, perhaps these should be any instead?
            map[JTokenType.Date] = JsonSchemaType.Any;
            map[JTokenType.Raw] = JsonSchemaType.Any;
            map[JTokenType.Bytes] = JsonSchemaType.Any;
            map[JTokenType.Guid] = JsonSchemaType.Any;
            map[JTokenType.Uri] = JsonSchemaType.Any;
            map[JTokenType.TimeSpan] = JsonSchemaType.Any;
            map[JTokenType.Undefined] = JsonSchemaType.Any;

            //NOTE: We should not be meeting these:
            //typeMapping[JTokenType.Constructor] = JsonSchemaType.None;
            //typeMapping[JTokenType.Property] = JsonSchemaType.None;
            //typeMapping[JTokenType.Comment] = JsonSchemaType.None;
            //typeMapping[JTokenType.Undefined] = JsonSchemaType.None;
            //typeMapping[JTokenType.Constructor] = JsonSchemaType.None;
        }

        public static JsonSchemaType ToSchemaType(this JTokenType self)
        {
            return map[self];
        }
    }
}