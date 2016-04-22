using LongdoCardsPOS.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongdoCardsPOS.Model
{
    public class User
    {
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

        public static User FromDict(object data)
        {
            var dict = data.ToDict();
            return new User
            {
                Id = dict.String("pcard_id"),
                Mobile = dict.String("mobile_no"),
                Fname = dict.String("fname"),
                Lname = dict.String("lname"),
                IsMale = dict.String("gender") == "M"
            };
        }
    }
}
