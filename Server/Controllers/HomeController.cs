using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Server.Models;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly RestClient client;

        public HomeController()
        {
            client = new RestClient("http://load-balancer/api");
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
            var restResponse = await client.ExecuteTaskAsync<List<TermDoc>>(r);
            return View(restResponse.Data);
        }
    }
}