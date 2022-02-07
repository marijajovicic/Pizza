using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
