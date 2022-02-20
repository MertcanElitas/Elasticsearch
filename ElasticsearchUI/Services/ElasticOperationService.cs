using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.XPath;
using ElasticsearchUI.Configuration.Connection;
using ElasticsearchUI.Configuration.Model;
using Nest;
using Newtonsoft.Json;

namespace ElasticsearchUI.Services
{
    public class ElasticOperationService : IElasticOperationService
    {
        private readonly  ElasticProvider _elasticProvider;

        public ElasticOperationService (ElasticProvider elasticProvider)
        {
            _elasticProvider = elasticProvider;
        }

        /// <summary>
        /// Insert single data.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InsertData()
        {
            var person = new Person()
            {
                id = "1",
                first_name = "Mertcan",
                last_name = "Elitaş"
            };

            var indexResponse = await _elasticProvider.ElasticClient.IndexDocumentAsync(person);

            return indexResponse.IsValid;
        }

        /// <summary>
        /// 0. itemden 10. itema kadar firstname değeri "Mertcan" olan kayıtları döner.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> GetElasticDataQuery()
        {
            var searchResponse = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .From(0)
                .Size(3000)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.first_name)
                        .Query("Mertcan")
                    )
                )
            );

            var peoples = searchResponse.Documents.ToList();

            return peoples;
        }

        /// <summary>
        /// 0. itemden 10. itema kadar firstname değeri "Mertcan" olan kayıtları döner.Tüm indexlerde arar.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> GetElasticDataQueryAllIndices()
        {
            var searchResponse = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .AllIndices()
                .From(0)
                .Size(3000)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.first_name)
                        .Query("Mertcan")
                    )
                )
            );

            var peoples = searchResponse.Documents.ToList();

            return peoples;
        }

        /// <summary>
        /// Bulk insert data.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InsertBulkData()
        {
            string data1 = System.IO.File.ReadAllText(@"/Users/melitas/CustomProjects/MOCK_DATA (1).json");
            string data2 = System.IO.File.ReadAllText(@"/Users/melitas/CustomProjects/MOCK_DATA (2).json");

            var list1 = JsonConvert.DeserializeObject<List<PersonDbData>>(data1);
            // var list2 = JsonConvert.DeserializeObject<List<PersonDbData>>(data2);
            //
            //  list1.AddRange(list2);

            var startDate = new DateTime(2010, 1, 1, 0, 0, 0);
            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);

            foreach (var item in list1)
            {
                startDate = startDate.AddMonths(1);
                endDate = endDate.AddMonths(1);

                item.StartDate = startDate;
                item.EndDate = endDate;
            }

            var asyncBulkIndexResponse = await _elasticProvider.ElasticClient.BulkAsync(b => b
                .Index("customer")
                .IndexMany(list1)
            );

            return asyncBulkIndexResponse.IsValid;
        }

        /// <summary>
        /// Firstname'i mertcan olan bütün kayıtları çeker ve lastname'e göre gruoplar.
        /// Sorgu dönemedi hata veriyor.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetElasticDataQueryByFirstNameAggregateLastName()
        {
            var searchResponse = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Size(40000)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.first_name)
                        .Query("Mertcan")
                    )
                )
                .Aggregations(a => a
                    .Terms("last_names", ta => ta
                        .Field(f => f.last_name)
                    )
                )
            );

            var termsAggregation = searchResponse.Aggregations.Terms("last_names");

            return true;
        }

        #region " Queries "

        /// <summary>
        /// Herhagi bir filtre olmaksınızn bütün kayıtları döner.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Person> GetAllPersonWithoutQuery()
        {
            var searchResponse = _elasticProvider.ElasticClient.Search<Person>(s => s
                .Query(q => q
                    .MatchAll()
                )
            );

            var result = searchResponse.Documents.ToList();

            return result;
        }

        /// <summary>
        /// Başlangıç targihine göre 1.1.2018-1.1.2011 arasındaki customer indexindeki dataları döner.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Person> GetPersonByStartDate()
        {
            var searchResponse = _elasticProvider.ElasticClient.Search<Person>(s => s
                .Size(100)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.StartDate)
                        .GreaterThanOrEquals(new DateTime(2011, 01, 01))
                        .LessThan(new DateTime(2018, 01, 01))
                    )
                )
                .Index("customer")
            );

            var result = searchResponse.Documents.ToList();

            return result;
        }

        /// <summary>
        /// Firstname değeri Horten olan kaydı döner
        /// </summary>
        /// <returns></returns>
        public async Task<Person> GetPersonByFirstname()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s =>
                s.Size(1)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.first_name)
                            .Query("Horten")))
                    .Index("customer"));

            var list = searchResult.Documents.ToList();

            if (list.Any())
            {
                return list.First();
            }

            return new Person();
        }

        /// <summary>
        /// Firstname değeri Horten ve Lastname değeri Prewett ve emaili hprewett17@unc.edu ve başlangıç tarihi 1.1.2022 de küçük ve 1.1..2021 olan kayıtı döner.
        /// </summary>
        /// <returns></returns>
        public async Task<Person> GetPersonByCombineQuery()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Query(q => q
                    .Bool(b =>
                        b.Must(mu => mu
                                    .Match(ma => ma
                                        .Field(fi => fi.first_name)
                                        .Query("Horten"))
                                , mu => mu
                                    .Match(ma => ma
                                        .Field(fi => fi.last_name)
                                        .Query("Prewett")),
                                mu => mu.Match(ma => ma
                                    .Field(fi => fi.email)
                                    .Query("hprewett17@unc.edu"))
                            )
                            .Filter(fi => fi
                                .DateRange(dr => dr.Field(fi => fi.StartDate)
                                    .GreaterThanOrEquals(new DateTime(2013, 1, 1))
                                    .LessThan(new DateTime(2022, 1, 1))))
                    )).Index("customer"));

            var list = searchResult.Documents.ToList();

            if (list.Any())
            {
                return list.First();
            }

            return new Person();
        }

        /// <summary>
        /// Firstname değeri Horten ve Lastname değeri Prewett ve emaili hprewett17@unc.edu ve başlangıç tarihi 1.1.2022 de küçük ve 1.1..2021 olan kayıtı döner.
        /// GetPersonByCombineQuery metodunun farklı bir syntax ile yazımı.
        /// </summary>
        /// <returns></returns>
        public async Task<Person> GetPersonByCombineQueryV2()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Query(q => q
                                .Match(m => m
                                    .Field(fi => fi.first_name)
                                    .Query("Horten")) &&
                            q.Match(ma => ma
                                .Field(fi => fi.last_name)
                                .Query("Prewett")) &&
                            q.Match(ma => ma
                                .Field(fi => fi.email)
                                .Query("hprewett17@unc.edu")) &&
                            q.DateRange(ma => ma
                                .GreaterThanOrEquals(new DateTime(2013, 1, 1))
                                .LessThan(new DateTime(2022, 1, 1))))
                .Index("customer"));

            var list = searchResult.Documents.ToList();

            if (list.Any())
            {
                return list.First();
            }

            return new Person();
        }

        /// <summary>
        /// Firstname değeri Horten veya Lastname değeri Prewett olan kayıtı döner.
        /// Term queryleri kullanırken dikkat açıklaması Notes.txt içerisinde.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> GetPersonByCombineQueryWithTerm()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            bs => bs.Term(p => p.first_name, "horten"),
                            bs => bs.Term(p => p.last_name, "prewett")
                        )
                    )
                )
                .Index("customer")
            );

            var result = searchResult.Documents.ToList();

            return result;
        }
        
        /// <summary>
        /// Firstname değeri Horten ve Lastname değeri Prewett ve emaili hprewett17@unc.edu  olan kayıtı döner.
        /// Term queryleri kullanırken dikkat açıklaması Notes.txt içerisinde.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> GetPersonByCombineQueryWithTermAndOperator()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Query(q => q
                        .Term(p => p.first_name, "horten") && q
                        .Term(p => p.last_name, "prewett")
                )
                .Index("customer")
            );

            var result = searchResult.Documents.ToList();

            return result;
        }
        
        /// <summary>
        /// Firstname değeri Horten olmayan kayıtları döner.
        /// Term queryleri kullanırken dikkat açıklaması Notes.txt içerisinde.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> GetPersonByTermUnary()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Size(500)
                .Query(q => !q
                        .Term(p => p.first_name, "horten") 
                )
                .Index("customer")
            );

            var result = searchResult.Documents.ToList();

            return result;
        }

        /// <summary>
        /// Elasticsearch üzerinde belli alanları select etmek istediğimizde StoredFields kullanılır. Bu kod bloğu değer dönmez çünkü bu şekilde kullanılabilmesi içine
        /// verilerin "stored" olarak depolanması gerekir bu özellik ise default olarak kapalıdr.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> ElastisSearchSelectExample()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>
            (s => s
                .StoredFields(sf=>sf.Fields(
                    fi=>fi.first_name))
                .Query(q => q.MatchAll())
                .Index("customer"));

            var result = await  _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .DocValueFields(f => f.Field(ff => ff.first_name.Suffix("keyword"))).Index("customer"));
            
            // var result = searchResult.Hits.Select(x=>x.Score.Value).ToList();

            return null;
        }
        
        /// <summary>
        /// Elasticsearch üzerinde belli alanları select etmek istediğimizde StoredFields kullanılır. Ancak stored field kullabilmek için alanların "stored"
        /// olarak işaretlenmesi gereklidir. Bu özellik varsayılan olarak kapalıdır. Bunun yerinde aşağıdaki yöntem kullanılır.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>> ElastisSearchSelectExampleWithoutStoredFields()
        {
            var searchResult = await _elasticProvider.ElasticClient.SearchAsync<Person>
            (s => s
                .Query(q => q.MatchAll())
                .Source(src=>src.IncludeAll()
                    .Excludes(e=>e
                        .Field(ff=>ff.last_name)
                        .Field(ff=>ff.first_name)))
                .Index("customer"));
            
             var result = searchResult.Documents.ToList();

            return null;
        }
        

        /// <summary>
        /// Scrooling document. 
        /// </summary>
        public async Task<List<Person>> GetPersonListWithScroll()
        {
            var searchResponse = await  _elasticProvider.ElasticClient.SearchAsync<Person>(s => s
                .Query(q => q.MatchAll()
                ).Index("customer").Scroll("10s"));

            var result = new List<Person>();
            
            while (searchResponse.Documents.Any())
            {
                result.AddRange(searchResponse.Documents.ToList());
                searchResponse = _elasticProvider.ElasticClient.Scroll<Person>("10s", searchResponse.ScrollId);
            }
            
            return result;
        }
        
        #endregion

        public void UpdateAllDataStartDateandEndDate()
        {
            var searchResponse = _elasticProvider.ElasticClient.Search<Person>(s => s
                .From(0)
                .Size(2000)
                .Query(q => q
                    .MatchAll()
                )
            );

            var list = searchResponse.Documents.ToList();

            var startDate = new DateTime(2010, 1, 1, 0, 0, 0);
            var endDate = new DateTime(2018, 1, 1, 0, 0, 0);

            foreach (var item in list)
            {
                startDate = startDate.AddHours(1);
                endDate = endDate.AddHours(1);

                item.StartDate = startDate;
                item.EndDate = endDate;
            }

            _elasticProvider.ElasticClient.IndexMany(list, "customer");
        }

        public async Task InsertSameFirstNameData()
        {
            var list = new List<PersonDbData>
            {
                new PersonDbData()
                {
                    id = "2001",
                    first_name = "Mertcan",
                    last_name = "elitas",
                    email = "mertcan@gmail.com"
                },
                new PersonDbData()
                {
                    id = "2002",
                    first_name = "Mertcan",
                    last_name = "yılmaz",
                    email = "mertcan@gmail.com"
                },
                new PersonDbData()
                {
                    id = "2003",
                    first_name = "Mertcan",
                    last_name = "kaya",
                    email = "mertcan@gmail.com"
                },
                new PersonDbData()
                {
                    id = "2004",
                    first_name = "Mertcan",
                    last_name = "elitas",
                    email = "mertcan@gmail.com"
                },
                new PersonDbData()
                {
                    id = "2005",
                    first_name = "Mertcan",
                    last_name = "yılmaz",
                    email = "mertcan@gmail.com"
                },
                new PersonDbData()
                {
                    id = "2006",
                    first_name = "Mertcan",
                    last_name = "tes",
                    email = "mertcan@gmail.com"
                },
            };

            var result = await _elasticProvider.ElasticClient.IndexManyAsync(list);
        }
    }
}