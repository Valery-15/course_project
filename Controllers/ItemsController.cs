using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public ItemsController(UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._options = BuildOptions();
        }

        private DbContextOptions<ApplicationContext> BuildOptions()
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

        public async Task<IActionResult> Item(int itemId)
        {
            using ApplicationContext db = new ApplicationContext(this._options);
            Item item = await db.Items.FindAsync(itemId);
            var currentUserId = _userManager.GetUserId(this.User);
            var model = new ItemViewModel(item, currentUserId);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateItem(int collectionId)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collection = db.Collections.Find(collectionId);
            CollectionField[] collectionFields = { };
            if (collection != null)
            {
                if (collection.AdditionalFields != null)
                {
                    collectionFields = JsonSerializer.Deserialize<CollectionField[]>(collection.AdditionalFields);
                }
            }
            CreateItemViewModel model = new CreateItemViewModel();
            var itemFields = new List<ItemField>();
            foreach(var collectionField in collectionFields)
            {
                itemFields.Add(new ItemField(collectionField));
            }
            model.ItemFields = itemFields;
            ViewBag.collectionId = collectionId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateItem(int collectionId, CreateItemViewModel model, ItemField[] itemFields)
        {
            if (!ModelState.IsValid)
            {
                //установить значения
                ViewBag.collectionId = collectionId;
                return View(model);
            }
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collection = db.Collections.Find(collectionId);
            if (collection != null)
            {

                Item itemToAdd = new Item();
                itemToAdd.Title = model.Title;
                itemToAdd.Tags = model.Tags;
                itemToAdd.AddDate = DateTime.Now;
                itemToAdd.CollectionId = collectionId;
                itemToAdd.AdditionalFields = JsonSerializer.Serialize(itemFields);


                db.Items.Add(itemToAdd);
                db.SaveChanges();
                return RedirectToAction("ItemsList", new { collectionId = collectionId});
            }
            ViewBag.collectionId = collectionId;
            return View(model);
        }

        [HttpGet]
        public IActionResult EditItem(int collectionId, int itemId)
        {
            CreateItemViewModel model = new CreateItemViewModel();
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collection = db.Collections.Find(collectionId);
            Item item = db.Items.Find(itemId);
            List<ItemField> list = new List<ItemField>();
            //list.Add(new ItemField { Title = collection.IntegerField1, Type = "number", Value = item.IntegerField1.ToString() });
            //list.Add(new ItemField { Title = collection.IntegerField2, Type = "number", Value = item.IntegerField2.ToString() });
            //list.Add(new ItemField { Title = collection.IntegerField3, Type = "number", Value = item.IntegerField3.ToString() });
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
            if (itemToDelete != null)
            {
                db.Items.Remove(itemToDelete);
                db.SaveChanges();
            }
            return RedirectToAction("ItemsList",new { collectionId = itemToDelete.CollectionId });
        }

        [HttpGet]
        public JsonResult GetItemsList(int collectionId)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            var collectionItems = db.Items.Where(item => item.CollectionId.Equals(collectionId)).ToList();
            return Json(collectionItems);
        }



        private Item MakeItem(Collection collection, HttpRequest request)
        {
            Item item = new Item();
            //try
            //{
            //    item.IntegerField1 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField1)).Value);
            //    item.IntegerField2 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField2)).Value);
            //    item.IntegerField3 = Int32.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.IntegerField3)).Value);

            //    item.StringField1 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.StringField1)).Value;
            //    item.StringField2 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.StringField2)).Value;
            //    item.StringField3 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.StringField3)).Value;

            //    item.MultiStringField1 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.MultiStringField1)).Value;
            //    item.MultiStringField2 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.MultiStringField2)).Value;
            //    item.MultiStringField3 = request.Form.FirstOrDefault(p => p.Key.Equals(collection.MultiStringField3)).Value;

            //    item.BoolField1 = Boolean.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.BoolField1)).Value);
            //    item.BoolField2 = Boolean.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.BoolField2)).Value);
            //    item.BoolField3 = Boolean.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.BoolField3)).Value);

            //    item.DateField1 = DateTime.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.DateField1)).Value);
            //    item.DateField2 = DateTime.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.DateField2)).Value);
            //    item.DateField3 = DateTime.Parse(request.Form.FirstOrDefault(p => p.Key.Equals(collection.DateField3)).Value);
            //} catch(FormatException)
            //{

            //} catch(ArgumentNullException)
            //{

            //}
            return item;
        }
    }
}
