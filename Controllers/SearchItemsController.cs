using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CollectionsApp.ViewModels;

namespace CollectionsApp.Controllers
{
    public class SearchItemsController : Controller
    {
        private readonly ApplicationContext _db;

        public SearchItemsController(ApplicationContext db)
        {
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult FindItems(string query)
        {
            var itemResults = from item in _db.Items
                              where EF.Functions.FreeText(item.Title, query)
                              select item;

            var collectionResults = from collection in _db.Collections
                                    where EF.Functions.FreeText(collection.Title, query) ||
                                            EF.Functions.FreeText(collection.Theme, query) ||
                                            EF.Functions.FreeText(collection.Description, query)
                                    select collection;

            var tagResults = _db.Tags.Where(tag => EF.Functions.FreeText(tag.TagValue, query)).Include(tag => tag.ItemTags).ToList();
           
            foreach(var tag in tagResults)
            {
                foreach(var itemTag in tag.ItemTags)
                {
                    var foundItem = _db.Items.Find(itemTag.ItemId);
                    if (foundItem != null)
                    {
                        if (!itemResults.Contains(foundItem))
                        {
                            itemResults.AsEnumerable().ToList().Add(foundItem);
                        }
                    }
                }
            }

            ViewBag.Query = query;
            return View("SearchResult", new SearchResultViewModel { Items = itemResults, Collections = collectionResults});
        }
    }
}
