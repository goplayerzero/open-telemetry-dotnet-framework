using AspdotNet481.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Diagnostics;
using System;
using Microsoft.Extensions.Logging;

namespace AspdotNet481.Controllers
{
    public class PlayerController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["P0SQLConnection"].ConnectionString;
        
        public List<PlayerModel> GetPlayerListFromDb()
        {
            var _logger = TelemetryService.CreateLogger("PlayerController");

            _logger.LogError("GetPlayerListFromDb");

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

        public PlayerModel GetPlayerDetailsFromDb(int id)
        {
            var _logger = TelemetryService.CreateLogger("PlayerController");

            if (id <= 0)
            {
                throw new ArgumentException("ID must be a positive integer.", nameof(id));
            }
                

            PlayerModel player = null;

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    var storeProcName = id > 10 ? "NotFoundQuery" : "GetPlayerDetails";

            //    Debug.WriteLine($"Current Trace ID {Activity.Current.TraceId}");

            //    using (SqlCommand command = new SqlCommand(storeProcName, connection))
            //    {
            //        command.CommandType = CommandType.StoredProcedure;
            //        command.Parameters.AddWithValue("@PlayerId", id);

            //        connection.Open();
            //        using (SqlDataReader reader = command.ExecuteReader())
            //        {
            //            if (reader.Read())
            //            {
            //                player = new PlayerModel
            //                {
            //                    Id = (int)reader["id"],
            //                    Name = reader["name"].ToString(),
            //                    Description = reader["description"].ToString()
            //                };
            //            }
            //        }
            //    }
            //}

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var storeProcName = id > 10 ? "NotFoundQuery" : "GetPlayerDetails";

                    Debug.WriteLine($"Current Trace ID {Activity.Current.TraceId}");

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
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Exception: {sqlEx.Message}");
                Debug.WriteLine($"SQL Exception: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                Debug.WriteLine($"Exception: {ex.Message}");
            }

            return player;
        }

        // GET: Player
        public ActionResult Index()
        {
            var list = GetPlayerListFromDb();
            ViewBag.PlayerList = list;
            ViewBag.PlayerDetails = new PlayerModel();
            return View();
        }

        [HttpPost]
        public ActionResult GetPlayerDetails(int playerId)
        {
            var list = GetPlayerListFromDb();
            ViewBag.PlayerList = list;

            var playerDetails = GetPlayerDetailsFromDb(playerId);
            ViewBag.PlayerDetails = playerDetails;
            return View("Index");
        }
    }
}