using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CollectionsApp.Models
{
    public class Tag
    {
        public Tag()
        {
            ItemTags = new HashSet<ItemTag>();
        }

        public int Id { get; set; }
        public string TagValue { get; set; }

        [JsonIgnore]
        public virtual ICollection<ItemTag> ItemTags { get; set; }
    }
}
