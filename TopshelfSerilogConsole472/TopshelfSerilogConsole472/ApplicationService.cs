using DataAccess;
using Serilog;
using System;
using PZConfigurations;

namespace TopshelfSerilogConsole472
{
    public class ApplicationService
    {
        public void Start()
        {
            try
            {
                Log.Information("Service Started...");

                var userService = new UserService();
                var user = userService.GetUser(2);

                Log.Warning($"User : {user}");

                var playerService = new PlayerService();
                var playerSp = playerService.GetPlayerDetailsBySp(2);

                Log.Information($"Player name {playerSp.Name}");

                var playerSql = playerService.GetPlayerDetailsBySql(1);

                var list = playerService.GetAllPlayers();

            }
            catch (Exception ex)
            {
                Log.Error("Main", ex.Message);
            }
        }

        public bool Stop()
        {
            Log.Information("Service Stopped...");
            PZConfigurations.PZConfigurations.GetInstance.Dispose();
            return true;
        }
    }
}
