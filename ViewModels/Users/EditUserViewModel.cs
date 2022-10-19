using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CollectionsApp.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "admin")]
        public bool IsAdmin{ get; set; }

        [Required]
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }
}
