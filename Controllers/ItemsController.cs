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
using Markdig;

namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class ItemsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationContext _db;

        public ItemsController(UserManager<IdentityUser> userManager,
            ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ItemsList(int collectionId)
        {
            var collection = _db.Collections.Find(collectionId);
            if(collection != null)
            {
                _db.Entry(collection).Reference("User").Load();
            }
            ViewBag.collection = collection;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Item(int itemId)
        {
            Item item = await _db.Items.FindAsync(itemId);
            List<Tag> itemTags = GetTagsByItem(itemId);
            var currentUserId = _userManager.GetUserId(this.User);
            var model = new ItemViewModel(item, itemTags, currentUserId, _db);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateItem(int collectionId)
        {
            Collection collection = _db.Collections.Find(collectionId);
            CreateItemViewModel model = new CreateItemViewModel(GetItemFields(collection));
            ViewBag.collection = collection;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateItem(int collectionId, CreateItemViewModel model,
            List<Tag> tags, List<ItemField> itemFields)
        {
            if(ModelState.IsValid & IsItemTitleUniqueInCollection(collectionId, model.Title) 
                & !ContainsItemTagsDuplicates(tags))
            {
                AddItemToDb(collectionId, model, tags, itemFields);
                return RedirectToAction("ItemsList", new { collectionId = collectionId });
            }
            model.Tags = tags;
            model.ItemFields = itemFields;
            ViewBag.collection = _db.Collections.Find(collectionId);
            return View(model);
        }

        [HttpGet]
        public IActionResult EditItem(int itemId)
        {
            Item itemToEdit = _db.Items.Find(itemId);
            var tags = GetTagsByItem(itemId);
            var model = new EditItemViewModel(itemToEdit, tags);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditItem(EditItemViewModel model, List<ItemField> itemFields,
            List<Tag> tags)
        {
            Item itemToEdit = _db.Items.Find(model.ItemId);
            if (ModelState.IsValid & IsEditedItemTitleUniqueInCollection(itemToEdit, model)
                & !ContainsItemTagsDuplicates(tags))
            {
                UpdateItemInDb(itemToEdit, model, itemFields, tags);
                return RedirectToAction("Item", new { itemId = model.ItemId });
            }
            model.Tags = tags;
            model.ItemFields = itemFields;
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteItem(int itemId)
        {
            Item itemToDelete = _db.Items.Find(itemId);
            if (itemToDelete != null)
            {
                _db.Items.Remove(itemToDelete);
                _db.SaveChanges();
            }
            return RedirectToAction("ItemsList",new { collectionId = itemToDelete.CollectionId });
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetItemsList(int collectionId)
        {
            var collectionItems = _db.Items.Where(item => item.CollectionId.Equals(collectionId)).ToList();
            var itemsList = new List<ItemTableViewModel>();
            foreach(var item in collectionItems)
            {
                var itemtagsList = _db.ItemTags.Where(itemtag => itemtag.ItemId == item.Id).ToList();
                var tagsList = new List<Tag>();
                foreach(var itemtag in itemtagsList)
                {
                    tagsList.Add(_db.Tags.Find(itemtag.TagId));
                }
                itemsList.Add(new ItemTableViewModel { 
                    Id = item.Id, 
                    Title = item.Title, 
                    JsonTags = JsonSerializer.Serialize(tagsList) });
            }
            return Json(itemsList);

        }

        private List<Tag> GetTagsByItem(int itemId)
        {
            var itemtags = _db.ItemTags.Where(itemtag => itemtag.ItemId == itemId).ToList();
            var tags = new List<Tag>();
            foreach(var itemtag in itemtags)
            {
                tags.Add(_db.Tags.Find(itemtag.TagId));
            }
            return tags;
        }

        private List<CollectionField> GetCollectionFields(Collection collection)
        {
            var collectionFields = new List<CollectionField>();
            if (collection != null)
            {
                if (collection.AdditionalFields != null)
                {
                    collectionFields = JsonSerializer.Deserialize<List<CollectionField>>(collection.AdditionalFields);
                }
            }
            return collectionFields;
        }

        private List<ItemField> GetItemFields(Collection collection)
        {
            var collectionFields = GetCollectionFields(collection);
            var itemFields = new List<ItemField>();
            foreach (var collectionField in collectionFields)
            {
                itemFields.Add(new ItemField(collectionField));
            }
            return itemFields;
        }

        private void DeleteItemTagsByItem(int itemId)
        {
            var tagsToDelete = _db.ItemTags.Where(itemtag => itemtag.ItemId == itemId);
            foreach (var itemtag in tagsToDelete)
            {
                _db.ItemTags.Remove(itemtag);
            }
            _db.SaveChanges();
        }

        private bool IsItemTitleUniqueInCollection(int collectionId, string itemTitle)
        {
            var itemsFromCollection = _db.Items.Where(item => item.CollectionId == collectionId).ToList();
            foreach(var item in itemsFromCollection)
            {
                if (item.Title.Equals(itemTitle))
                {
                    ModelState.AddModelError(string.Empty, "Item title \"" + itemTitle + "\" is already taken.");
                    return false;
                }
            }
            return true;
        }

        private bool IsEditedItemTitleUniqueInCollection(Item itemToEdit, EditItemViewModel model)
        {
            if (itemToEdit.Title.Equals(model.Title))
            {
                return true;
            }
            return IsItemTitleUniqueInCollection(itemToEdit.CollectionId, model.Title);
        }

        private bool ContainsItemTagsDuplicates(List<Tag> tags)
        {
            tags.RemoveAll(tag => tag.TagValue == null);
            var tagValues = new List<string>();
            foreach(var tag in tags)
            {
                tagValues.Add(tag.TagValue);
            }
            bool containsDuplicates = tagValues.Count() != tagValues.Distinct().Count();
            if (containsDuplicates)
            {
                ModelState.AddModelError(string.Empty, "Tag values can't contain duplicates.");
            }
            return containsDuplicates;
        }

        private void AddItemToDb(int collectionId, CreateItemViewModel model,
            List<Tag> tags, List<ItemField> itemFields)
        {
            Item itemToAdd = new Item
            {
                Title = model.Title,
                AddDate = DateTime.Now,
                CollectionId = collectionId,
                AdditionalFields = JsonSerializer.Serialize(itemFields),
                Author = _userManager.GetUserName(this.User)
            };
            _db.Items.Add(itemToAdd);
            _db.SaveChanges();

            AddItemTagsToDb(itemToAdd, tags);
        }

        private void AddItemTagsToDb(Item item, List<Tag> tags)
        {
            tags.RemoveAll(tag => tag.TagValue == null);

            foreach (var tag in tags)
            {
                bool isTagExistsInDb = _db.Tags.Where(t => t.TagValue.Equals(tag.TagValue)).Count() != 0;
                Tag tagForItem;
                if (isTagExistsInDb)
                {
                    tagForItem = _db.Tags.Where(t => t.TagValue.Equals(tag.TagValue)).First();
                } else
                {
                    _db.Tags.Add(tag);
                    _db.SaveChanges();
                    tagForItem = tag;
                }
                _db.ItemTags.Add(new ItemTag { ItemId = item.Id, TagId = tagForItem.Id });
                _db.SaveChanges();
            }
        }


        private void UpdateItemInDb(Item itemToEdit, EditItemViewModel model, List<ItemField> itemFields,
            List<Tag> tags)
        {
            itemToEdit.Title = model.Title;
            itemToEdit.AdditionalFields = JsonSerializer.Serialize<List<ItemField>>(itemFields);
            _db.Items.Update(itemToEdit);
            _db.SaveChanges();
            UpdateItemTagsInDb(itemToEdit, tags);
        }

        private void UpdateItemTagsInDb(Item item, List<Tag> tags)
        {
            DeleteItemTagsByItem(item.Id);
            AddItemTagsToDb(item, tags);
        }
    }
}
