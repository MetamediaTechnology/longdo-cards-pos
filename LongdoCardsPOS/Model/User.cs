using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongdoCardsPOS
{
    public class User
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Mail { get; set; }
        public string Mobile { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Fullname
        {
            get
            {
                return (Fname + " " + Lname).Trim();
            }
        }
        public string Gender
        {
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
        public bool IsPlastic
        {
            get
            {
                return Key == "pcard_id";
            }
        }

        public static User FromDict(IDictionary<string, object> dict)
        {
            var isPlastic = dict.String("card_type") == "plastic";
            var user = FromDict(dict, isPlastic);
            if (isPlastic)
            {
                user.Mail = dict.String("pcard_no");
            }
            if (user.Mobile?.StartsWith("+66") ?? false)
            {
                user.Mobile = "0" + user.Mobile.Substring(3);
            }
            if (string.IsNullOrEmpty(user.Fullname))
            {
                user.Fname = dict.String("name");
                user.Lname = "";
            }
            return user;
        }

        public static User FromDict(IDictionary<string, object> dict, bool isPlastic)
        {
            return new User
            {
                Key = isPlastic ? "pcard_id" : "cuid",
                Id = dict.String(isPlastic ? "pcard_id" : "uid"),
                Mail = dict.String("mail"),
                Mobile = dict.String("tel"),
                Fname = dict.String("fname"),
                Lname = dict.String("lname"),
                IsMale = !dict.ContainsKey("gender") || dict["gender"] == null ? (bool?)null : dict.String("gender") == "M"
            };
        }
    }
}
