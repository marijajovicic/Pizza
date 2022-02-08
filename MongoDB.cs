using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria
{
    public class MongoDB
    {
        public static string DatabaseName => "pizzeria"; 
        public static string AdministratorCollection => "admin";
        public static string IngredientCollection => "ingredient";
        public static string LayerCollection => "layer";
        public static string PizzaCollection => "pizza";

    }
}
