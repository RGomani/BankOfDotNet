using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BankOfDotNet.NewMvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace BankOfDotNet.NewMvcClient.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

        // By using the Authorized attribute, we are securing the "Secure" action
        // by our authenticating authority which is IdentityServer4 which we configured
        // in the Startup.cs
        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        // This will handle the log-out
        public async Task Logout()
        {
            // Signout the Cookies as an approach to authenticating the user (configured in Startup.cs)
            await HttpContext.SignOutAsync("Cookies");
            // Signout the Open-ID Connect
            await HttpContext.SignOutAsync("oidc");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
