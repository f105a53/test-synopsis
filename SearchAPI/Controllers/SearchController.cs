using System.Collections.Generic;
using Common.Data;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

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
        public List<TermDoc> Search([FromQuery] string q)
        {
            return searchService.GetResults(q);
        }
    }
}