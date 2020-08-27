using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCoreSQL.Models;

namespace NetCoreSQL.Controllers
{
    public class StarController : Controller
    {
        private readonly ILogger<StarController> _logger;
        private readonly IConfiguration Configuration;
        public StarController(ILogger<StarController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            List<StarWar> starWars = new List<StarWar>();
            var connectionString = Configuration["ConnectionStrings:MyDbConnection"];
            using (var conn = new SqlConnection(connectionString))
            {
                conn.AccessToken = await (new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
                await conn.OpenAsync();
                var sql = "SELECT  * FROM [dbo].[StarWars]";
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            StarWar starWar = new StarWar();
                            starWar.episode = Convert.ToInt32(reader["episode"]);
                            starWar.score = Convert.ToInt32(reader["score"]);
                            starWar.name = Convert.ToString(reader["name"]);
                            starWars.Add(starWar);
                        }
                    }

                }
            }


                return View(starWars);
        }
    }
}