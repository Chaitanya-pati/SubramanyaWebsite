using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SubramanyaWebsite.Models;
using SubramanyaWebsite.Services;

namespace SubramanyaWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailQueue _emailQueue;

        public HomeController(ILogger<HomeController> logger, IEmailQueue emailQueue)
        {
            _logger = logger;
            _emailQueue = emailQueue;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookCare(CareRequest request)
        {
            // Silently accept spam submissions so bots do not learn that the
            // honeypot exposed them, but never put them on the email queue.
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                return Json(new
                {
                    success = true,
                    message = "Thank you. Your request has been received."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Please check the form details and try again." });
            }

            if (!_emailQueue.TryEnqueue(request))
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "We could not accept your request right now. Please call +91 96325 54468 or +91 97392 113907."
                });
            }

            _logger.LogInformation(
                "Care request accepted for background email delivery from {RequestEmail}.",
                request.Email);

            return Json(new
            {
                success = true,
                message = $"Thank you {request.Name}! Your request has been received. " +
                          "Our care coordinator will contact you within 15 minutes."
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
