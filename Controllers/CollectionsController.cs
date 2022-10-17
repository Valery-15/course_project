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
using Microsoft.AspNetCore.Authorization;

namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class CollectionsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public CollectionsController(UserManager<User> userManager)
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

        public IActionResult Index(string collectionsOwnerId)
        {
            string currentUserId = _userManager.GetUserId(this.User);
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            ViewBag.currentUserId = currentUserId;
            return View();
        }

        [HttpGet]
        public IActionResult CreateCollection(string collectionsOwnerId)
        {
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection(string collectionsOwnerId, CreateCollectionViewModel model)
        {

            if (ModelState.IsValid)
            {
                if(!isCollectionTitleUnique(collectionsOwnerId, model.Title))
                {
                    ModelState.AddModelError(string.Empty, "Title \"" + model.Title + "\" is already taken.");
                    ViewBag.collectionsOwnerId = collectionsOwnerId;
                    return View(model);
                }

                if (model.ContainsFieldNamesDuplicates())
                {
                    ModelState.AddModelError(string.Empty, "Field names contain duplicates.");
                    ViewBag.collectionsOwnerId = collectionsOwnerId;
                    return View(model);
                }
                
                using ApplicationContext db = new ApplicationContext(this._options);

                Collection collectionToAdd = new Collection
                {
                    UserId = collectionsOwnerId,
                    Title = model.Title,
                    Description = model.Description,
                    Theme = model.Theme,
                    ImageUrl = model.ImageUrl,
                    IntegerField1 = model.IntegerField1,
                    IntegerField2 = model.IntegerField2,
                    IntegerField3 = model.IntegerField3,
                    MultiStringField1 = model.MultiStringField1,
                    MultiStringField2 = model.MultiStringField2,
                    MultiStringField3 = model.MultiStringField3,
                    StringField1 = model.StringField1,
                    StringField2 = model.StringField2,
                    StringField3 = model.StringField3,
                    BoolField1 = model.BoolField1,
                    BoolField2 = model.BoolField2,
                    BoolField3 = model.BoolField3,
                    DateField1 = model.DateField1,
                    DateField2 = model.DateField2,
                    DateField3 = model.DateField3
                };

                await db.Collections.AddAsync(collectionToAdd);
                db.SaveChanges();
                return RedirectToAction("Index", new { collectionsOwnerId = collectionsOwnerId });
            }
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCollection(int collectionId)
        {
            using ApplicationContext db = new ApplicationContext(this._options);
            Collection collectionToEdit = await db.Collections.FindAsync(collectionId);
            ViewBag.CollectionId = collectionId;
            var viewModel = new EditCollectionViewModel { Title = collectionToEdit.Title, Theme = collectionToEdit.Theme, Description = collectionToEdit.Description };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditCollection(int collectionId, EditCollectionViewModel model)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            Collection editedCollection = await db.Collections.FindAsync(collectionId);
            if (editedCollection != null)
            {
                editedCollection.Title = model.Title;
                editedCollection.Theme = model.Theme;
                editedCollection.Description = model.Description;
                editedCollection.ImageUrl = model.ImageUrl;

                db.Collections.Update(editedCollection);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collectionsOwnerId = editedCollection.UserId });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCollection(int collectionId)
        {
            ApplicationContext db = new ApplicationContext(this._options);
            Collection collectionToDelete = await db.Collections.FindAsync(collectionId);
            if (collectionToDelete != null)
            {
                db.Collections.Remove(collectionToDelete);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collectionsOwnerId = collectionToDelete.UserId});
        }


        [HttpGet]
        public JsonResult GetCollectionsList(string userId)
        {
            using ApplicationContext db = new ApplicationContext(this._options);
            var userCollections = db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            return Json(userCollections);
        }

        public bool isCollectionTitleUnique(string userId, string collectionTitle)
        {
            using ApplicationContext db = new ApplicationContext(this._options);
            var userCollections = db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                if (collection.Title.Equals(collectionTitle))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
