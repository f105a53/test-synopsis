using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Server.Models;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRestClient _client;

        public HomeController()
        {
            _client = new RestClient("http://search-api/api").UseSerializer(() => new JsonNetSerializer());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string searchQuery)
        {
            return RedirectToAction(nameof(Search), new {q = searchQuery});
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var r = new RestRequest("search", Method.GET, DataFormat.Json);
            r.AddQueryParameter("q", q);
            var results = await _client.GetAsync<SearchResults>(r);
            return View(results);
        }
    }
}
