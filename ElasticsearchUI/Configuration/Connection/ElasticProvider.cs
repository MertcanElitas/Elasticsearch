using System;
using Nest;

namespace ElasticsearchUI.Configuration.Connection
{
    public class ElasticProvider
    {
        public ElasticClient ElasticClient { get; private set; }

        public ElasticProvider ()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("people");

            ElasticClient = new ElasticClient(settings);
        }
    }
}