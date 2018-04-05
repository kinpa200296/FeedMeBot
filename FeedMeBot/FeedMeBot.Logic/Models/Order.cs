using System;
using System.Collections.Generic;

namespace FeedMeBot.Logic
{
    [Serializable]
    public class Order
    {
        public List<Dish> Dishes { get; set; }


        public Order()
        {
            Dishes = new List<Dish>();
        }
    }
}
