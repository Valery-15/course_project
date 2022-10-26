using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            LatestItems = new List<Item>();
            LargestCollections = new List<Collection>();
            PopularTags = new List<Tag>();
        }

        public HomePageViewModel(IEnumerable<Item> latestItems, 
            IEnumerable<Collection> largestCollections,
            IEnumerable<Tag> popularTags)
        {
            LatestItems = latestItems;
            LargestCollections = largestCollections;
            PopularTags = popularTags;
        }

        public IEnumerable<Item> LatestItems;
        public IEnumerable<Collection> LargestCollections;
        public IEnumerable<Tag> PopularTags;
    }
}
