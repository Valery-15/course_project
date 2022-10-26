using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class SearchResultViewModel
    {
        public IEnumerable<Collection> Collections;
        public IEnumerable<Item> Items;

        public SearchResultViewModel()
        {
            Collections = new List<Collection>();
            Items = new List<Item>();
        }
    }
}
