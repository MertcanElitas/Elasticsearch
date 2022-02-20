using System.Threading.Tasks;
using ElasticsearchUI.Configuration.Connection;
using ElasticsearchUI.Configuration.Model;
using ElasticsearchUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticController : Controller
    {
        private IElasticOperationService _elasticOperationService;

        public ElasticController (IElasticOperationService elasticOperationService)
        {
            this._elasticOperationService = elasticOperationService;
        }

        // GET
        [HttpGet("Execute")]
        public async Task<IActionResult> Execute()
        {
            //var result =await _elasticOperationService.InsertData();
            //var result =await _elasticOperationService.InsertBulkData();
            //var data= _elasticOperationService.GetPersonByStartDate();
            //var result =await _elasticOperationService.GetElasticDataQuery();
            //await _elasticOperationService.InsertSameFirstNameData();
            //var result = await _elasticOperationService.GetElasticDataQueryByFirstNameAggregateLastName();
            //var result = await _elasticOperationService.GetPersonByFirstname();
            //var result = await _elasticOperationService.GetPersonByCombineQuery();
            //var result = await _elasticOperationService.GetPersonByCombineQueryV2();
            //var result = await _elasticOperationService.GetPersonByCombineQueryWithTerm();
            //var result = await _elasticOperationService.GetPersonByCombineQueryWithTermAndOperator();
            //var result = await _elasticOperationService.GetPersonByTermUnary();
            //var result = await _elasticOperationService.ElastisSearchSelectExample();
            //var result = await _elasticOperationService.GetPersonListWithScroll();
            var result = await _elasticOperationService.ElastisSearchSelectExampleWithoutStoredFields();

            return Ok(result);
        }
    }
}