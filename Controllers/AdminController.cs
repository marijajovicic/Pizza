﻿using Microsoft.AspNetCore.Mvc;
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

            return View("Ingredient", (ingredients, layers, ""));
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

            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                return View("Ingredient", (ingredients, layers, "Name can not be empty"));
            } 
            if (ingredient.Price <= 0)
            {
                return View("Ingredient", (ingredients, layers, "Price must be greater than zero"));
            }
            var layer = layers.FirstOrDefault(l => l.Id == ingredient.LayerId);
            if (layer == null)
            {
                return View("Ingredient", (ingredients, layers, "Layer does not exists"));
            } 

            var ingredientCreate = new Ingredient
            {
               Layer = layer,
               Name = ingredient.Name,
               Price = ingredient.Price
            };
            await _mongoDatabase.GetCollection<Ingredient>(MongoDB.IngredientCollection).InsertOneAsync(ingredientCreate);

            return View("Ingredient", (ingredients, layers,""));
        }
 
        [HttpDelete]
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
            var layers = await _mongoDatabase.GetCollection<Ingredient>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();
            
            if (pizzas.Any(p => p.IngredientIds.Contains(id)))
            {
                return View("Ingredient", (ingredients, layers, "", "Unable to delete pizza, there is existing pizza with ingredient"));

            }

            return View("Ingredient", (ingredients, layers, ""));
        }

        public async Task<IActionResult> Layer()
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            }

            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();

            return View((layers, "")); 
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
                return View("Layer", (layers, "Layer name can not be empty"));
            }

            var found = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(l => l.Name == layer.Name).AnyAsync();
            if (found)
            {
                return View("Layer", (layers, "Layer already exists"));
            }

            await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).InsertOneAsync(layer);
            layers.Add(layer);

            return View("Layer", (layers, "")); 
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLayer(string id)
        {
            if (SessionHelper.IsUsernameEmpty(HttpContext.Session))
            {
                return RedirectToAction("Index");
            } 

            var layers = await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).Find(new BsonDocument()).ToListAsync();

            if (layers.Any(l => l.Id == id))
            {
                return View("Layer", (layers, ""));
            }

            await _mongoDatabase.GetCollection<Layer>(MongoDB.LayerCollection).DeleteOneAsync(l => l.Id == id);

            return View("Layer", (layers.Where(l => l.Id != id), "")); 
        }

    }

}
