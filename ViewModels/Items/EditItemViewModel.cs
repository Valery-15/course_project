using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class EditItemViewModel
    {
        public EditItemViewModel()
        {
            Tags = new List<string>();
        }

        public int CollectionId { get; set; }
        public int ItemId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        public List<string> Tags { get; set; }

        public List<ItemField> ItemFields { get; set; }
    }
}
