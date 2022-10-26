using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CollectionsApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;


namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class LikesController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public LikesController(ApplicationContext db, 
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult ShowLikes(int itemId)
        {
            bool isCurrentUserLikedItem = _db.Likes.Where(like => (like.ItemId == itemId && like.UserId.Equals(_userManager.GetUserId(this.User)))).Count() != 0;
            int likesNumber = _db.Likes.Where(like => like.ItemId == itemId).Count();
            return Json(new { 
                isCurrentUserLikedItem = isCurrentUserLikedItem, 
                likesNumber = likesNumber
            });
        }


        [HttpGet]
        public IActionResult AddLike(int itemId)
        {
            Like likeToAdd = new Like { UserId = _userManager.GetUserId(this.User), ItemId = itemId };
            _db.Likes.Add(likeToAdd);
            _db.SaveChanges();
            return RedirectToAction("ShowLikes", new { itemId = itemId });
        }


        [HttpGet]
        public IActionResult RemoveLike(int itemId)
        {
            var likes  = _db.Likes.Where(like => like.UserId.Equals(_userManager.GetUserId(this.User)) 
                        && like.ItemId == itemId).ToList();
            foreach(var like in likes)
            {
                _db.Likes.Remove(like);
            }
            _db.SaveChanges();
            return RedirectToAction("ShowLikes", new { itemId = itemId });
        }
    }
}
