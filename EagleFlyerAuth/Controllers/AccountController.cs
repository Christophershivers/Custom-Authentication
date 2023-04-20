using EagleFlyerAuth.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EagleFlyerAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly Context _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(Context Context, IWebHostEnvironment webHostEnvironment)
        {
            _Context = Context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel lm)
        {
            bool userExist = _Context.Users.Any(u => u.Email == lm.Email && u.Password == lm.Password);
            SignupModel signup = _Context.Users.FirstOrDefault(u => u.Email == lm.Email && u.Password == lm.Password);
            
            if (!userExist)
            {
                ModelState.AddModelError("", "wrong password or email");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, signup.ID),
                new Claim(ClaimTypes.Name, signup.UserName),
                new Claim(ClaimValueTypes.Email, signup.Email),
                new Claim("ProfileImage", signup.ProfileImage)

            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync( CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Signup()
        {
            ViewBag.Exist = false;
            return View();
        }

        [HttpPost]
        public IActionResult Signup(SignupModel sm)
        {
            ViewBag.Exist = false;
            var checkIfUserExist = _Context.Users.Any(u => u.UserName == sm.UserName);
            if (checkIfUserExist)
            {
                ModelState.AddModelError(sm.UserName, "this user already exist");
                ViewBag.Exist = true;
                return View();
            }
           
            
           
            _Context.Users.Add(sm);
            _Context.SaveChanges();
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


        public IActionResult Settings()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var searchSignedInUser = _Context.Users.Where(x => x.ID == id).FirstOrDefault();
            

           


            return View(searchSignedInUser);
        }


        [HttpPost]
        public async Task<IActionResult> Settings(SignupModel sum)
        {
            //var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var searchSignedInUser = _Context.Users.Where(x => x.ID == id).FirstOrDefault();
            
            if (sum.ProfileImageUpload != null)
            {

                string folder = "Users/" + sum.ID + "/ProfileImage/";
                string rootFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                string subPath = rootFolder;

                Directory.CreateDirectory(subPath);

                folder += Guid.NewGuid().ToString() + "_" + sum.ProfileImageUpload.FileName;
                rootFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                sum.ProfileImage = "/" + folder;
                await sum.ProfileImageUpload.CopyToAsync(new FileStream(rootFolder, FileMode.Create));

                // Update the ProfileImage claim with the new image URL
                var identity = (ClaimsIdentity)User.Identity;
                identity.RemoveClaim(identity.FindFirst("ProfileImage"));
                identity.AddClaim(new Claim("ProfileImage", sum.ProfileImage));
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            }

            _Context.Update(sum);
            _Context.SaveChanges();


            return View();
        }
    }
}
