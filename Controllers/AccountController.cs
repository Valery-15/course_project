using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace CollectionsApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            IdentityUser user = new IdentityUser { 
                Email = model.Email, 
                UserName = model.UserName, 
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "active user");
                return RedirectToAction("Login", "Account");
            }
            else
            {
                AddModelErrors(result.Errors, ModelState);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                return await SignInActiveUserByPassword(user, model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User with such an email doesn't exist.");
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private void AddModelErrors(IEnumerable<IdentityError> errors, 
            ModelStateDictionary modelState)
        {
            foreach (var error in errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<IActionResult> SignInActiveUserByPassword(IdentityUser user, LoginViewModel model)
        {
            if (!await _userManager.IsInRoleAsync(user, "active user"))
            {
                return RedirectToAction("AccessDenied");
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("CollectionsList", "Collections", new { collectionsOwnerId = user.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid password.");
                return View(model);
            }
        }
    }
}
