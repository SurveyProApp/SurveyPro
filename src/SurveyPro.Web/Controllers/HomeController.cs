// <copyright file="HomeController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Web.Controllers
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using SurveyPro.Application.Interfaces;
    using SurveyPro.Web.Infrastructure.Filters;
    using SurveyPro.Web.ViewModels;

    public class HomeController : Controller
    {
        private readonly IQuoteService quoteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="quoteService">Quote service.</param>
        public HomeController(IQuoteService quoteService)
        {
            this.quoteService = quoteService;
        }

        [RateLimit(15)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var quote = await this.quoteService.GetQuoteAsync(cancellationToken);
            return View(new HomeViewModel { Quote = quote });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult RateLimitExceeded(int retryAfterSeconds = 60)
        {
            ViewBag.RetryAfterSeconds = retryAfterSeconds;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
