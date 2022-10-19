using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using CollectionsApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CollectionsApp.ViewModels
{
    public class ItemViewModel
    {
        public ItemViewModel()
        {
            AdditionalFields = new List<ItemField>();
            Comments = new List<Comment>();
        }

        public ItemViewModel(Item item, string currentUserId)
        {
            using ApplicationContext db = new ApplicationContext(BuildOptions());
            CollectionTitle = db.Collections.Find(item.CollectionId).Title;
            Title = item.Title;
            Tags = item.Tags;
            LikesNumber = item.LikesNumber;
            if (currentUserId != null)
            {
                var like = new Like();
                like.ItemId = item.Id;
                like.UserId = currentUserId;
                isCurrentUserLikedItem = db.Likes.Contains<Like>(like);
            } 
            else
            {
                isCurrentUserLikedItem = false;
            }
            AdditionalFields = new List<ItemField>(JsonSerializer.Deserialize<ItemField[]>(item.AdditionalFields));
            Comments = new List<Comment>(db.Comments.Where(comment => comment.ItemId == item.Id));
        }

        private DbContextOptions<ApplicationContext> BuildOptions()
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

        public string CollectionTitle { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public int LikesNumber { get; set; }
        public bool isCurrentUserLikedItem { get; set; }
        public List<ItemField> AdditionalFields { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
