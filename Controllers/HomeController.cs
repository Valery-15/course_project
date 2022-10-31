using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.EntityFrameworkCore;
using CollectionsApp.ViewModels;

namespace CoollectionsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationContext _db;

        public HomeController(ApplicationContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var latestItems = new List<Item>(
                (from item in _db.Items.ToList()
                orderby item.AddDate descending, item.Title ascending
                select item).Take(5)
                );

            var largestCollections = new List<Collection>(
                (from collection in _db.Collections.ToList()
                 where collection.Size != 0
                 orderby collection.Size descending, collection.Title ascending
                 select collection).Take(5)
                );

            List<Tag> popularTags = _db.Tags.FromSqlRaw("SELECT Id, TagValue FROM GetTagsWithPopularity()").Take(18).ToList();

            return View(new HomePageViewModel(latestItems, largestCollections, popularTags));
        }
    }
}
