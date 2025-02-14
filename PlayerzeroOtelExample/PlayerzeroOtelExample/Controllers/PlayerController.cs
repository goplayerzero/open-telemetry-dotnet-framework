using PlayerzeroOtelExample.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PlayerzeroOtelExample.Controllers
{
    [RoutePrefix("api/player")]
    public class PlayerController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["P0SQLConnection"].ConnectionString;

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

        public PlayerModel GetPlayerDetailsFromDb(int id)
        {

            if (id <= 0)
            {
                throw new ArgumentException("ID must be a positive integer.", nameof(id));
            }


            PlayerModel player = null;

            try
            {
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
            }
            catch (SqlException sqlEx)
            {
                Debug.WriteLine($"SQL Exception: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }

            return player;
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetPlayer(int id)
        {
            var player = GetPlayerDetailsFromDb(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }
    }
}
