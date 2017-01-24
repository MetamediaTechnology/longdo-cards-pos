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

        public static IEnumerable<User> Users { get; set; }

        public IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            filter = filter.ToLower();
            return Users.Where(u => (u.Id?.Contains(filter) ?? false)
                || (u.Mail?.Contains(filter) ?? false)
                || (u.Mobile?.Contains(filter) ?? false)
                || (u.Fullname?.ToLower().Contains(filter) ?? false)).Take(MAX_SUGGEST);
        }
    }
}
