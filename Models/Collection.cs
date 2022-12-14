using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.ViewModels;

namespace CollectionsApp.Models
{
    public partial class Collection
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public int Size { get; set; }
        public string ImageUrl { get; set; }
        public string AdditionalFields { get; set; }

        public virtual IdentityUser User { get; set; }
        public virtual ICollection<Item> Items { get; set; }

        public Collection()
        {
            Items = new HashSet<Item>();
        }

        public Collection(string collectionOwnerId, CreateCollectionViewModel model, List<CollectionField> additionalFields)
        {
            UserId = collectionOwnerId;
            Title = model.Title;
            Description = model.Description;
            Theme = model.Theme;
            additionalFields.RemoveAll(s => s.Title == null);
            AdditionalFields = JsonSerializer.Serialize(additionalFields);
        }
    }
}
