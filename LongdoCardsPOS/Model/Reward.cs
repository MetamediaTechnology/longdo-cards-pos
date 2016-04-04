using LongdoCardsPOS.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongdoCardsPOS.Model
{
    public class Reward
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
        public string DisplayAmount { get; set; }

        public static Reward FromDict(object data)
        {
            var dict = data.ToDict();
            return new Reward
            {
                Id = dict.String("reward_id"),
                Name = dict.String("name"),
                Amount = dict.String("point_amount"),
                DisplayAmount = dict.String("display_amount"),
            };
        }
    }
}
