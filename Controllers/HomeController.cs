using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            List<Item> latestItems = new List<Item>(
                (from item in _db.Items.ToList()
                orderby item.AddDate descending, item.Title ascending
                select item).Take<Item>(4)
                );

            List<Collection> largestCollections = new List<Collection>(
                (from collection in _db.Collections.ToList()
                 where collection.Size != 0
                 orderby collection.Size descending, collection.Title ascending
                 select collection).Take<Collection>(5)
                );

            var popularTags = _db.Tags.FromSqlRaw("SELECT Id, TagValue FROM GetTagsWithPopularity()").ToList();

            return View(new HomePageViewModel(latestItems, largestCollections, popularTags));
        }


    }
}
