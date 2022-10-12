using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollectionsApp.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(10)]
        public String Status { get; set; }
    }
}
