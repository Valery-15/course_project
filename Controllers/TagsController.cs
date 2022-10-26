using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CollectionsApp.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace CollectionsApp.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public TagsController(ApplicationContext db,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        
        [HttpGet]
        public JsonResult GetTagsList()
        {
            var tags = _db.Tags.ToList();
            var tagsList = new List<string>();
            foreach(var tag in tags)
            {
                tagsList.Add(tag.TagValue);
            }
            return Json(tagsList);
        }

        [HttpGet]
        public IActionResult GetItemsByTag(int tagId)
        {
            var itemTagsList = _db.ItemTags.Include(itemTag => itemTag.Item).Where(itemTag => itemTag.TagId == tagId).ToList();
            var itemsList = new List<Item>();
            foreach(var itemTag in itemTagsList)
            {
                itemsList.Add(itemTag.Item);
            }
            ViewBag.query = _db.Tags.Find(tagId).TagValue;
            return View("~/Views/SearchItems/SearchResult.cshtml", new SearchResultViewModel { Items = itemsList });
        }
    }
}
