using System;
using Nest;

namespace ElasticsearchUI.Configuration.Model
{
    public class Person
    {
        public string id { get; set; }
        
        [Keyword]
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string gender { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}