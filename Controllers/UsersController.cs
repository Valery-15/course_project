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
    [Authorize(Roles = "active user")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public UsersController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
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

        public IActionResult UsersList() => View();

        [HttpGet]
        public IActionResult CreateUser() => View();

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            IdentityUser user = new IdentityUser { Email = model.Email, UserName = model.UserName, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "active user");
                return RedirectToAction("UsersList");
            }
            else
            {
                AddModelErrors(result.Errors, ModelState);
                return View(model);
            } 
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("UsersList");
            EditUserViewModel userEditViewModel = new EditUserViewModel { Email = user.Email, UserName = user.UserName, IsAdmin = await _userManager.IsInRoleAsync(user, "admin"), IsActive = await _userManager.IsInRoleAsync(user, "active user") };
            ViewBag.UserId = userId;
            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string userId)
        {
            IdentityUser userToEdit = await _userManager.FindByIdAsync(userId);
            if(userToEdit == null) return RedirectToAction("UsersList");

            userToEdit.Email = model.Email;
            userToEdit.UserName = model.UserName;
            //status
            if (model.IsActive)
            {
                await _userManager.AddToRoleAsync(userToEdit, "active user");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(userToEdit, "active user");
            }
            if (model.IsAdmin)
            {
                await _userManager.AddToRoleAsync(userToEdit, "admin");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(userToEdit, "admin");
            }
            var result = await _userManager.UpdateAsync(userToEdit);

            var currentUser = await _userManager.GetUserAsync(this.User);
            await _signInManager.RefreshSignInAsync(currentUser);
            return RedirectToAction("UsersList");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (userId.Equals(_userManager.GetUserId(this.User))){
                await _signInManager.SignOutAsync();
            }
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                //var currentUser = await _userManager.GetUserAsync(this.User);
                //if (currentUser == null)
                //{
                //    return RedirectToAction("Index", "Home");
                //}
            }
            return RedirectToAction("UsersList");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUserStatus(string userId, string currentStatus)
        {
            bool isActive = !currentStatus.Equals("active");

            IdentityUser user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                if (isActive)
                {
                    await _userManager.AddToRoleAsync(user, "active user");
                } else
                {
                    await _userManager.RemoveFromRoleAsync(user, "active user");
                }
                var currentUser = await _userManager.GetUserAsync(this.User);
                await _signInManager.RefreshSignInAsync(currentUser);
            }
            return RedirectToAction("UsersList");
        }


        [HttpGet]
        public async Task<JsonResult> GetUsersList()
        {
            using ApplicationContext db = new ApplicationContext(_options);
            var usersTableList = db.Users.ToList();
            var usersList = new List<UsersTableViewModel>();
            foreach(var user in usersTableList)
            {
                usersList.Add(new UsersTableViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    IsAdmin = await _userManager.IsInRoleAsync(user, "admin"),
                    Status = (await _userManager.IsInRoleAsync(user, "active user")) ? "active" : "blocked"
                });
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
