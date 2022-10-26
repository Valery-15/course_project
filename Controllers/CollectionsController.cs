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
                var collectionToAdd = new Collection(collectionsOwnerId, model, collectionFields);
                await _db.Collections.AddAsync(collectionToAdd);
                _db.SaveChanges();
                return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionsOwnerId });
            }
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCollection(int collectionId)
        {
            Collection collectionToEdit = await _db.Collections.FindAsync(collectionId);
            ViewBag.CollectionId = collectionId;
            var viewModel = new EditCollectionViewModel { Title = collectionToEdit.Title, Theme = collectionToEdit.Theme, Description = collectionToEdit.Description };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditCollection(int collectionId, EditCollectionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            Collection collectionToEdit = _db.Collections.Find(collectionId);
            if (collectionToEdit != null)
            {
                EditColletionFieldValues(collectionToEdit, model);
                _db.Collections.Update(collectionToEdit);
                _db.SaveChanges();
            }
            return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToEdit.UserId });
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
            return Json(userCollections);
        }


        private bool IsCollectionTitleUnique(string userId, string collectionTitle)
        {
            var userCollections = _db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                if (collection.Title.Equals(collectionTitle))
                {
                    ModelState.AddModelError(string.Empty, "Title \"" + collectionTitle + "\" is already taken.");
                    return false;
                }
            }
            return true;
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
            bool containDuplicates = fieldTitlesList.Distinct().Count() != fieldTitlesList.Count;
            if (containDuplicates)
            {
                ModelState.AddModelError(string.Empty, "Collection field titles can't contain duplicates.");
            }
            return containDuplicates;
        }

        private void EditColletionFieldValues(Collection collectionToEdit, EditCollectionViewModel model)
        {
            collectionToEdit.Title = model.Title;
            collectionToEdit.Theme = model.Theme;
            collectionToEdit.Description = model.Description;
            collectionToEdit.ImageUrl = model.ImageUrl;
        }
    }
}
