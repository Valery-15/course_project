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
        public CreateItemViewModel()
        {
            Fields = new List<ItemField>();
        }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [MaxLength(150)]
        public string Tags { get; set; }

        public List<ItemField> Fields  { get; set; } 
    }
}
