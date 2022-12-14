using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CollectionsApp.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ItemId { get; set; }

        public virtual Item Item { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
