using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JunimoServer.Services.CropSaver
{
    public class CropWatcher
    {

        private readonly Dictionary<Vector2, bool>  _previousHasCrop = new Dictionary<Vector2, bool>();

        private readonly Action<TerrainFeature> _onCropAdded;

        public CropWatcher(IModHelper helper, Action<TerrainFeature> onCropAdded)
        {
            helper.Events.GameLoop.UpdateTicked += GameLoopOnUpdateTicked;
            _onCropAdded = onCropAdded;
        }

        private void GameLoopOnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            var farmLocation = Game1.getLocationFromName("Farm");
            if (farmLocation == null) return;
            
            var farmTerrainFeatures = farmLocation.terrainFeatures.Values;

            foreach (var feature in farmTerrainFeatures)
            {
                var tileLoc = feature.currentTileLocation;
                var hasCrop = feature.ContainsCrop();
                if (!_previousHasCrop.ContainsKey(tileLoc))
                {
                    _previousHasCrop.Add(tileLoc, hasCrop);
                    continue;
                }

                if (hasCrop)
                {
                    if (!_previousHasCrop[tileLoc])
                    {
                        _onCropAdded(feature);
                    }
                }

                _previousHasCrop[tileLoc] = hasCrop;
            }
            
        }
    }
}