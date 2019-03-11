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
        private readonly LinkedList<RestClient> clients = new LinkedList<RestClient>();
        private LinkedListNode<RestClient> current;

        public SearchController()
        {
            clients.AddLast(new RestClient("http://localhost:5000/api"));
            clients.AddLast(new RestClient("https://localhost:5001/api"));
            current = clients.First;
        }

        [HttpGet]
        public async Task<List<TermDoc>> Search([FromQuery] string q)
        {
            //Use Round-robin to choose next server
            current = current.Next ?? current.List.First;
            var client = current;
            //Report request
            Metrics.Increment("totalRequests", 1,
                new Dictionary<string, string> {{"server", client.Value.BaseUrl.ToString()}});

            var r = new RestRequest("search", Method.GET, DataFormat.Json);
            r.AddQueryParameter("q", q);
            IRestResponse<List<TermDoc>> result;
            //Report request duration
            using (Metrics.Time("serverResponseTime",
                new Dictionary<string, string> {{"server", client.Value.BaseUrl.ToString()}}))
            {
                result = await client.Value.ExecuteTaskAsync<List<TermDoc>>(r);
            }

            return result.Data;
        }
    }
}