using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Pizzeria.Helper;
using Pizzeria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pizzeria.Controllers
{
    public class AdminController : Controller
    {
       private readonly IMongoDatabase _mongoDatabase;

        public AdminController(IMongoClient mongoClient)
        {
            _mongoDatabase = mongoClient.GetDatabase(MongoDB.DatabaseName);
        }

        public IActionResult Index()
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Login");
            }
            return View(); 
        }

        public async Task<IActionResult> Login()
        {
            if (!SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            Administrator administrator = new Administrator
            {
                Username = "admin",
                Password = "admin"
            };

            var entry = await _mongoDatabase.GetCollection<Administrator>(MongoDB.AdministratorCollection).Find(a => a.Username == administrator.Username && a.Password == administrator.Password).FirstOrDefaultAsync();
            if (entry is null)
            {
                await _mongoDatabase.GetCollection<Administrator>(MongoDB.AdministratorCollection).InsertOneAsync(administrator);
            } 

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Administrator administrator)
        {
            if (!SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            var admin = await _mongoDatabase.GetCollection<Administrator>(MongoDB.AdministratorCollection).Find(a => a.Username == administrator.Username && a.Password == administrator.Password).FirstOrDefaultAsync();
            if (admin is null)
            {
                return View((object)"Wrong username or password");
            } 

            SessionHelper.SetUsername(HttpContext.Session, administrator.Username);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Ingredient()
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();
            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var models = ingredients.Select(i => new IngredientViewTableModel
            {
                Id = i.Id,
                LayerName = i.Layer.Name,
                Name = i.Name,
                Price = Math.Round(i.Price, 2).ToString("0.00").Replace(",", ".")
            });

            return View("Ingredient", (models, layers, "", ""));
        }

        [HttpPost]
        public async Task<IActionResult> AddIngredient(IngredientViemModel ingredient)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();
            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var models = ingredients.Select(i => new IngredientViewTableModel
            {
                Id = i.Id,
                LayerName = i.Layer.Name,
                Name = i.Name,
                Price = Math.Round(i.Price, 2).ToString("0.00").Replace(",", ".")
            });

            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                return View("Ingredient", (models, layers, "Name can not be empty", ""));
            } 
            var price = float.Parse(ingredient.Price.Replace(".", ","));
            if (price < 0)
            {
                return View("Ingredient", (models, layers, "Price must be greater than zero", ""));
            }
            var layer = layers.FirstOrDefault(l => l.Id == ingredient.LayerId);
            if (layer == null)
            {
                return View("Ingredient", (models, layers, "Layer does not exists", ""));
            } 

            var ingredientCreate = new Ingredient
            {
               Layer = layer,
               Name = ingredient.Name,
               Price = price
            };
            await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).InsertOneAsync(ingredientCreate);

            return RedirectToAction("Ingredient");
        }
 
        [HttpPost]
        public async Task<IActionResult> DeleteIngredient(string id)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Ingredient");
            }

            var pizzas = await _mongoDatabase.GetCollection<Pizza>(MongoDB.PizzaCollection).Find(new BsonDocument()).ToListAsync();
            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();
            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var models = ingredients.Select(i => new IngredientViewTableModel
            {
                Id = i.Id,
                LayerName = i.Layer.Name,
                Name = i.Name,
                Price = Math.Round(i.Price, 2).ToString("0.00").Replace(",", ".")
            });

            
            if (pizzas.Any(p => p.IngredientIds.Contains(id)))
            {
                return View("Ingredient", (models, layers, "", "Unable to delete ingredient, there is existing pizza with ingredient"));

            }
            if (ingredients.All(i => i.Id != id))
            {
                return View("Ingredient", (models, layers, "", "Invalid ingredient"));
            }

            await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).DeleteOneAsync(i => i.Id == id);
 
            return View("Ingredient", (models.Where(i => i.Id != id).ToList(), layers, "", ""));
        }

        [HttpPost]
        public async Task<IActionResult> EditIngredient(string id, string price)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Ingredient");
            }

            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();
            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var models = ingredients.Select(i => new IngredientViewTableModel
            {
                Id = i.Id,
                LayerName = i.Layer.Name,
                Name = i.Name,
                Price = Math.Round(i.Price, 2).ToString("0.00").Replace(",", ".")
            }); 
            
            var priceF = float.Parse(price.Replace(".", ","));
            if (priceF < 0)
            {
                return View("Ingredient", (models, layers, "", "price can not be less then zero")); 
            }

            var ingredient = ingredients.FirstOrDefault(i => i.Id == id);

            if (ingredient == null)
            {
                return View("Ingredient", (models, layers, "", "Invalid ingredient"));
            }

            ingredient.Price = priceF;
            await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).ReplaceOneAsync(i => i.Id == id, ingredient);
            models = ingredients.Select(i => new IngredientViewTableModel
            {
                Id = i.Id,
                LayerName = i.Layer.Name,
                Name = i.Name,
                Price = Math.Round(i.Price, 2).ToString("0.00").Replace(",", ".")
            }); 
 
            return View("Ingredient", (models, layers, "", ""));
        }

        public async Task<IActionResult> Layer()
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();

            return View((layers, "", "")); 
        }


        [HttpPost]
        public async Task<IActionResult> AddLayer(Layer layer)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            layer.Name = layer.Name?.Trim()?.ToLower();
            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();

            if (string.IsNullOrEmpty(layer.Name))
            {
                return View("Layer", (layers, "Layer name can not be empty", ""));
            }

            var found = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(l => l.Name == layer.Name).AnyAsync();
            if (found)
            {
                return View("Layer", (layers, "Layer already exists", ""));
            }

            await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).InsertOneAsync(layer);
            layers.Add(layer);

            return View("Layer", (layers, "", "")); 
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLayer(string id)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();

            if (layers.All(l => l.Id != id))
            {
                return View("Layer", (layers, "", ""));
            }
            if (ingredients.Any(i => i.Layer.Id == id))
            {
                return View("Layer", (layers, "", "There is an ingredient that uses this layer, please delete all ingredients that are using this layer"));
            }

            await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).DeleteOneAsync(l => l.Id == id);

            return View("Layer", (layers.Where(l => l.Id != id), "", "")); 
        }

        public async Task<IActionResult> Pizza()
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 
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

        public async Task<IActionResult> AddPizza(Pizza pizza)
        { 
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            if (!string.IsNullOrWhiteSpace(pizza.Name) && pizza.IngredientIds.Count != 0)
            {
                await _mongoDatabase.GetCollection<Pizza>(MongoDB.PizzaCollection).InsertOneAsync(pizza);
            }

            return RedirectToAction("Pizza");
        }

        public async Task<IActionResult> AllPizzas()
        { 
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            var pizzas = await _mongoDatabase.GetCollection<Pizza>(MongoDB.PizzaCollection).Find(new BsonDocument()).ToListAsync();
            var ingredients = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).Find(new BsonDocument()).ToListAsync();

            var model = pizzas.Select(p => {
                var viewIngredients = ingredients.Where(i => p.IngredientIds.Contains(i.Id));
                var pizzaView = new PizzaViewModel
                {
                    Id = p.Id,
                    Name = p.Name, 
                    Ingredients = viewIngredients.Select(i => i.Name).ToList(),
                    Price = Math.Round(viewIngredients.Sum(i => i.Price), 2).ToString("0.00")
                };

                return pizzaView;
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePizza(string id)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            await _mongoDatabase.GetCollection<Pizza>(MongoDB.PizzaCollection).DeleteOneAsync(p => p.Id == id);

            return RedirectToAction("AllPizzas");
        }
    }

}
