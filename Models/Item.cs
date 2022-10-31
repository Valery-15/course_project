using System;
using System.Collections.Generic;

namespace CollectionsApp.Models
{
    public partial class Item
    {
        public Item()
        {
            Comments = new HashSet<Comment>();
            ItemTags = new HashSet<ItemTag>();
            Likes = new HashSet<Like>();
        }

        public int Id { get; set; }
        public int CollectionId { get; set; }
        public string Title { get; set; }
        public DateTime AddDate { get; set; }
        public int LikesNumber { get; set; }
        public string AdditionalFields { get; set; }
        public string Author { get; set; }

        public virtual Collection Collection { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ItemTag> ItemTags { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }
}
