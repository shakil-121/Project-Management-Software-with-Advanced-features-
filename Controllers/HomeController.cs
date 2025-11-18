using FastPMS.Data;
using FastPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FastPMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        

        private readonly ILogger<HomeController> _logger;
        private readonly PmsDbContext pmsDbContext;

        public HomeController(ILogger<HomeController> logger, PmsDbContext pmsDbContext)
        {
            _logger = logger;
            this.pmsDbContext = pmsDbContext;
        }

        public IActionResult Index()
        {
            var projects = pmsDbContext.Projects.ToList();

            var notStartedCount = projects.Count(p => p.Status.Trim().ToLower() == "not started");
            var inProgressCount = projects.Count(p => p.Status.Trim().ToLower() == "in progress");
            var onHoldCount = projects.Count(p => p.Status.Trim().ToLower() == "on hold");
            var CompletedCount = projects.Count(p => p.Status == "Completed");

            // Total count of all projects
            var totalProjectsCount = projects.Count();

            Console.WriteLine($"Completed Projects Count: {CompletedCount}");


            // Pass the counts to the view using ViewBag
            ViewBag.NotStartedCount = notStartedCount;
            ViewBag.InProgressCount = inProgressCount;
            ViewBag.OnHoldCount = onHoldCount;
            ViewBag.CompletedCount = CompletedCount;
            ViewBag.TotalProjectsCount = totalProjectsCount;

            return View();
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
