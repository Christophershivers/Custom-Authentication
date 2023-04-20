using EagleFlyerAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EagleFlyerAuth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _Context;

        public HomeController(ILogger<HomeController> logger, Context Context)
        {
            _logger = logger;
            _Context = Context;
        }

        public IActionResult Index()
        {
            
            
            var contextUser = HttpContext.User;
            var userId = contextUser.FindFirst(ClaimTypes.NameIdentifier);

            var users = _Context.Users.ToList();

           
            foreach(var em in users)
            {
                Console.WriteLine("this is it: " + em.FirstName);
                
            }

            

            if(userId != null)
            {
                var user = _Context.Users.FirstOrDefault(u => u.ID == userId.Value);
                return View(user);
            }
            else
            {
                return View();
            }
            
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
