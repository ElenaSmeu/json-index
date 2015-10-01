﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Newtonsoft.Json.Linq;
using Version = Lucene.Net.Util.Version;

namespace DotJEM.Json.Index
{
    public interface ILuceneWriter
    {
        void Write(JObject entity);
        void WriteAll(IEnumerable<JObject> entities);
        void Delete(JObject entity);
        void DeleteAll(IEnumerable<JObject> entities);
        void Optimize();
    }

    internal class LuceneWriter : ILuceneWriter
    {
        private readonly IDocumentFactory factory;
        private readonly LuceneStorageIndex index;

        public LuceneWriter(LuceneStorageIndex index) 
            : this(index, new LuceneDocumentFactory(index))
        {
        }

        public LuceneWriter(LuceneStorageIndex index, IDocumentFactory factory)
        {
            this.index = index;
            this.factory = factory;
        }

        public void Write(JObject entity)
        {
            IndexWriter writer = index.Storage.GetWriter(index.Analyzer);
            writer.UpdateDocument(CreateIdentityTerm(entity), factory.Create(entity));
            writer.Commit();
        }

        public void WriteAll(IEnumerable<JObject> entities)
        {
            IndexWriter writer = index.Storage.GetWriter(index.Analyzer);
            foreach (JObject entity in entities)
            {
                try
                {
                    Term term = CreateIdentityTerm(entity);
                    Document document = factory.Create(entity);
                    writer.UpdateDocument(term, document);
                }
                catch (Exception)
                {
                    //ignore
                }

            }
            writer.Commit();
        }

        public void Delete(JObject entity)
        {
            IndexWriter writer = index.Storage.GetWriter(index.Analyzer);
            writer.DeleteDocuments(CreateIdentityTerm(entity));
            writer.Commit();
        }

        public void DeleteAll(IEnumerable<JObject> entities)
        {
            IndexWriter writer = index.Storage.GetWriter(index.Analyzer);
            writer.DeleteDocuments(entities.Select(CreateIdentityTerm).Where(x => x != null).ToArray());
            writer.Commit();
        }

        public void Optimize()
        {
            index.Storage.GetWriter(index.Analyzer).Optimize();
        }

        private Term CreateIdentityTerm(JObject entity)
        {
            try
            {
                return index.Configuration.IdentityResolver.CreateTerm(entity);
            }
            catch (Exception)
            {
                //ignore
                return null;
            }
        }
    }
}
