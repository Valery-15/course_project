using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Http;

namespace CollectionsApp.ViewModels
{
    public class CreateCollectionViewModel
    {
        [Required]
        [Display(Name = "Title")]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Theme")]
        public string Theme { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public IEnumerable<CollectionField> AdditionalFields { get; set; }

        public CreateCollectionViewModel()
        {
            AdditionalFields = new List<CollectionField>();
        }
    }
}
