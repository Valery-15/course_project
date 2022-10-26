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
            ViewBag.collection = _db.Collections.Find(collectionId);
            _db.Entry(ViewBag.collection).Reference("User").Load();
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Item(int itemId)
        {
            Item item = await _db.Items.FindAsync(itemId);
            var currentUserId = _userManager.GetUserId(this.User);
            var model = new ItemViewModel(item, currentUserId, _db);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateItem(int collectionId)
        {
            Collection collection = _db.Collections.Find(collectionId);
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
            ViewBag.collection = collection;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateItem(int collectionId, CreateItemViewModel model, 
            ItemField[] itemFields, string[] tags)
        {
            if (!ModelState.IsValid)
            {
                //установить значения
                ViewBag.collection = _db.Collections.Find(collectionId);
                return View(model);
            }
            Collection collection = _db.Collections.Find(collectionId);
            if (collection != null)
            {

                Item itemToAdd = new Item();
                itemToAdd.Title = model.Title;
                //itemToAdd.Tags = model.Tags;
                itemToAdd.AddDate = DateTime.Now;
                itemToAdd.CollectionId = collectionId;
                itemToAdd.AdditionalFields = JsonSerializer.Serialize(itemFields);
                itemToAdd.Author = _userManager.GetUserName(this.User);

                _db.Items.Add(itemToAdd);
                _db.SaveChanges();


                List<string> tagsList = new List<string>(tags);
                tagsList.RemoveAll(tagValue => tagValue == null);
                

                foreach (var tag in tagsList)
                {
                    Tag tagToAdd;
                    if (_db.Tags.Where(t => t.TagValue.Equals(tag)).Count() == 0)
                    {
                        tagToAdd = new Tag { TagValue = tag };
                        _db.Tags.Add(tagToAdd);
                        _db.SaveChanges();
                    } else
                    {
                        tagToAdd = _db.Tags.Where(t => t.TagValue.Equals(tag)).First();
                    }
                    _db.ItemTags.Add(new ItemTag { ItemId = itemToAdd.Id, TagId = tagToAdd.Id });
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("ItemsList", new { collectionId = collectionId });
        }

        [HttpGet]
        public IActionResult EditItem(int itemId)
        {
            Item itemToEdit = _db.Items.Find(itemId);
            List<ItemField> itemFields = new List<ItemField>(JsonSerializer.Deserialize<ItemField[]>(itemToEdit.AdditionalFields));
            EditItemViewModel model = new EditItemViewModel();
            model.Title = itemToEdit.Title;
            //model.Tags = _db.ItemTags.Where(itemtag => itemtag.ItemId == itemId);
            model.ItemFields = itemFields;
            model.CollectionId = itemToEdit.CollectionId;
            model.ItemId = itemId;
            return View(model);
        }

        [HttpPost]
        public IActionResult EditItem(EditItemViewModel model, ItemField[] itemFields)
        {
            if (!ModelState.IsValid) return View(model);

            Item itemToEdit = _db.Items.Find(model.ItemId);
            if (itemToEdit != null)
            {
                itemToEdit.Title = model.Title;
                //itemToEdit.Tags = model.Tags;
                itemToEdit.AdditionalFields = JsonSerializer.Serialize<ItemField[]>(itemFields);
                _db.Items.Update(itemToEdit);
                _db.SaveChanges();
                
            }
            return RedirectToAction("Item", new { itemId = model.ItemId });
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
    }
}
