using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace NetCoreAPICosmosDB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoListController : ControllerBase
    {
        private Container _container;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<TodoListController> _logger;

        public TodoListController(ILogger<TodoListController> logger)
        {
            _logger = logger;
            var account = "https://testweb.documents.azure.com:443/";
            var key = "dtPL78WoeSZkyaCdB3bOvdiP5vHpySYoPx2bIUgy24S2sPggK1lw1BLTW4rzyZHGx2cwtuJVbnMxWBwNc6kh7g==";
            var client = new CosmosClient(account, key);
            var database = client.GetDatabase("ToDoList");
            _container = database.GetContainer("Items");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task<string> AddItem(Item item) {

            _ = item.Id;

           await  _container.CreateItemAsync<Item>(item, new PartitionKey(item.Name));

            return "OK";
        
        
        }
    }
}
