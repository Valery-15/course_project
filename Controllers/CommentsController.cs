using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class CommentsController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsController(ApplicationContext db,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public IActionResult AddComment(int itemId, string body)
        {
            string currentUserId = _userManager.GetUserId(this.User);
            var commentToAdd = new Comment(currentUserId, itemId, body);
            _db.Comments.Add(commentToAdd);
            _db.SaveChanges();
            return RedirectToAction("Item", "Items", new { itemId = itemId });
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetCommentsList(int itemId)
        {
            var comments = _db.Comments.Where(comment => comment.ItemId == itemId).Include(comment => comment.User).ToList();
            return Json(comments);
        }
    }
}
