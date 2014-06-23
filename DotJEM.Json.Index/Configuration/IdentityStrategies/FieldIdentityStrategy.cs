﻿using Lucene.Net.Index;
using Newtonsoft.Json.Linq;
using System;

namespace DotJEM.Json.Index.Configuration.IdentityStrategies
{
    public class FieldIdentityStrategy : IIdentityStrategy
    {
        private readonly string field;

        protected string Field { get { return field; } }

        public FieldIdentityStrategy(string field)
        {
            this.field = field;
        }

        public virtual string Resolve(JObject entity)
        {
            return entity[field].Value<string>();
        }

        public virtual Term CreateTerm(JObject entity)
        {
            return new Term(field, Resolve(entity));
        }
    }

    public class GuidIdentity : FieldIdentityStrategy
    {
        public GuidIdentity(string field) 
            : base(field)
        {
        }

        public override string Resolve(JObject entity)
        {
            return entity["_id"].Value<Guid>().ToString();
        }
    }
}