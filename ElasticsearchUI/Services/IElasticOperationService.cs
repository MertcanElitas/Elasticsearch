using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticsearchUI.Configuration.Model;

namespace ElasticsearchUI.Services
{
    public interface IElasticOperationService
    {
        Task<bool> InsertData();

        Task<List<Person>> GetElasticDataQuery();
        
        Task<bool> InsertBulkData();

        Task<bool> GetElasticDataQueryByFirstNameAggregateLastName();

        IEnumerable<Person> GetPersonByStartDate();
        Task InsertSameFirstNameData();

        Task<Person> GetPersonByFirstname();

        Task<List<Person>> ElastisSearchSelectExample();
        
        Task<List<Person>>  ElastisSearchSelectExampleWithoutStoredFields();
        
        Task<List<Person>> GetPersonListWithScroll();
        
        Task<Person> GetPersonByCombineQuery();

        Task<Person> GetPersonByCombineQueryV2();

        Task<List<Person>> GetPersonByCombineQueryWithTerm();

        Task<List<Person>> GetPersonByCombineQueryWithTermAndOperator();

        Task<List<Person>> GetPersonByTermUnary();
        
        void UpdateAllDataStartDateandEndDate();
    }
}