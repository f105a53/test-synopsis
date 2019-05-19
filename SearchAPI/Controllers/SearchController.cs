using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Lucene.Net.Search;

namespace SearchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService searchService;

        public SearchController(SearchService searchService)
        {
            this.searchService = searchService;
        }

        [HttpGet]
        public TopDocs Search([FromQuery] string q)
        {
            return searchService.GetResults(q);
        }
    }
}