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
    [Authorize(Roles = "active user")]
    public class CollectionsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DbContextOptions<ApplicationContext> _options;

        public CollectionsController(UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            this._options = BuildOptions();
        }

        private DbContextOptions<ApplicationContext>BuildOptions()
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

        public IActionResult CollectionsList(string collectionsOwnerId)
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
        public async Task<IActionResult> CreateCollection(string collectionsOwnerId, CreateCollectionViewModel model,
            CollectionField[] collectionFields)
        {
            if (ModelState.IsValid)
            {
                if(!isCollectionTitleUnique(collectionsOwnerId, model.Title))
                {
                    ModelState.AddModelError(string.Empty, "Title \"" + model.Title + "\" is already taken.");
                }
                else if (ContainsCollectionFieldTitlesDuplicates(collectionFields))
                {
                    ModelState.AddModelError(string.Empty, "Collection field titles can't contain duplicates.");
                }
                else
                {
                    using ApplicationContext db = new ApplicationContext(this._options);
                    Collection collectionToAdd = MakeCollection(collectionsOwnerId, model, collectionFields);
                    await db.Collections.AddAsync(collectionToAdd);
                    db.SaveChanges();
                    return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionsOwnerId });
                } 
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
            return RedirectToAction("CollectionsList", new { collectionsOwnerId = editedCollection.UserId });
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
            return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToDelete.UserId});
        }

        [HttpGet]
        public JsonResult GetCollectionsList(string userId)
        {
            using ApplicationContext db = new ApplicationContext(this._options);
            var userCollections = db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            return Json(userCollections);
        }


        private bool isCollectionTitleUnique(string userId, string collectionTitle)
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

        private bool ContainsCollectionFieldTitlesDuplicates(CollectionField[] collectionFields)
        {
            var fieldTitlesList = new List<string>(
                            new string[] { "Title", "Theme", "Description", "Size" }
                        );
            foreach (var collectionField in collectionFields)
            {
                fieldTitlesList.Add(collectionField.Title);
            }
            fieldTitlesList.RemoveAll(s => s == null);
            return fieldTitlesList.Distinct().Count() != fieldTitlesList.Count;
        }

        
        private Collection MakeCollection(string collectionsOwnerId, CreateCollectionViewModel model, CollectionField[] collectionFields)
        {
            var collectionFieldsList = new List<CollectionField>(collectionFields);
            collectionFieldsList.RemoveAll(s => s.Title == null);
            string jsonCollectionFieldsList = JsonSerializer.Serialize(collectionFieldsList.ToArray());
            var collection = new Collection
            {
                UserId = collectionsOwnerId,
                Title = model.Title,
                Description = model.Description,
                Theme = model.Theme,
                ImageUrl = model.ImageUrl,
                AdditionalFields = jsonCollectionFieldsList
            };
            return collection;
        }

    }
}
