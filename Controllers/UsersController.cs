using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CollectionsApp.ViewModels;

namespace CoollectionsApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public UsersController(UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _options = BuildOptions();
        }

        private DbContextOptions<ApplicationContext> BuildOptions()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string ConnectionString = config.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            return optionsBuilder.UseSqlServer(ConnectionString).Options;
        }

        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.UserName, EmailConfirmed = true, Status = "active" };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddModelErrors(result.Errors, ModelState);
                }
            }
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                EditUserViewModel userEditViewModel = new EditUserViewModel { Email = user.Email, UserName = user.UserName, Status = user.Status };
                ViewBag.UserId = userId;
                return View(userEditViewModel);
            } 
            else
            {
                return RedirectToAction("Index","Users");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string userId)
        {
            User userToEdit = await _userManager.FindByIdAsync(userId);
            if (userToEdit != null)
            {
                userToEdit.Email = model.Email;
                userToEdit.UserName = model.UserName;
                userToEdit.Status = model.Status;
                var result = await _userManager.UpdateAsync(userToEdit);
            }
            
            return RedirectToAction("Index","Users");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUserStatus(string userId, string currentStatus)
        {
            string changedStatus = currentStatus.Equals("active") ? "blocked" : "active";
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Status = changedStatus;
                var result = await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<JsonResult> GetUsersList()
        {
            using ApplicationContext db = new ApplicationContext(_options);
            var usersTableList = db.Users.ToList();
            var usersList = new List<UsersTableViewModel>();
            foreach(var user in usersTableList)
            {
                usersList.Add(new UsersTableViewModel { Email = user.Email, UserName = user.UserName, IsAdmin = await _userManager.IsInRoleAsync(user, "admin"), Status = user.Status });
            }
            return Json(usersList);
        }

        private void AddModelErrors(IEnumerable<IdentityError> errors, ModelStateDictionary modelState)
        {
            foreach (var error in errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
