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

namespace CollectionsApp.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly DbContextOptions<ApplicationContext> options;

        public CollectionsController(UserManager<User> userManager)
        {
            this.userManager = userManager;

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            string ConnectionString = configuration.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            this.options = optionsBuilder
                    .UseSqlServer(ConnectionString)
                    .Options;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddCollection()
        {
            return View();
        }



        //[HttpPost]
        //public async Task<IActionResult> AddCollection(CollectionViewModel model)
        //{
        //    using ApplicationContext db = new ApplicationContext(this.options);
        //    Collection c = new Collection { Title = model.Title, Description = model.Description,
        //        Theme = model.Theme, ImageUrl = null, }
        //}

        [HttpGet]
        public IActionResult EditCollection(int id)
        {
            return View();
        }


        [HttpGet]
        public JsonResult GetCollectionsList()
        {
            var currentUserId = userManager.GetUserId(this.User);
            using ApplicationContext db = new ApplicationContext(this.options);
            var currentUserCollections = db.Collections.Where(c => c.UserId.Equals(currentUserId)).ToList();
            return Json(currentUserCollections);
        }

        [HttpGet]
        public JsonResult GetCollectionsList(string userId)
        {
            using ApplicationContext db = new ApplicationContext(this.options);
            var userCollections = db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            return Json(userCollections);
        }

    }
}
