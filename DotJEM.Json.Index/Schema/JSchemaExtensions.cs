using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Schema;

namespace DotJEM.Json.Index.Schema
{
    public static class JSchemaExtensions
    {
        public static JSchema Merge(this JSchema self, JSchema other)
        {
            if (other == null)
                return self;

            self.Type = self.Type | other.Type;
            self.ExtendedType = self.ExtendedType | other.ExtendedType;
            self.Indexed = self.Indexed || other.Indexed;
            self.Required = self.Required || other.Required;

            self.Title = MostQualifying(self.Title, other.Title);
            self.Description = MostQualifying(self.Description, other.Description);
            self.Area = MostQualifying(self.Area, other.Area);
            self.ContentType = MostQualifying(self.ContentType, other.ContentType);
            self.Field = MostQualifying(self.Field, other.Field);

            self.Items = self.Items != null ? self.Items.Merge(other.Items) : other.Items;

            self.MergeExtensions(other);

            if (other.Properties == null)
            {
                return self.EnsureValidObject();
            }

            if (self.Properties == null)
            {
                self.Properties = other.Properties;
                return self.EnsureValidObject();
            }

            foreach (KeyValuePair<string, JSchema> pair in other.Properties)
            {
                if (self.Properties.ContainsKey(pair.Key))
                {
                    self.Properties[pair.Key] = self.Properties[pair.Key].Merge(pair.Value);
                }
                else
                {
                    self.Properties.Add(pair.Key, pair.Value);
                }
            }
            return self;
        }

        private static JSchema EnsureValidObject(this JSchema self)
        {
            if (self.Type.HasFlag(JsonSchemaType.Object) && self.Properties == null)
                self.Properties = new JSchemaProperties();

            //if (Type.HasFlag(JsonSchemaType.Array) && Items == null)
            //    Items = new JSchema();

            return self;
        }

        private static string MostQualifying(string self, string other)
        {
            return string.IsNullOrEmpty(other) ? (self ?? other) : other;
        }

        public static JsonSchemaExtendedType LookupExtentedType(this JSchema self, string field)
        {
            try
            {
                if (self.Field == null || !field.StartsWith(self.Field))
                    return JsonSchemaExtendedType.None;

                if (self.Field == field)
                    return self.ExtendedType;

                JsonSchemaExtendedType extendedTypes = self.Items != null
                    ? self.Items.LookupExtentedType(field) : JsonSchemaExtendedType.None;

                if (self.Properties != null)
                {
                    extendedTypes = extendedTypes | self.Properties.Aggregate(JsonSchemaExtendedType.None,
                        (types, next) => LookupExtentedType(next.Value, field) | types);
                }

                return extendedTypes;
            }
            catch (NullReferenceException ex)
            {
                //NOTE: This is defensive... 
                if (self.Properties == null)
                    throw;

                var kv = self.Properties.Where(p => p.Key == null || p.Value == null).ToArray();
                if (kv.Length <= 0)
                    throw;

                string message = "Found " + kv.Length + " propertie(s) where either the key or value was null in '" 
                                 + self.ContentType + ":" + self.Field + "'.\n\r" 
                                 + string.Join("\n\r", kv.Select(v => v.Key + " : " + v.Value));
                throw new NullReferenceException(message, ex);
            }
        }

        public static IEnumerable<JSchema> Traverse(this JSchema self)
        {
            try
            {
                var all = Enumerable.Empty<JSchema>().Union(new[] { self });
                if (self.Items != null)
                {
                    all = all.Union(self.Items.Traverse());
                }

                if (self.Properties != null)
                {
                    all = all.Union(self.Properties.Values.SelectMany(property => property.Traverse()));
                }

                return all;
            }
            catch (NullReferenceException ex)
            {
                //NOTE: This is defensive... 
                if (self.Properties == null)
                    throw;

                var kv = self.Properties.Where(p => p.Key == null || p.Value == null).ToArray();
                if (kv.Length <= 0)
                    throw;

                string message = "Found " + kv.Length + " propertie(s) where either the key or value was null in '"
                                 + self.ContentType + ":" + self.Field + "'.\n\r"
                                 + string.Join("\n\r", kv.Select(v => v.Key + " : " + v.Value));
                throw new NullReferenceException(message, ex);
            }
        }
    }
}