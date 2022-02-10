using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria.Models
{
    public class LayerGroupedIngredientViewModel
    {
        public string LayerName { get; set; }
        public IList<IngredientViewModelForGrouped> Ingredients { get; set; }
    }

    public class IngredientViewModelForGrouped
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; } 
    }
}
