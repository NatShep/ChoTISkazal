using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;
using Chotiskazal.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Chotiskazal.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private UserService _userService;

        public AccountController(UserService userService)
            => _userService = userService;
        
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = _userService.GetUserByLoginOrNull(model.Login);
                if (user != null)
                {
                    await Authenticate(model.Login); // аутентификация
                    return RedirectToAction("Menu", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userService.GetUserByLoginOrNull(model.Login);
                if (user == null)
                {
                    // добавляем пользователя в бд
                    _userService.AddUser(new User(model.Name,model.Login,model.Password,model.Email));
                    await Authenticate(model.Login);
                    return RedirectToAction("Menu", "Home");
                }
                else
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
 
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}