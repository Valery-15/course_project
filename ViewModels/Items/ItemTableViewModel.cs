using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollectionsApp.Models;

namespace CollectionsApp.ViewModels
{
    public class ItemTableViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string JsonTags { get; set; }
        //public IEnumerable<ItemField> AdditionalFields { get; set; }
    }
}
