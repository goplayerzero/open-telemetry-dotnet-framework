using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Serilog;

namespace DataAccess
{
    public class PlayerService
    {
        public PlayerService()
        {
        }

        private readonly string connectionString = "Server=localhost;Database=playerzero;Integrated Security=True;";

        public Player GetPlayerDetailsBySp(int id)
        {
            Player player = null;
            try
            {
                if (id <= 0)
                {

                    Log.Error($"ID must be a positive integer. {id}");
                    throw new ArgumentException("ID must be a positive integer.", nameof(id));
                }
                else if (id > 10)
                {
                    Log.Error($"GetPlayerDetailsBySp - You are requesting for {id}, it should be less than or equal to 10");
                }

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
                                player = new Player
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
            catch (Exception e)
            {
                Log.Error($"Exception in GetPlayerDetailsBySp: {e}");
                throw new Exception($"Exception in GetPlayerDetailsBySp {e}");
            }

            return player;
        }

        public Player GetPlayerDetailsBySql(int id)
        {
            Player player = null;
            try
            {
                if (id <= 0)
                {
                    Log.Error($"ID must be a positive integer. {id}");
                    throw new ArgumentException("ID must be a positive integer.", nameof(id));
                }
                else if (id > 10)
                {
                    Log.Error($"GetPlayerDetailsBySql - You are requesting for {id}, it should be less than or equal to 10");
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
                                player = new Player
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
                Log.Error($"ArgumentException in GetPlayerDetailsBySql: {ex}");
                throw new Exception($"ArgumentException in GetPlayerDetailsBySql: {ex}");
            }
            catch (SqlException ex)
            {
                Log.Error($"Database error in GetPlayerDetailsBySql: {ex}");
                throw new Exception($"Database error in GetPlayerDetailsBySql: {ex}");
            }
            catch (Exception ex)
            {
                Log.Error($"GetPlayerDetailsBySql: {ex}");
                throw new Exception($"GetPlayerDetailsBySql: {ex}");
            }

            return player;
        }

        public List<Player> GetAllPlayers()
        {
            List<Player> players = new List<Player>();

            try
            {
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
                                Player player = new Player
                                {
                                    Id = (int)reader["Id"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                };

                                players.Add(player);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Log.Error($"Database error in GetAllPlayers: {ex}");
                throw new Exception($"Database error in GetAllPlayers: {ex}");
            }
            catch (Exception ex)
            {
                Log.Error($"GetAllPlayers: {ex}");
                throw new Exception($"GetAllPlayers: {ex}");
            }

            return players;
        }

    }
}
