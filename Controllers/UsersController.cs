using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.ViewModels;

namespace CoollectionsApp.Controllers
{
    [Authorize(Roles = "admin")]
    [Authorize(Roles = "active user")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationContext _db;

        public UsersController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ApplicationContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [HttpGet]
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
            var userEditViewModel = new EditUserViewModel { 
                Email = user.Email, 
                UserName = user.UserName, 
                IsAdmin = await _userManager.IsInRoleAsync(user, "admin"), 
                IsActive = await _userManager.IsInRoleAsync(user, "active user") 
            };
            ViewBag.UserId = userId;
            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string userId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId;
                return View(model);
            }
            IdentityUser userToEdit = await _userManager.FindByIdAsync(userId);
            if(userToEdit == null) return RedirectToAction("UsersList");

            var result = await UpdateUserInDb(userToEdit, model);
            if (result.Succeeded)
            {
                await RefreshCurrentUserAppCookie();
                return RedirectToAction("UsersList");
            }
            else
            {
                AddModelErrors(result.Errors, ModelState);
                ViewBag.UserId = userId;
                return View(model);
            }
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
                await _userManager.DeleteAsync(user);
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
                await ManageUserRoles(user, "active user", isActive);
                await RefreshCurrentUserAppCookie();
            }
            return RedirectToAction("UsersList");
        }


        [HttpGet]
        public async Task<JsonResult> GetUsersList()
        {
            var usersTableList = _db.Users.ToList();
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

        private async Task<IdentityResult> UpdateUserInDb(IdentityUser userToEdit, EditUserViewModel model)
        {
            userToEdit.Email = model.Email;
            userToEdit.UserName = model.UserName;
            await ManageUserRoles(userToEdit, "active user", model.IsActive);
            await ManageUserRoles(userToEdit, "admin", model.IsAdmin);
            return await _userManager.UpdateAsync(userToEdit);
        }

        private async Task ManageUserRoles(IdentityUser user, string role, bool isInRole)
        {
            if (isInRole)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
        }

        private async Task RefreshCurrentUserAppCookie()
        {
            var currentUser = await _userManager.GetUserAsync(this.User);
            await _signInManager.RefreshSignInAsync(currentUser);
        }
    }
}
