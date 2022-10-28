using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class EditItemViewModel
    {
        public int CollectionId { get; set; }
        public int ItemId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        public List<Tag> Tags { get; set; }

        public List<ItemField> ItemFields { get; set; }

        public EditItemViewModel()
        {
            ItemFields = new List<ItemField>();
            Tags = new List<Tag>();
        }

        public EditItemViewModel(Item itemToEdit, List<Tag> tags)
        {
            CollectionId = itemToEdit.CollectionId;
            ItemId = itemToEdit.Id;
            Title = itemToEdit.Title;
            Tags = tags.Take(3).ToList();
            ItemFields = JsonSerializer.Deserialize<List<ItemField>>(itemToEdit.AdditionalFields);
        }
    }
}
