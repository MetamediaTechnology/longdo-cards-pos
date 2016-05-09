using LongdoCardsPOS.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongdoCardsPOS.Model
{
    public class User
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Mobile { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Gender {
            get
            {
                return IsMale == null ? string.Empty : (IsMale.Value ? "M" : "F");
            }
        }

        public bool? IsMale { get; set; }

        public bool? IsFemale
        {
            get
            {
                return !IsMale;
            }
            set
            {
                IsMale = !value;
            }
        }

        public static User FromDict(object data, bool isPlastic)
        {
            var dict = data.ToDict();
            return new User
            {
                Key = isPlastic ? "pcard_id" : "cuid",
                Id = dict.String(isPlastic ? "pcard_id" : "uid"),
                Mobile = dict.String("tel"),
                Fname = dict.String("fname"),
                Lname = dict.String("lname"),
                IsMale = dict["gender"] == null ? (bool?)null : dict.String("gender") == "M"
            };
        }
    }
}
