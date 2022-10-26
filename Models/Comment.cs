using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CollectionsApp.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ItemId { get; set; }
        public string Body { get; set; }
        public DateTime AddDate { get; set; }

        public virtual Item Item { get; set; }
        public virtual IdentityUser User { get; set; }

        public Comment(){}

        public Comment(string userId, int itemId, string body)
        {
            UserId = userId;
            ItemId = itemId;
            Body = body;
            AddDate = DateTime.UtcNow;
        }
    }
}
