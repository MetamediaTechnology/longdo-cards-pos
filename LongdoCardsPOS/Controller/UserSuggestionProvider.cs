using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfControls;

namespace LongdoCardsPOS.Controller
{
    class UserSuggestionProvider : ISuggestionProvider
    {
        const int MAX_SUGGEST = 5;
        static char[] SEPARATOR = new char[] { ':' };

        public static IEnumerable<User> Users { get; set; }
        public static IEnumerable<User> FilterUsers { get; set; }

        public IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || Users == null)
            {
                FilterUsers = null;
            }
            else
            {
                filter = filter.ToLower();
                if (filter.StartsWith("c0:") || filter.StartsWith("c1:"))
                {
                    filter = filter.Split(SEPARATOR, 3)[1];
                    FilterUsers = Users.Where(u => u.Id == filter); ;
                }
                else
                {
                    FilterUsers = Users.Where(u => (u.Id?.Contains(filter) ?? false)
                        || (u.Mail?.Contains(filter) ?? false)
                        || (u.Mobile?.Contains(filter) ?? false)
                        || (u.Fullname?.ToLower().Contains(filter) ?? false)).Take(MAX_SUGGEST);
                }
            }

            return FilterUsers;
        }
    }
}
