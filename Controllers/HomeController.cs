using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Pizzeria.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;

        public HomeController(IMongoClient mongoClient)
        {
            _mongoDatabase = mongoClient.GetDatabase(MongoDB.DatabaseName);
        }

        public async Task<IActionResult> Index()
        { 
            var pizzas = await _mongoDatabase.GetCollection<Pizza>(MongoDB.PizzaCollection).Find(new BsonDocument()).ToListAsync();
            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();

            var model = pizzas.Select(p => {
                var viewIngredients = ingredients.Where(i => p.IngredientIds.Contains(i.Id));
                var pizzaView = new PizzaViewModel
                {
                    Id = p.Id,
                    Name = p.Name, 
                    Ingredients = viewIngredients.Select(i => $"{i.Layer.Name}: {i.Name}").ToList(),
                    Price = Math.Round(viewIngredients.Sum(i => i.Price), 2).ToString("0.00") + "€"
                }; 
                return pizzaView;
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> MakePizza()
        {
            var layres = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();

            var models = ingredients.GroupBy(i => i.Layer.Name, (layerName, ingredient) => new LayerGroupedIngredientViewModel{ 
                LayerName = layerName,
                Ingredients = ingredient.Select(i => 
                    new IngredientViewModelForGrouped
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Price = Math.Round(i.Price, 2).ToString("0.00")
                    }).ToList()
            }).ToList();
            return View(models);
        }
    } 
}
