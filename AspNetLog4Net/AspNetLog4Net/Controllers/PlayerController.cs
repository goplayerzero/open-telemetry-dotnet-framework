using AspNetLog4Net.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using log4net;


namespace AspNetLog4Net.Controllers
{
    public class PlayerController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["P0SQLConnection"].ConnectionString;

        private static readonly ILog logger = LogManager.GetLogger(typeof(PlayerController));

        public List<PlayerModel> GetPlayerListFromDb()
        {
            List<PlayerModel> players = new List<PlayerModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("GetAllPlayers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PlayerModel player = new PlayerModel
                            {
                                Id = (int)reader["id"],
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString()
                            };
                            players.Add(player);
                        }
                    }
                }
            }

            return players;
        }

        public PlayerModel GetPlayerDetailsBySql(int id)
        {
            PlayerModel player = null;
            try
            {
                if (id <= 0)
                {
                    logger.Error($"ID must be a positive integer. {id}");
                    throw new ArgumentException("ID must be a positive integer.", nameof(id));
                }
                else if (id > 10)
                {
                    logger.Error($"GetPlayerDetailsBySql - You are requesting for {id}, it should be less than or equal to 10");
                }

                string query = "SELECT Id, Name, Description FROM Players WHERE Id = @PlayerId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@PlayerId", id);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                player = new PlayerModel
                                {
                                    Id = (int)reader["Id"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                logger.Error($"ArgumentException in GetPlayerDetailsBySql: {ex}");
                throw new Exception($"ArgumentException in GetPlayerDetailsBySql: {ex}");
            }
            catch (SqlException ex)
            {
                logger.Error($"Database error in GetPlayerDetailsBySql: {ex}");
                throw new Exception($"Database error in GetPlayerDetailsBySql: {ex}");
            }
            catch (Exception ex)
            {
                logger.Error($"GetPlayerDetailsBySql: {ex}");
                throw new Exception($"GetPlayerDetailsBySql: {ex}");
            }

            return player;
        }

        public PlayerModel GetPlayerDetailsFromDb(int id)
        {
            if (id <= 0)
            {
                logger.Error($"GetPlayerDetailsFromDb ID must be a positive integer. {id}");
                throw new ArgumentException("ID must be a positive integer.", nameof(id));
            }   

            PlayerModel player = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var storeProcName = id > 10 ? "NotFoundQuery" : "GetPlayerDetails";
                
                using (SqlCommand command = new SqlCommand(storeProcName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PlayerId", id);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            player = new PlayerModel
                            {
                                Id = (int)reader["id"],
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString()
                            };
                        }
                    }
                }
            }

            return player;
        }

        // GET: Player
        public ActionResult Index()
        {
            var list = GetPlayerListFromDb();
            ViewBag.PlayerList = list;
            ViewBag.PlayerDetails = new PlayerModel();

            var playerId = 23;
            var userService = new UserService();

            var user = userService.GetUser(playerId);

            var playerDetails = GetPlayerDetailsBySql(playerId);
            ViewBag.PlayerDetails = playerDetails;
            return View();
        }

        [HttpPost]
        public ActionResult GetPlayerDetails(int playerId)
        {
            var list = GetPlayerListFromDb();
            ViewBag.PlayerList = list;
            logger.Warn($"Current Trace ID {Activity.Current.TraceId}");

            var userService = new UserService();

            var user = userService.GetUser(playerId);

            var playerDetails = GetPlayerDetailsBySql(playerId);
            ViewBag.PlayerDetails = playerDetails;
            return View("Index");
        }
    }
}