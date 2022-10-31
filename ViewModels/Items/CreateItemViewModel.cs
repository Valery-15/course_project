using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class CreateItemViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        public List<Tag> Tags { get; set; }

        public List<ItemField> ItemFields { get; set; }

        public CreateItemViewModel()
        {
            ItemFields = new List<ItemField>();
            Tags = new List<Tag>();
        }

        public CreateItemViewModel(List<ItemField> itemFields)
        {
            ItemFields = itemFields;
            Tags = new List<Tag>();
        }
    }
}
