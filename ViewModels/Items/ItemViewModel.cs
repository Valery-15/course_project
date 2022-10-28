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
        public int CollectionId { get; set; }
        public int ItemId { get; set; }
        public string CollectionTitle { get; set; }
        public string Title { get; set; }
        public int LikesNumber { get; set; }
        public bool isCurrentUserLikedItem { get; set; }
        public List<ItemField> AdditionalFields { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Tag> Tags { get; set; }

        public ItemViewModel()
        {
            AdditionalFields = new List<ItemField>();
            Comments = new List<Comment>();
        }

        public ItemViewModel(Item item, List<Tag> tags, string currentUserId, ApplicationContext db)
        {
            CollectionId = item.CollectionId;
            CollectionTitle = db.Collections.Find(item.CollectionId).Title;
            ItemId = item.Id;
            Title = item.Title;
            Tags = tags;
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
            Comments = new List<Comment>(db.Comments.Where(comment => comment.ItemId == item.Id).Include(comment => comment.User));
        }

    }
}
