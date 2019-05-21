using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly SearchService _searchService;

        public HomeController(SearchService searchService)
        {
            _searchService = searchService;
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
            var searchResults = await _searchService.Search(q);
            return View(searchResults);
        }
    }
}