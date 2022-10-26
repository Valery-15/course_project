﻿using System;
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
            ItemFields = new List<ItemField>();
        }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        public IEnumerable<ItemField> ItemFields { get; set; }
    }
}
