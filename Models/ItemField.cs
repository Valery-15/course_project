using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollectionsApp.Models
{
    public class ItemField : CollectionField
    {
        public ItemField()
        {

        }

        public ItemField(CollectionField collectionField)
        {
            Title = collectionField.Title;
            InputType = collectionField.InputType;
        }

        public string Value { get; set; }
    }
}
