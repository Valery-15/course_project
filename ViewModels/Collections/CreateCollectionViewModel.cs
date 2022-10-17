using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CollectionsApp.ViewModels
{
    public class CreateCollectionViewModel
    {
        [Required]
        [Display(Name = "Title")]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Theme")]
        public string Theme { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        public string IntegerField1 { get; set; }
        public string IntegerField2 { get; set; }
        public string IntegerField3 { get; set; }

        public string StringField1 { get; set; }
        public string StringField2 { get; set; }
        public string StringField3 { get; set; }

        public string MultiStringField1 { get; set; }
        public string MultiStringField2 { get; set; }
        public string MultiStringField3 { get; set; }

        public string BoolField1 { get; set; }
        public string BoolField2 { get; set; }
        public string BoolField3 { get; set; }

        public string DateField1 { get; set; }
        public string DateField2 { get; set; }
        public string DateField3 { get; set; }

        public List<string> GetCollectionFieldNames()
        {
            List<string> fieldNamesList = new List<string>(new string[]{
                IntegerField1, IntegerField2, IntegerField3,
                StringField1, StringField2, StringField3,
                MultiStringField1, MultiStringField2, MultiStringField3,
                BoolField1, BoolField2, BoolField3,
                DateField1, DateField2, DateField3
            });
            fieldNamesList.RemoveAll(s => s == null);
            return fieldNamesList;
        }

        public bool ContainsFieldNamesDuplicates()
        {
            List<string> fieldNames = GetCollectionFieldNames();
            return (fieldNames.Count != fieldNames.Distinct().Count());
        }
    }
}
