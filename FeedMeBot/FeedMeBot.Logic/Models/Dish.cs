using System;
using System.Collections.Generic;

namespace FeedMeBot.Logic
{
    [Serializable]
    public class Dish
    {
        public string Base { get; set; }

        public List<string> Fillings { get; set; }


        public Dish()
        {
            Fillings = new List<string>();
        }


        public override string ToString()
        {
            return $"{Base} ({string.Join(", ", Fillings)})";
        }
    }
}
