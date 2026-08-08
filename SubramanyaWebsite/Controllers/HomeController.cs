using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SubramanyaWebsite.Models;
using SubramanyaWebsite.Services;

namespace SubramanyaWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
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
        public async Task<IActionResult> BookCare(CareRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Please check the form details and try again." });
            }

            try
            {
                await _emailService.SendCareRequestEmailsAsync(request, cancellationToken);
                return Json(new
                {
                    success = true,
                    message = $"Thank you {request.Name}! Your request has been sent. " +
                              "Our care coordinator will contact you within 15 minutes."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Care request email delivery failed.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "We could not send your request right now. Please call +91 96325 54468 or +91 97392 113907."
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
