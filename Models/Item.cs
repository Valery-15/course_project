using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CollectionsApp.Models
{
    public partial class Item
    {
        public Item()
        {
            Comments = new HashSet<Comment>();
            Likes = new HashSet<Like>();
        }

        public int Id { get; set; }
        public int CollectionId { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public DateTime AddDate { get; set; }
        public int LikesNumber { get; set; }
        public int? IntegerField1 { get; set; }
        public int? IntegerField2 { get; set; }
        public int? IntegerField3 { get; set; }
        public string StringField1 { get; set; }
        public string StringField2 { get; set; }
        public string StringField3 { get; set; }
        public string MultiStringField1 { get; set; }
        public string MultiStringField2 { get; set; }
        public string MultiStringField3 { get; set; }
        public bool? BoolField1 { get; set; }
        public bool? BoolField2 { get; set; }
        public bool? BoolField3 { get; set; }
        public DateTime? DateField1 { get; set; }
        public DateTime? DateField2 { get; set; }
        public DateTime? DateField3 { get; set; }

        public virtual Collection Collection { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }
}
