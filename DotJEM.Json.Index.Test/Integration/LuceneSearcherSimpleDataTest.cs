﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index.Configuration;
using DotJEM.Json.Index.Test.Constraints;
using DotJEM.Json.Index.Test.Util;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Json.Index.Test.Integration
{
    [TestFixture]
    public class LuceneSearcherSimpleDataTest
    {
        private readonly IStorageIndex index = new LuceneStorageIndex();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var config = index.Configuration;
            config.SetTypeResolver("Type").SetAreaResolver("Area")
                .ForAll().SetIdentity("Id");

            //config.For("ship").Index("number", As.Long());

            //config.ForAll().Index("Number", As.Term());

            //config.For("Car").Index("Model", As.Default().Analyzed(Field.Index.NOT_ANALYZED))
            //                 .Query("Model", Using.Term().When.Always());

            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000001"), Type = "Person", Name = "John", LastName = "Doe", Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000002"), Type = "Person", Name = "Peter", LastName = "Pan", Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000003"), Type = "Person", Name = "Alice", Area = "Foo" }));

            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000004"), Type = "Car", Brand = "Ford", Model = "Mustang", Number = 5, Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000005"), Type = "Car", Brand = "Dodge", Model = "Charger", Number = 10, Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000006"), Type = "Car", Brand = "Chevrolet", Model = "Camaro", Number = 15, Area = "Foo" }));

            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000007"), Type = "Flower", Name = "Lilly", Meaning = "Majesty", Number = 5, Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000008"), Type = "Flower", Name = "Freesia", Meaning = "Innocence", Number = 10, Area = "Foo" }));
            index.Write(JObject.FromObject(new { Id = new Guid("00000000-0000-0000-0000-000000000009"), Type = "Flower", Name = "Aster", Meaning = "Patience", Number = 15, Area = "Foo" }));
        }

        [Test]
        public void Search_ForMustangWithSpecifiedFields_Returns()
        {
            Query query = new TermQuery(new Term("Number", NumericUtils.LongToPrefixCoded(5)));
            //Query query = NumericRangeQuery.NewLongRange("Number", 5, 5, true, true);

            //List<dynamic> result = index.CreateSearcher().Search("Number:5").Select(hit => hit.Json).ToList();
            List<dynamic> result = index.Searcher.Search(query).Select(hit => hit.Json).ToList();
            Assert.That(result,
                Has.Count.EqualTo(2));
        }

        //[Test]
        //public void Search_ForMustangWithSpecifiedFields_ReturnsCarMustang()
        //{
        //    List<dynamic> result = index.Searcher.Search("Mustang", "Model".Split(',')).Select(hit => hit.Json).ToList();
        //    Assert.That(result,
        //        Has.Count.EqualTo(1) &
        //        Has.Exactly(1).That(HAS.JProperties(JObject.Parse("{ Id: '00000000-0000-0000-0000-000000000004', Type: 'Car', Brand: 'Ford', Model: 'Mustang' }"))));
        //}

        [Test]
        public void Search_ForMustang_ReturnsCarMustang()
        {
            List<dynamic> result = index.Searcher.Search("Mustang").Select(hit => hit.Json).ToList();
            Assert.That(result,
                Has.Count.EqualTo(1) &
                Has.Exactly(1).That(HAS.JProperties(JObject.Parse("{ Id: '00000000-0000-0000-0000-000000000004', Type: 'Car', Brand: 'Ford', Model: 'Mustang' }"))));
        }

        [Test]
        public void Search_ForMustang3_ReturnsCarMustang()
        {
            BooleanQuery query = new BooleanQuery();
            query.Add(new WildcardQuery(new Term("Model", "Mustang*")), Occur.SHOULD);
            query.Add(new FuzzyQuery(new Term("Model", "Mustang")), Occur.SHOULD);

            List<dynamic> result = index.Searcher.Search(query).Select(hit => hit.Json).ToList();
            Assert.That(result,
                Has.Count.EqualTo(1) &
                Has.Exactly(1).That(HAS.JProperties(JObject.Parse("{ Id: '00000000-0000-0000-0000-000000000004', Type: 'Car', Brand: 'Ford', Model: 'Mustang' }"))));
        }
    }
}
