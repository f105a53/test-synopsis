using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PublicAPI.Models;
using PublicAPI.Services;
using Email = PublicAPI.Models.Email;

namespace PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        // GET: api/Search/query
        [HttpGet("{query}", Name = "Get")]
        public async Task<SearchResults<Email>> Get(string query)
        {
            var searchResults = await _searchService.Search(query);

            return searchResults;
        }
    }
}
