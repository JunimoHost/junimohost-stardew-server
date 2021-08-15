using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoServer.Services.GameLoader
{
    class GameLoaderService
    {
        private static readonly string SAVE_KEY = "JunimoHost.GameLoader";

        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly GameLoaderSaveData saveData;
        public GameLoaderService(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            saveData = helper.Data.ReadGlobalData<GameLoaderSaveData>(SAVE_KEY) ?? new GameLoaderSaveData();
        }

        public bool HasLoadableSave()
        {
            string saveName = saveData.SaveNameToLoad;

            if (saveName == null)
            {
                return false;
            }

            string savePath = GetSavePath(saveName);
            bool saveExists = Directory.Exists(savePath);

            return saveExists;

        }

        public bool LoadSave()
        {
            string saveName = saveData.SaveNameToLoad;
            string savePath = GetSavePath(saveName);
            bool saveExists = Directory.Exists(savePath);
            if (!saveExists)
            {
                monitor.Log($"{savePath} does not exist. Aborting load.", LogLevel.Warn);
                return false;
            }

            Game1.multiplayerMode = 2;

            try
            {
                monitor.Log($"Loading Save: {saveName}", LogLevel.Info);
                SaveGame.Load(saveName);
                if (Game1.activeClickableMenu is IClickableMenu menu && menu != null)
                {
                    menu.exitThisMenu(false);
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Load Failed: {ex.Message} ", LogLevel.Error);
                return false;
            }

            return true;

        }

        public void SetSaveNameToLoad(string saveName)
        {
            saveData.SaveNameToLoad = saveName;
            helper.Data.WriteGlobalData(SAVE_KEY, saveData);
            monitor.Log($"Save set to load: {saveName}");
        }

        private string GetSavePath(string saveName)
        {
            return Path.Combine(Constants.SavesPath, saveName);
        }
    }
}
