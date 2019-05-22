using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {

        // GET: api/Search/query
        [HttpGet("{query}", Name = "Get")]
        public SearchResults<Email> Get(string query)
        {
            return null;
        }
    }
}
