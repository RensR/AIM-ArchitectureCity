using Microsoft.AspNetCore.Mvc;

namespace AIM.Controllers
{
    /// <summary>
    /// Home controller to handle root requests
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Loads the frontpage
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/> of the homepage
        /// </returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Loads the about page
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/> of the about page
        /// </returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Loads the contact page
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/> of the contact page
        /// </returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Loads the error page
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/> of the error page
        /// </returns>
        public IActionResult Error()
        {
            return View();
        }
    }
}
