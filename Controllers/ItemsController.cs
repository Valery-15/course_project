using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
    public class ItemsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public ItemsController(UserManager<User> userManager)
        {
            this._userManager = userManager;
            this._options = buildOptions();
        }

        private DbContextOptions<ApplicationContext> buildOptions()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            string ConnectionString = configuration.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            return optionsBuilder
                    .UseSqlServer(ConnectionString)
                    .Options;
        }

        public IActionResult ItemsList(int collectionId)
        {
            ViewBag.collectionId = collectionId;
            return View();
        }

        [HttpGet]
        public IActionResult CreateItem(int collectionId)
        {
            CreateItemViewModel model = new CreateItemViewModel();
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collection = db.Collections.Find(collectionId);
            if(collection != null)
            {
                model.Fields = collection.GetItemFields();
            }
            ViewBag.collectionId = collectionId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateItem(int collectionId, CreateItemViewModel model)
        {

            if (ModelState.IsValid)
            {
                
            }
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collection = db.Collections.Find(collectionId);
            if (collection != null)
            {
                Item itemToAdd = MakeItem(collection, Request);
                itemToAdd.Title = model.Title;
                itemToAdd.Tags = model.Tags;
                itemToAdd.AddDate = DateTime.Now;
                itemToAdd.CollectionId = collectionId;
                db.Items.Add(itemToAdd);
                db.SaveChanges();
                return RedirectToAction("ItemsList", new { collectionId = collectionId});
                model.Fields = collection.GetItemFields();
            }
            ViewBag.collectionId = collectionId;
            return View(model);
        }


        [HttpGet]
        public IActionResult AddLike(string userId, int itemId, string returnUrl)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            Like like = new Like { UserId = userId, ItemId = itemId };
            db.Likes.Add(like);
            db.SaveChanges();
            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult RemoveLike(int likeId, string returnUrl)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            Like likeToRemove = db.Likes.Find(likeId);
            if (likeToRemove != null)
            {
                db.Likes.Remove(likeToRemove);
                db.SaveChanges();
            }
            return Redirect(returnUrl);
        }



        [HttpGet]
        public IActionResult DeleteItem(int itemId)
        {
            ApplicationContext db = new ApplicationContext(_options);
            Item itemToDelete = db.Items.Find(itemId);
            db.Items.Remove(itemToDelete);
            db.SaveChanges();
            return RedirectToAction("ItemsList",new { collectionId = itemToDelete.CollectionId });
        }

        [HttpGet]
        public JsonResult GetItemsList(int collectionId)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            var collectionItems = db.Items.Where(item => item.CollectionId.Equals(collectionId)).ToList();
            return Json(collectionItems);
        }



        public Item MakeItem(Collection collection, HttpRequest request)
        {
            Item item = new Item();
            try
            {
                item.IntegerField1 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField1)).Value);
                item.IntegerField2 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField2)).Value);
                item.IntegerField3 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField3)).Value);
            } catch(FormatException e)
            {

            }
            return item;

        }
    }
}
