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
using Markdig;

namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class CollectionsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationContext _db;

        public CollectionsController(UserManager<IdentityUser> userManager,
            ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CollectionsList(string collectionsOwnerId)
        {
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View();
        }

        [HttpGet]
        public IActionResult CreateCollection(string collectionsOwnerId)
        {
            ViewBag.collectionFields = null;
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection(string collectionsOwnerId, 
            CreateCollectionViewModel model, CollectionField[] collectionFields)
        {
            if (ModelState.IsValid & IsCollectionTitleUnique(collectionsOwnerId, model.Title) &
                    !ContainsCollectionFieldTitlesDuplicates(collectionFields) )
            {
                await AddCollectionToDb(collectionsOwnerId, model, collectionFields);
                return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionsOwnerId });
            }
            ViewBag.collectionFields = collectionFields;
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCollection(int collectionId)
        {
            Collection collectionToEdit = await _db.Collections.FindAsync(collectionId);
            var model = new EditCollectionViewModel(collectionToEdit);
            ViewBag.collection = collectionToEdit;
            return View(model);
        }

        [HttpPost]
        public IActionResult EditCollection(int collectionId, EditCollectionViewModel model)
        {
            Collection collectionToEdit = _db.Collections.Find(collectionId);
            if (ModelState.IsValid & IsEditedCollectionTitleUnique(collectionToEdit.UserId, collectionToEdit.Title, model.Title))
            {
                UpdateCollectionInDb(collectionToEdit, model);
                return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToEdit.UserId });
            }
            ViewBag.collection = collectionToEdit;
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteCollection(int collectionId)
        {
            Collection collectionToDelete = _db.Collections.Find(collectionId);
            if (collectionToDelete != null)
            {
                _db.Collections.Remove(collectionToDelete);
                _db.SaveChanges();
            }
            return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToDelete.UserId});
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetCollectionsList(string userId)
        {
            var userCollections = _db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                string htmlDescription = Markdown.ToHtml(collection.Description);
                collection.Description = htmlDescription;
            }
            return Json(userCollections);
        }


        private bool IsCollectionTitleUnique(string userId, string collectionTitle)
        {
            var userCollections = _db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                if (collection.Title.Equals(collectionTitle))
                {
                    ModelState.AddModelError(string.Empty, "Collection title \"" + collectionTitle + "\" is already taken.");
                    return false;
                }
            }
            return true;
        }

        
        private bool IsEditedCollectionTitleUnique(string userId, string previousCollectionTitle, 
            string newCollectionTitle)
        {
            if (previousCollectionTitle.Equals(newCollectionTitle))
            {
                return true;
            } else
            {
                return IsCollectionTitleUnique(userId, newCollectionTitle);
            }
        }

        private bool ContainsCollectionFieldTitlesDuplicates(CollectionField[] collectionFields)
        {
            var fieldTitlesList = new List<string>(
                            new string[] { "Title", "Tags" }
                        );
            foreach (var collectionField in collectionFields)
            {
                fieldTitlesList.Add(collectionField.Title);
            }
            fieldTitlesList.RemoveAll(s => s == null);
            bool containsDuplicates = fieldTitlesList.Distinct().Count() != fieldTitlesList.Count;
            if (containsDuplicates)
            {
                ModelState.AddModelError(string.Empty, "Collection field titles can't contain duplicates.");
            }
            return containsDuplicates;
        }

        private async Task AddCollectionToDb(string collectionsOwnerId,
            CreateCollectionViewModel model, CollectionField[] collectionFields)
        {
            var collectionToAdd = new Collection(collectionsOwnerId, model, collectionFields);
            await _db.Collections.AddAsync(collectionToAdd);
            _db.SaveChanges();
        }

        private void UpdateCollectionInDb(Collection collectionToEdit, EditCollectionViewModel model)
        {
            EditCollectionFieldValues(collectionToEdit, model);
            _db.Collections.Update(collectionToEdit);
            _db.SaveChanges();
        }

        
        private void EditCollectionFieldValues(Collection collectionToEdit, EditCollectionViewModel model)
        {
            collectionToEdit.Title = model.Title;
            collectionToEdit.Theme = model.Theme;
            collectionToEdit.Description = model.Description;
        }

        
    }
}
