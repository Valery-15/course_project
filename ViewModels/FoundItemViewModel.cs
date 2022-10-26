using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.ViewModels;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace CollectionsApp.ViewModels
{
    public class FoundItemViewModel
    {
        public Collection FoundItemCollection;
        public Item FoundItem;
    }
}
