using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data;
using InfluxDB.Collector;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace LoadBalancer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly string[] servers = {"http://serach-api1/api", "http://search-api2/api"};
        private readonly RestClient current;

        public SearchController()
        {
            //Randomly choose a server
            var i = new Random().Next(0, servers.Length);
            current = new RestClient(servers[i]);
        }

        [HttpGet]
        public async Task<List<TermDoc>> Search([FromQuery] string q)
        {
            var restClient = current;
            //Report request
            Metrics.Increment("totalRequests", 1,
                new Dictionary<string, string> {{"server", restClient.BaseUrl.ToString()}});

            var r = new RestRequest("search", Method.GET, DataFormat.Json);
            r.AddQueryParameter("q", q);
            IRestResponse<List<TermDoc>> result;
            //Report request duration
            using (Metrics.Time("serverResponseTime",
                new Dictionary<string, string> {{"server", restClient.BaseUrl.ToString()}}))
            {
                result = await restClient.ExecuteTaskAsync<List<TermDoc>>(r);
            }

            return result.Data;
        }
    }
}