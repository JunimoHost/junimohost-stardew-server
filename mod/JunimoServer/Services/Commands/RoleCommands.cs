using System.Linq;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Util;
using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.Roles
{
    public class RoleCommands
    {
        public static void Register(IModHelper helper, IChatCommandApi chatCommandApi, RoleService roleService)
        {
            chatCommandApi.RegisterCommand("admin", "\"farmerName\" to assign admin status to the player.", (args, msg) =>
            {
                if (!roleService.IsPlayerAdmin(msg.SourceFarmer))
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "You are not an admin.");
                    return;
                }
                if (args.Length != 1)
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "Invalid use of command. Correct format is !admin farmer");
                    return;
                }

                var farmerToAdmin = Game1.getAllFarmers().FirstOrDefault(farmer => farmer.Name == args[1]);

                if (farmerToAdmin == null)
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "Player not found: " + args[1]);
                    return;
                }
                
                roleService.AssignAdmin(farmerToAdmin.UniqueMultiplayerID);
                helper.SendPrivateMessage(msg.SourceFarmer, "Assigned Admin to: " + farmerToAdmin.Name);
            });
            chatCommandApi.RegisterCommand("unadmin", "\"farmerName\" to take away admin status from the player.", (args, msg) =>
            {
                if (!roleService.IsPlayerAdmin(msg.SourceFarmer))
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "You are not an admin.");
                    return;
                }
                
                if (args.Length != 1)
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "Invalid use of command. Correct format is !unadmin farmer");
                    return;
                }

                var farmerToUnadmin = Game1.getAllFarmers().FirstOrDefault(farmer => farmer.Name == args[1]);

                if (farmerToUnadmin == null)
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "Player not found: " + args[1]);
                    return;
                }

                if (farmerToUnadmin.UniqueMultiplayerID == helper.GetOwnerPlayerId())
                {
                    helper.SendPrivateMessage(msg.SourceFarmer, "You can't unadmin the owner of the server.");
                    return;
                }

                roleService.UnassignAdmin(farmerToUnadmin.UniqueMultiplayerID);
                helper.SendPrivateMessage(msg.SourceFarmer, "Took away admin from: " + farmerToUnadmin.Name);
            });
        }
    }
}