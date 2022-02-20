using System;
using System.Collections.Generic;
using System.Linq;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace JunimoServer.Services.Commands
{
    public static class CabinCommand
    {
        public static void Register(IModHelper helper, IChatCommandApi chatCommandApi,
            PersistentOptions.PersistentOptions options, IMonitor monitor)
        {
            chatCommandApi.RegisterCommand("cabin",
                "!cabin plank|log|stone places a cabin right of player's location.\nThis will clear basic debris to make space.",
                (args, msg) =>
                {
                    var numExistingCabins =
                        helper.Reflection.GetMethod(Game1.server, "cabins")
                            .Invoke<IEnumerable<Cabin>>().ToList().Count;
                    var numTotalCabins = options.Data.MaxPlayers;

                    monitor.Log($"\nnumExistingCabins: {numExistingCabins}\nnumTotalCabins: {numTotalCabins}");

                    if (numExistingCabins >= numTotalCabins)
                    {
                        helper.SendPublicMessage($"Build Failed: Player/Cabin limit of {numTotalCabins} reached.");
                        return;
                    }

                    string[] cabinTypes = {"Plank", "Log", "Stone"};
                    var cabinType = cabinTypes[new Random().Next(cabinTypes.Length)];
                    var cabinChoice = args.Length > 0 ? args[0].ToLower() : "";

                    cabinType = cabinChoice switch
                    {
                        "log" => "Log",
                        "stone" => "Stone",
                        "plank" => "Plank",
                        _ => cabinType
                    };
                    
                    monitor.Log($"\ncabinChoice: {cabinChoice}\ncabinType: {cabinType}");


                    var fromPlayer = Game1.getOnlineFarmers()
                        .First(farmer => farmer.UniqueMultiplayerID == msg.SourceFarmer);

                    var cabinBlueprint = new BluePrint($"{cabinType} Cabin");
                    var cabinLocation = new Vector2(fromPlayer.getTileX() + 1, fromPlayer.getTileY());

                    var isBuildableArea = true;
                    for (var x = 0; x < cabinBlueprint.tilesWidth; x++)
                    {
                        for (var y = 0; y < cabinBlueprint.tilesHeight; y++)
                        {
                            var currentTileLocation = new Vector2(cabinLocation.X + x, cabinLocation.Y + y);

                            fromPlayer.currentLocation.terrainFeatures.Remove(currentTileLocation);
                            fromPlayer.currentLocation.objects.Remove(currentTileLocation);

                            if (!Game1.getFarm().isBuildable(currentTileLocation))
                            {
                                isBuildableArea = false;
                            }
                        }
                    }

                    var shouldSkipSafetyCheck = isBuildableArea;

                    if (Game1.getFarm().buildStructure(cabinBlueprint, cabinLocation, Game1.player,
                            skipSafetyChecks: shouldSkipSafetyCheck))
                    {
                        Game1.getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
                        helper.SendPublicMessage(
                            $"Build Succeeded: Cabin {numExistingCabins + 1} of {numTotalCabins} constructed.");
                    }
                    else
                    {
                        helper.SendPublicMessage("Build Failed: Area not clear.");
                    }

                }
            );
        }
    }
}