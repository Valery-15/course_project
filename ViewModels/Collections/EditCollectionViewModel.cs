using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class EditCollectionViewModel
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

        public EditCollectionViewModel(){}

        public EditCollectionViewModel(Collection collectionToEdit) {
            Title = collectionToEdit.Title;
            Theme = collectionToEdit.Theme;
            Description = collectionToEdit.Description;
        }

    }
}
