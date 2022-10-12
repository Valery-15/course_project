using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.Models;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable


namespace CollectionsApp
{
    public class Collection
    {
        public Collection()
        {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [MaxLength(10)]
        public string Theme { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
