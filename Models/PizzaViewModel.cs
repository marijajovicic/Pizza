using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria.Models
{
    public class PizzaViewModel
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<string> Ingredients { get; set; }
        public string Price { get; set; }
    }
}
