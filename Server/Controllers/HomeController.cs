using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly SearchService searchService;

        public HomeController(SearchService searchService) {
            this.searchService = searchService;
        }

        [HttpPost]
        public IActionResult Index([FromForm]string searchQuery) 
{
            return RedirectToAction(nameof(Search), new { q = searchQuery });
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search([FromQuery]string q)
        {
            return View(searchService.GetResults(q));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
