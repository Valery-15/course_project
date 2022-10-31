using System.Collections.Generic;
using System.Linq;
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
            var itemResults = new List<Item>(from item in _db.Items
                              where EF.Functions.FreeText(item.Title, query) |
                                      EF.Functions.Contains(item.AdditionalFields, query + " NEAR Value")
                              select item);

            AddItemsFromCommentResults(query, itemResults);
            AddItemsFromTagResults(query, itemResults);

            var collectionResults = new List<Collection>(from collection in _db.Collections
                                    where EF.Functions.FreeText(collection.Title, query) |
                                            EF.Functions.FreeText(collection.Theme, query) |
                                            EF.Functions.FreeText(collection.Description, query)
                                    select collection);

            ViewBag.Query = query;
            return View("SearchResult", new SearchResultViewModel { Items = itemResults, Collections = collectionResults});
        }

        private void AddItemsFromCommentResults(string query, List<Item> itemResults)
        {
            var commentsResults = new List<Comment>(from comment in _db.Comments
                                                    where EF.Functions.FreeText(comment.Body, query)
                                                    select comment);

            foreach (var comment in commentsResults)
            {
                Item item = _db.Items.Find(comment.ItemId);
                if (item != null)
                {
                    if (!itemResults.Contains(item))
                    {
                        itemResults.Add(item);
                    }
                }
            }
        }

        private void AddItemsFromTagResults(string query, List<Item> itemResults)
        {
            var tagResults = _db.Tags.Where(tag => EF.Functions.FreeText(tag.TagValue, query)).Include(tag => tag.ItemTags).ToList();

            foreach (var tag in tagResults)
            {
                foreach (var itemTag in tag.ItemTags)
                {
                    var foundItem = _db.Items.Find(itemTag.ItemId);
                    if (foundItem != null)
                    {
                        if (!itemResults.Contains(foundItem))
                        {
                            itemResults.Add(foundItem);
                        }
                    }
                }
            }
        }

    }
}
