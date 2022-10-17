using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.Models;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable


namespace CollectionsApp.Models
{
    public partial class Collection
    {
        public Collection()
        {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public int Size { get; set; }
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

        public virtual User User { get; set; }
        public virtual ICollection<Item> Items { get; set; }

   

        public List<ItemField> GetItemFields()
        {
            List<ItemField> fieldsList = new List<ItemField>();
            List<string> integerFields = new List<string>(new string[]{ IntegerField1, IntegerField2, IntegerField3});
            List<string> stringFields = new List<string>(new string[] { StringField1, StringField2, StringField3 });
            List<string> multiStringFields = new List<string>(new string[] { MultiStringField1, MultiStringField2, MultiStringField3 });
            List<string> boolFields = new List<string>(new string[] { BoolField1, BoolField2, BoolField3 });
            List<string> dateFields = new List<string>(new string[] { DateField1, DateField2, DateField3 });

            foreach(var integerField in integerFields)
            {
                if (integerField != null)
                {
                    fieldsList.Add(new ItemField { Title = integerField, Type = "number" });
                }
            }

            foreach (var stringField in stringFields)
            {
                if (stringField != null)
                {
                    fieldsList.Add(new ItemField { Title = stringField, Type = "text" });
                }
            }

            foreach (var multiStringField in multiStringFields)
            {
                if (multiStringField != null)
                {
                    fieldsList.Add(new ItemField { Title = multiStringField, Type = "textarea" });
                }
            }

            foreach (var boolField in boolFields)
            {
                if (boolField != null)
                {
                    fieldsList.Add(new ItemField { Title = boolField, Type = "checkbox" });
                }
            }

            foreach (var dateField in dateFields)
            {
                if (dateField != null)
                {
                    fieldsList.Add(new ItemField { Title = dateField, Type = "date" });
                }
            }
            return fieldsList;
        }
    }
}
