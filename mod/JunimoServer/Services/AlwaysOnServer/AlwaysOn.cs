using System.Collections.Generic;
using System.Linq;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Services.ServerOptim;
using JunimoServer.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace JunimoServer.Services.AlwaysOnServer
{
    public class AlwaysOnServer
    {

        private const string StartNowText = "Type !event to start now";
        /// <summary>The mod configuration from the player.</summary>
        private AlwaysOnConfig Config;

        /// <summary>Whether the main player is currently being automated.</summary>
        private bool IsAutomating;

        public bool clientPaused;

        //debug tools
        private bool shippingMenuActive;

        private bool eventCommandUsed;

        private bool eggHuntAvailable; //is egg festival ready start timer for triggering eggHunt Event
        private int eggHuntCountDown; //to trigger egg hunt after set time

        private bool flowerDanceAvailable;
        private int flowerDanceCountDown;

        private bool luauSoupAvailable;
        private int luauSoupCountDown;

        private bool jellyDanceAvailable;
        private int jellyDanceCountDown;

        private bool grangeDisplayAvailable;
        private int grangeDisplayCountDown;

        private bool goldenPumpkinAvailable;
        private int goldenPumpkinCountDown;

        private bool iceFishingAvailable;
        private int iceFishingCountDown;

        private bool winterFeastAvailable;

        private int winterFeastCountDown;

        // Festivals
        private static readonly SDate eggFestival = new SDate(13, "spring");
        private static readonly SDate flowerDance = new SDate(24, "spring");
        private static readonly SDate luau = new SDate(11, "summer");
        private static readonly SDate danceOfJellies = new SDate(28, "summer");
        private static readonly SDate stardewValleyFair = new SDate(16, "fall");
        private static readonly SDate spiritsEve = new SDate(27, "fall");
        private static readonly SDate festivalOfIce = new SDate(8, "winter");
        private static readonly SDate feastOfWinterStar = new SDate(25, "winter");
        private static readonly SDate grampasGhost = new SDate(1, "spring", 3);

        private static readonly SDate[] Festivals =
        {
            eggFestival, flowerDance, luau, danceOfJellies, stardewValleyFair, spiritsEve, festivalOfIce,
            feastOfWinterStar, grampasGhost
        };
        ///////////////////////////////////////////////////////


        //variables for timeout reset code

        private int timeOutTicksForReset;
        private int festivalTicksForReset;
        private int shippingMenuTimeoutTicks;


        private readonly IModHelper _helper;
        private readonly IChatCommandApi _chatCommandApi;
        private readonly IMonitor _monitor;
        private readonly ServerOptimizer _optimizer;
        private readonly bool _disableRendering;


        public AlwaysOnServer(IModHelper helper, IMonitor monitor, IChatCommandApi chatCommandApi,
            ServerOptimizer optimizer, bool disableRendering)
        {
            _helper = helper;
            _monitor = monitor;
            _chatCommandApi = chatCommandApi;
            _optimizer = optimizer;
            _disableRendering = disableRendering;
            Config = helper.ReadConfig<AlwaysOnConfig>();


            helper.ConsoleCommands.Add("server", "Toggles headless 'auto mode' on/off", this.ToggleAutoMode);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving; // Shipping Menu handler
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked; //game tick event handler
            helper.Events.GameLoop.TimeChanged += OnTimeChanged; // Time of day change handler
            helper.Events.GameLoop.UpdateTicked +=
                OnUpdateTicked; //handles various events that should occur as soon as they are available
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Specialized.UnvalidatedUpdateTicked +=
                OnUnvalidatedUpdateTick; //used bc only thing that gets throug save window
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            _chatCommandApi.RegisterCommand("event", "Tries to start the current festival's event.", StartEvent);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (_disableRendering)
            {
                _optimizer.DisableDrawing();
            }
        }


        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.IsServer)
            {
                TurnOnAutoMode();
            }
        }

        //draw textbox rules
        private static void DrawTextBox(int x, int y, SpriteFont font, string message, int align = 0,
            float colorIntensity = 1f)
        {
            SpriteBatch spriteBatch = Game1.spriteBatch;
            int width = (int) font.MeasureString(message).X + 32;
            int num = (int) font.MeasureString(message).Y + 21;
            switch (align)
            {
                case 0:
                    IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y,
                        width, num + 4, Color.White * colorIntensity);
                    Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2(x + 16, y + 16),
                        Game1.textColor);
                    break;
                case 1:
                    IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                        x - width / 2, y, width, num + 4, Color.White * colorIntensity);
                    Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2(x + 16 - width / 2, y + 16),
                        Game1.textColor);
                    break;
                case 2:
                    IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                        x - width, y, width, num + 4, Color.White * colorIntensity);
                    Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2(x + 16 - width, y + 16),
                        Game1.textColor);
                    break;
            }
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            //draw a textbox in the top left corner saying Server On
            if (Game1.options.enableServer && IsAutomating)
            {
                DrawTextBox(5, 100, Game1.dialogueFont, "Auto Mode On");
                DrawTextBox(5, 180, Game1.dialogueFont, $"Press {Config.ServerHotKey} On/Off");
                DrawTextBox(5, 340, Game1.dialogueFont, $"{Game1.server.connectionsCount} Players Online");
            }
        }


        // toggles auto mode on/off with console command "server"
        private void ToggleAutoMode(string command, string[] args)
        {
            ToggleAutoMode();
        }

        private void ToggleAutoMode()

        {
            if (!Context.IsWorldReady) return;
            if (!IsAutomating)
            {
                TurnOnAutoMode();
            }
            else
            {
                TurnOffAutoMode();
            }
        }

        private void TurnOffAutoMode()
        {
            IsAutomating = false;
            Game1.warpFarmer("Farmhouse", 0, 0, false);
            _monitor.Log("Auto mode off!", LogLevel.Info);

            Game1.chatBox.addInfoMessage("The host has returned!");

            Game1.displayHUD = true;
            Game1.addHUDMessage(new HUDMessage("Auto Mode Off!", ""));
            if (_disableRendering)
            {
                _optimizer.EnableDrawing();
            }
        }

        private void TurnOnAutoMode()
        {
            Config = _helper.ReadConfig<AlwaysOnConfig>();
            IsAutomating = true;


            _monitor.Log("Auto mode on!", LogLevel.Info);
            Game1.chatBox.addInfoMessage("The host is in automatic mode!");

            Game1.displayHUD = true;
            Game1.addHUDMessage(new HUDMessage("Auto Mode On!", ""));


            HideFarmer();

            if (_disableRendering)
            {
                _optimizer.DisableDrawing();
            }
            // set in game levels to max (leaving out, not sure why this was needed)
            // Game1.player.FarmingLevel = 10;
            // Game1.player.MiningLevel = 10;
            // Game1.player.ForagingLevel = 10;
            // Game1.player.FishingLevel = 10;
            // Game1.player.CombatLevel = 10;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //toggles server on/off with configurable hotkey
            if (Context.IsWorldReady)
            {
                if (e.Button == this.Config.ServerHotKey)
                {
                    ToggleAutoMode();
                }
            }
        }


        /// <summary>Raised once per second after the game state is updated.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // pause if no players present
            if (this.IsAutomating)
                this.PauseIfNobodyPresent();
            else
                Game1.paused = false;


            //left click menu spammer and event skipper to get through random events happening
            //also moves player around, this seems to free host from random bugs sometimes
            if (IsAutomating)
            {
                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
                {
                    Game1.activeClickableMenu.receiveLeftClick(10, 10);
                }

                if (Game1.CurrentEvent != null && Game1.CurrentEvent.skippable)
                {
                    Game1.CurrentEvent.skipEvent();
                }
            }


            // automate events
            if (this.IsAutomating)
            {
                // egg-hunt event
                if (eggHuntAvailable && Game1.CurrentEvent is {isFestival: true})
                {
                    if (eventCommandUsed)
                    {
                        eggHuntCountDown = this.Config.EggHuntCountDownConfig;
                        eventCommandUsed = false;
                    }

                    eggHuntCountDown += 1;

                    float chatEgg = this.Config.EggHuntCountDownConfig / 60f;
                    if (eggHuntCountDown == 1)
                    {
                        _helper.SendPublicMessage($"The Egg Hunt will begin in {chatEgg:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);
                    }

                    if (eggHuntCountDown == this.Config.EggHuntCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (eggHuntCountDown >= this.Config.EggHuntCountDownConfig + 5)
                    {
                        if (Game1.activeClickableMenu != null)
                        {
                            //_helper.Reflection.GetMethod(Game1.activeClickableMenu, "receiveLeftClick").Invoke(10, 10, true);
                        }

                        //festival timeout
                        festivalTicksForReset += 1;
                        if (festivalTicksForReset >= this.Config.EggFestivalTimeOut + 180)
                        {
                            Game1.options.setServerMode("offline");
                        }
                        ///////////////////////////////////////////////
                    }
                }

                // flower dance event
                if (flowerDanceAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    if (eventCommandUsed)
                    {
                        flowerDanceCountDown = this.Config.FlowerDanceCountDownConfig;
                        eventCommandUsed = false;
                    }

                    flowerDanceCountDown += 1;

                    float chatFlower = this.Config.FlowerDanceCountDownConfig / 60f;
                    if (flowerDanceCountDown == 1)
                    {
                        _helper.SendPublicMessage($"The Flower Dance will begin in {chatFlower:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);
                    }

                    if (flowerDanceCountDown == this.Config.FlowerDanceCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (flowerDanceCountDown >= this.Config.FlowerDanceCountDownConfig + 5)
                    {
                        if (Game1.activeClickableMenu != null)
                        {
                            // _helper.Reflection.GetMethod(Game1.activeClickableMenu, "receiveLeftClick").Invoke(10, 10, true);
                        }

                        //festival timeout
                        festivalTicksForReset += 1;
                        if (festivalTicksForReset >= this.Config.FlowerDanceTimeOut + 90)
                        {
                            Game1.options.setServerMode("offline");
                        }
                        ///////////////////////////////////////////////
                    }
                }

                //luauSoup event
                if (luauSoupAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    if (eventCommandUsed)
                    {
                        luauSoupCountDown = this.Config.LuauSoupCountDownConfig;
                        //add iridium starfruit to soup
                        var item = new SObject(268, 1, false, -1, 3);
                        _helper.Reflection.GetMethod(new Event(), "addItemToLuauSoup").Invoke(item, Game1.player);
                        eventCommandUsed = false;
                    }

                    luauSoupCountDown += 1;

                    float chatSoup = this.Config.LuauSoupCountDownConfig / 60f;
                    if (luauSoupCountDown == 1)
                    {
                        _helper.SendPublicMessage($"The Soup Tasting will begin in {chatSoup:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);

                        //add iridium starfruit to soup
                        var item = new SObject(268, 1, false, -1, 3);
                        _helper.Reflection.GetMethod(new Event(), "addItemToLuauSoup").Invoke(item, Game1.player);
                    }

                    if (luauSoupCountDown == this.Config.LuauSoupCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (luauSoupCountDown >= this.Config.LuauSoupCountDownConfig + 5)
                    {
                        if (Game1.activeClickableMenu != null)
                        {
                            //_helper.Reflection.GetMethod(Game1.activeClickableMenu, "receiveLeftClick").Invoke(10, 10, true);
                        }

                        //festival timeout
                        festivalTicksForReset += 1;
                        if (festivalTicksForReset >= this.Config.LuauTimeOut + 80)
                        {
                            Game1.options.setServerMode("offline");
                        }
                        ///////////////////////////////////////////////
                    }
                }

                //Dance of the Moonlight Jellies event
                if (jellyDanceAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    if (eventCommandUsed)
                    {
                        jellyDanceCountDown = this.Config.JellyDanceCountDownConfig;
                        eventCommandUsed = false;
                    }

                    jellyDanceCountDown += 1;

                    float chatJelly = this.Config.JellyDanceCountDownConfig / 60f;
                    if (jellyDanceCountDown == 1)
                    {
                        _helper.SendPublicMessage(
                            $"The Dance of the Moonlight Jellies will begin in {chatJelly:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);

                    }

                    if (jellyDanceCountDown == this.Config.JellyDanceCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (jellyDanceCountDown >= this.Config.JellyDanceCountDownConfig + 5)
                    {
                        if (Game1.activeClickableMenu != null)
                        {
                            // _helper.Reflection.GetMethod(Game1.activeClickableMenu, "receiveLeftClick").Invoke(10, 10, true);
                        }

                        //festival timeout
                        festivalTicksForReset += 1;
                        if (festivalTicksForReset >= this.Config.DanceOfJelliesTimeOut + 180)
                        {
                            Game1.options.setServerMode("offline");
                        }
                        ///////////////////////////////////////////////
                    }
                }

                //Grange Display event
                if (grangeDisplayAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    if (eventCommandUsed)
                    {
                        grangeDisplayCountDown = this.Config.GrangeDisplayCountDownConfig;
                        eventCommandUsed = false;
                    }

                    grangeDisplayCountDown += 1;
                    festivalTicksForReset += 1;
                    //festival timeout code
                    if (festivalTicksForReset == this.Config.FairTimeOut - 120)
                    {
                        _helper.SendPublicMessage("2 minutes to the exit or");
                        _helper.SendPublicMessage("everyone will be kicked.");
                    }

                    if (festivalTicksForReset >= this.Config.FairTimeOut)
                    {
                        Game1.options.setServerMode("offline");
                    }

                    ///////////////////////////////////////////////
                    float chatGrange = this.Config.GrangeDisplayCountDownConfig / 60f;
                    if (grangeDisplayCountDown == 1)
                    {
                        _helper.SendPublicMessage($"The Grange Judging will begin in {chatGrange:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);

                    }

                    if (grangeDisplayCountDown == this.Config.GrangeDisplayCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (grangeDisplayCountDown == this.Config.GrangeDisplayCountDownConfig + 5)
                        this.LeaveFestival();
                }

                //golden pumpkin maze event
                if (goldenPumpkinAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    goldenPumpkinCountDown += 1;
                    festivalTicksForReset += 1;
                    //festival timeout code
                    if (festivalTicksForReset == this.Config.SpiritsEveTimeOut - 120)
                    {
                        _helper.SendPublicMessage("2 minutes to the exit or");
                        _helper.SendPublicMessage("everyone will be kicked.");
                    }

                    if (festivalTicksForReset >= this.Config.SpiritsEveTimeOut)
                    {
                        Game1.options.setServerMode("offline");
                    }

                    ///////////////////////////////////////////////
                    if (goldenPumpkinCountDown == 10)
                        this.LeaveFestival();
                }

                //ice fishing event
                if (iceFishingAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    if (eventCommandUsed)
                    {
                        iceFishingCountDown = this.Config.IceFishingCountDownConfig;
                        eventCommandUsed = false;
                    }

                    iceFishingCountDown += 1;

                    float chatIceFish = this.Config.IceFishingCountDownConfig / 60f;
                    if (iceFishingCountDown == 1)
                    {
                        _helper.SendPublicMessage($"The Ice Fishing Contest will begin in {chatIceFish:0.#} minutes.");
                        _helper.SendPublicMessage(StartNowText);

                    }

                    if (iceFishingCountDown == this.Config.IceFishingCountDownConfig + 1)
                    {
                        _helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion")
                            .Invoke(Game1.getCharacterFromName("Lewis"), "yes");
                    }

                    if (iceFishingCountDown >= this.Config.IceFishingCountDownConfig + 5)
                    {
                        if (Game1.activeClickableMenu != null)
                        {
                            //_helper.Reflection.GetMethod(Game1.activeClickableMenu, "receiveLeftClick").Invoke(10, 10, true);
                        }

                        //festival timeout
                        festivalTicksForReset += 1;
                        if (festivalTicksForReset >= this.Config.FestivalOfIceTimeOut + 180)
                        {
                            Game1.options.setServerMode("offline");
                        }
                        ///////////////////////////////////////////////
                    }
                }

                //Feast of the Winter event
                if (winterFeastAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
                {
                    winterFeastCountDown += 1;
                    festivalTicksForReset += 1;
                    //festival timeout code
                    if (festivalTicksForReset == this.Config.WinterStarTimeOut - 120)
                    {
                        _helper.SendPublicMessage("2 minutes to the exit or");
                        _helper.SendPublicMessage("everyone will be kicked.");
                    }

                    if (festivalTicksForReset >= this.Config.WinterStarTimeOut)
                    {
                        Game1.options.setServerMode("offline");
                    }

                    ///////////////////////////////////////////////
                    if (winterFeastCountDown == 10)
                        this.LeaveFestival();
                }
            }

            // Skip level up menu
            if (IsAutomating && Game1.activeClickableMenu is LevelUpMenu menu)
            {
                // Taken from LevelUpMenu.cs:504
                _monitor.Log("Skipping level up menu");
                menu.isActive = false;
                menu.informationUp = false;
                menu.isProfessionChooser = false;
                menu.RemoveLevelFromLevelList();
            }
        }

        //Pause game if no clients Code
        private void PauseIfNobodyPresent()
        {
            var numPlayers = Game1.otherFarmers.Count;
            var currentDate = SDate.Now();
            var isFestivalDay = Festivals.Contains(currentDate);

            if (numPlayers >= 1)
            {
                if (clientPaused)
                    Game1.netWorldState.Value.IsPaused = true;
                else
                    Game1.paused = false;
            }
            else if (numPlayers == 0 && Game1.timeOfDay is >= 610 and <= 2500 && !isFestivalDay)
            {
                Game1.paused = true;
            }
        }

        private void StartEvent(string[] args, ReceivedMessage msg)
        {
            var currentDate = SDate.Now();

            if (Game1.CurrentEvent is not {isFestival: true})
            {
                _helper.SendPublicMessage("Must be at festival to start the event.");
                return;
            }

            if (currentDate == eggFestival)
            {
                eventCommandUsed = true;
                eggHuntAvailable = true;
            }
            else if (currentDate == flowerDance)
            {
                eventCommandUsed = true;
                flowerDanceAvailable = true;
            }
            else if (currentDate == luau)
            {
                eventCommandUsed = true;
                luauSoupAvailable = true;
            }
            else if (currentDate == danceOfJellies)
            {
                eventCommandUsed = true;
                jellyDanceAvailable = true;
            }
            else if (currentDate == stardewValleyFair)
            {
                eventCommandUsed = true;
                grangeDisplayAvailable = true;
            }
            else if (currentDate == spiritsEve)
            {
                eventCommandUsed = true;
                goldenPumpkinAvailable = true;
            }
            else if (currentDate == festivalOfIce)
            {
                eventCommandUsed = true;
                iceFishingAvailable = true;
            }
            else if (currentDate == feastOfWinterStar)
            {
                eventCommandUsed = true;
                winterFeastAvailable = true;
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            HandleAutoSleep();
            HandleAutoLeaveFestival();

            // lock player chests
            if (Config.LockPlayerChests)
            {
                LockPlayersChests();
            }

            // automate choices
            if (IsAutomating)
            {
                //petchoice
                if (!Game1.player.hasPet())
                {
                    _helper.Reflection.GetMethod(new Event(), "namePet").Invoke(this.Config.PetName.Substring(0));
                }

                if (Game1.player.hasPet() && Game1.getCharacterFromName(Game1.player.getPetName()) is Pet pet)
                {
                    pet.Name = this.Config.PetName.Substring(0);
                    pet.displayName = this.Config.PetName.Substring(0);
                }

                //cave choice unlock 
                if (!Game1.player.eventsSeen.Contains(65))
                {
                    Game1.player.eventsSeen.Add(65);


                    if (this.Config.FarmCaveChoiceIsMushrooms)
                    {
                        Game1.MasterPlayer.caveChoice.Value = 2;
                        (Game1.getLocationFromName("FarmCave") as FarmCave).setUpMushroomHouse();
                    }
                    else
                    {
                        Game1.MasterPlayer.caveChoice.Value = 1;
                    }
                }

                //community center unlock
                if (!Game1.player.eventsSeen.Contains(611439))
                {
                    Game1.player.eventsSeen.Add(611439);
                    Game1.MasterPlayer.mailReceived.Add("ccDoorUnlock");
                }

                if (this.Config.UpgradeHouse != 0 && Game1.player.HouseUpgradeLevel != this.Config.UpgradeHouse)
                {
                    Game1.player.HouseUpgradeLevel = this.Config.UpgradeHouse;
                }

                // just turns off auto mod if the game gets exited back to title screen
                if (Game1.activeClickableMenu is TitleMenu)
                {
                    IsAutomating = false;
                }
            }
        }

        private void HandleAutoSleep()
        {
            var numPlayers = Game1.otherFarmers.Count;
            if (numPlayers == 0) return; // only auto sleep when other players are online

            var numReadySleep = Game1.player.team.GetNumberReady("sleep");
            var numberRequiredSleep = Game1.player.team.GetNumberRequired("sleep");

            if (numberRequiredSleep - numReadySleep == 1)
            {
                StartSleep();
            }
        }

        private void HandleAutoLeaveFestival()
        {
            var numPlayers = Game1.otherFarmers.Count;
            if (numPlayers == 0) return;

            var numReady = Game1.player.team.GetNumberReady("festivalEnd");
            var numReq = Game1.player.team.GetNumberRequired("festivalEnd");

            if (numReq - numReady == 1)
            {
                LeaveFestival();
            }
        }

        private void LockPlayersChests()
        {
            foreach (var farmer in Game1.getOnlineFarmers().Where(farmer => !farmer.IsMainPlayer))
            {
                if (farmer.currentLocation is FarmHouse house && farmer != house.owner)
                {
                    // lock offline player inventory
                    if (house is Cabin cabin)
                    {
                        NetMutex inventoryMutex = _helper.Reflection.GetField<NetMutex>(cabin, "inventoryMutex")
                            .GetValue();
                        inventoryMutex.RequestLock();
                    }

                    // lock fridge & chests
                    house.fridge.Value.mutex.RequestLock();
                    foreach (var chest in house.objects.Values.OfType<Chest>())
                        chest.mutex.RequestLock();
                }
            }
        }


        /// <summary>Raised after the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!IsAutomating) return;
            
            var currentTime = Game1.timeOfDay;
            var numPlayers = Game1.otherFarmers.Count;
            var currentDate = SDate.Now();
            
            UpdateFestivalStatus();

            //handles various events that the host normally has to click through
            if (currentDate != grampasGhost && currentDate != eggFestival && currentDate != flowerDance &&
                currentDate != luau && currentDate != danceOfJellies && currentDate != stardewValleyFair &&
                currentDate != spiritsEve && currentDate != festivalOfIce && currentDate != feastOfWinterStar)
            {
                if (currentTime == 620)
                {
                    //check mail 10 a day
                    for (int i = 0; i < 10; i++)
                    {
                        _helper.Reflection.GetMethod(Game1.currentLocation, "mailbox").Invoke();
                    }
                }

                if (currentTime == 630)
                {
                    //rustkey-sewers unlock
                    if (!Game1.player.hasRustyKey)
                    {
                        int items1 = _helper.Reflection.GetMethod(new LibraryMuseum(), "numberOfMuseumItemsOfType")
                            .Invoke<int>("Arch");
                        int items2 = _helper.Reflection.GetMethod(new LibraryMuseum(), "numberOfMuseumItemsOfType")
                            .Invoke<int>("Minerals");
                        int items3 = items1 + items2;
                        if (items3 >= 60)
                        {
                            Game1.player.eventsSeen.Add(295672);
                            Game1.player.eventsSeen.Add(66);
                            Game1.player.hasRustyKey = true;
                        }
                    }


                    //community center complete
                    if (this.Config.IsCommunityCenterRun)
                    {
                        if (!Game1.player.eventsSeen.Contains(191393) &&
                            Game1.player.mailReceived.Contains("ccCraftsRoom") &&
                            Game1.player.mailReceived.Contains("ccVault") &&
                            Game1.player.mailReceived.Contains("ccFishTank") &&
                            Game1.player.mailReceived.Contains("ccBoilerRoom") &&
                            Game1.player.mailReceived.Contains("ccPantry") &&
                            Game1.player.mailReceived.Contains("ccBulletin"))
                        {
                            CommunityCenter locationFromName =
                                Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
                            for (int index = 0; index < locationFromName.areasComplete.Count; ++index)
                                locationFromName.areasComplete[index] = true;
                            Game1.player.eventsSeen.Add(191393);
                        }
                    }

                    //Joja run 
                    if (!this.Config.IsCommunityCenterRun)
                    {
                        if (Game1.player.Money >= 10000 && !Game1.player.mailReceived.Contains("JojaMember"))
                        {
                            Game1.player.Money -= 5000;
                            Game1.player.mailReceived.Add("JojaMember");
                            _helper.SendPublicMessage("Buying Joja Membership");
                        }

                        if (Game1.player.Money >= 30000 && !Game1.player.mailReceived.Contains("jojaBoilerRoom"))
                        {
                            Game1.player.Money -= 15000;
                            Game1.player.mailReceived.Add("ccBoilerRoom");
                            Game1.player.mailReceived.Add("jojaBoilerRoom");
                            _helper.SendPublicMessage("Buying Joja Minecarts");
                        }

                        if (Game1.player.Money >= 40000 && !Game1.player.mailReceived.Contains("jojaFishTank"))
                        {
                            Game1.player.Money -= 20000;
                            Game1.player.mailReceived.Add("ccFishTank");
                            Game1.player.mailReceived.Add("jojaFishTank");
                            _helper.SendPublicMessage("Buying Joja Panning");
                        }

                        if (Game1.player.Money >= 50000 && !Game1.player.mailReceived.Contains("jojaCraftsRoom"))
                        {
                            Game1.player.Money -= 25000;
                            Game1.player.mailReceived.Add("ccCraftsRoom");
                            Game1.player.mailReceived.Add("jojaCraftsRoom");
                            _helper.SendPublicMessage("Buying Joja Bridge");
                        }

                        if (Game1.player.Money >= 70000 && !Game1.player.mailReceived.Contains("jojaPantry"))
                        {
                            Game1.player.Money -= 35000;
                            Game1.player.mailReceived.Add("ccPantry");
                            Game1.player.mailReceived.Add("jojaPantry");
                            _helper.SendPublicMessage("Buying Joja Greenhouse");
                        }

                        if (Game1.player.Money >= 80000 && !Game1.player.mailReceived.Contains("jojaVault"))
                        {
                            Game1.player.Money -= 40000;
                            Game1.player.mailReceived.Add("ccVault");
                            Game1.player.mailReceived.Add("jojaVault");
                            _helper.SendPublicMessage("Buying Joja Bus");
                            Game1.player.eventsSeen.Add(502261);
                        }
                    }
                }

                //Hide at start of day
                if (currentTime is 600 or 610)
                {
                    HideFarmer();
                }
                

                //get fishing rod (standard spam clicker will get through cutscene)
                if (currentTime == 900 && !Game1.player.eventsSeen.Contains(739330))
                {
                    Game1.player.increaseBackpackSize(1);
                    Game1.warpFarmer("Beach", 50, 50, 1);
                }
            }
        }

        private void UpdateFestivalStatus()
        {
            if (Game1.otherFarmers.Count == 0) return;
            
            var currentDate = SDate.Now();

            if (currentDate == eggFestival)
            {
                EggFestival();
            }
            else if (currentDate == flowerDance)
            {
                FlowerDance();
            }
            else if (currentDate == luau)
            {
                Luau();
            }
            else if (currentDate == danceOfJellies)
            {
                DanceOfTheMoonlightJellies();
            }
            else if (currentDate == stardewValleyFair)
            {
                StardewValleyFair();
            }
            else if (currentDate == spiritsEve)
            {
                SpiritsEve();
            }
            else if (currentDate == festivalOfIce)
            {
                FestivalOfIce();
            }
            else if (currentDate == feastOfWinterStar)
            {
                FeastOfWinterStar();
            }
        }

        public void EggFestival()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime is >= 900 and <= 1400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Town", 1, 20, 1);
                });

                eggHuntAvailable = true;
            }
            else if (currentTime >= 1410)
            {
                eggHuntAvailable = false;
                Game1.options.setServerMode("online");
                eggHuntCountDown = 0;
                festivalTicksForReset = 0;
            }
        }


        public void FlowerDance()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime is >= 900 and <= 1400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Forest", 1, 20, 1);
                });

                flowerDanceAvailable = true;
            }
            else if (currentTime >= 1410)
            {
                flowerDanceAvailable = false;
                Game1.options.setServerMode("online");
                flowerDanceCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void Luau()
        {
            var currentTime = Game1.timeOfDay;
            if (currentTime is >= 900 and <= 1400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Beach", 1, 20, 1);
                });

                luauSoupAvailable = true;
            }
            else if (currentTime >= 1410)
            {
                luauSoupAvailable = false;
                Game1.options.setServerMode("online");
                luauSoupCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void DanceOfTheMoonlightJellies()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime >= 2200 && currentTime <= 2400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Beach", 1, 20, 1);
                });

                jellyDanceAvailable = true;
            }
            else if (currentTime >= 2410)
            {
                jellyDanceAvailable = false;
                Game1.options.setServerMode("online");
                jellyDanceCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void StardewValleyFair()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime >= 900 && currentTime <= 1500)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Town", 1, 20, 1);
                });

                grangeDisplayAvailable = true;
            }
            else if (currentTime >= 1510)
            {
                Game1.displayHUD = true;
                grangeDisplayAvailable = false;
                Game1.options.setServerMode("online");
                grangeDisplayCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void SpiritsEve()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime is >= 2200 and <= 2350)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Town", 1, 20, 1);
                });

                goldenPumpkinAvailable = true;
            }
            else if (currentTime >= 2400)
            {
                Game1.displayHUD = true;
                goldenPumpkinAvailable = false;
                Game1.options.setServerMode("online");
                goldenPumpkinCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void FestivalOfIce()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime >= 900 && currentTime <= 1400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Forest", 1, 20, 1);
                });

                iceFishingAvailable = true;
            }
            else if (currentTime >= 1410)
            {
                iceFishingAvailable = false;
                Game1.options.setServerMode("online");
                iceFishingCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        public void FeastOfWinterStar()
        {
            var currentTime = Game1.timeOfDay;

            if (currentTime >= 900 && currentTime <= 1400)
            {
                Game1.player.team.SetLocalReady("festivalStart", true);
                Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, who =>
                {
                    Game1.exitActiveMenu();
                    Game1.warpFarmer("Town", 1, 20, 1);
                });

                winterFeastAvailable = true;
            }
            else if (currentTime >= 1410)
            {
                winterFeastAvailable = false;
                Game1.options.setServerMode("online");
                winterFeastCountDown = 0;
                festivalTicksForReset = 0;
            }
        }

        private void StartSleep()
        {
            HideFarmer();
            _helper.Reflection.GetMethod(Game1.getLocationFromName("Farmhouse"), "startSleep").Invoke();
            Game1.displayHUD = true;
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (_disableRendering)
            {
                _optimizer.EnableDrawing();
            }

            if (IsAutomating)
            {
                shippingMenuActive = true;
                if (Game1.activeClickableMenu is ShippingMenu)
                {
                    _monitor.Log("This is the Shipping Menu");
                    _helper.Reflection.GetMethod(Game1.activeClickableMenu, "okClicked").Invoke();
                }
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this event will trigger a stability warning in the SMAPI console.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUnvalidatedUpdateTick(object sender, UnvalidatedUpdateTickedEventArgs e)
        {
            var currentDate = SDate.Now();


            if (currentDate == danceOfJellies ||
                currentDate == spiritsEve && this.Config.EndOfDayTimeOut != 0)
            {
                if (Game1.timeOfDay is >= 2400 or 600)
                {
                    timeOutTicksForReset += 1;
                    if (timeOutTicksForReset >= (5040 + (this.Config.EndOfDayTimeOut * 60)))
                    {
                        Game1.options.setServerMode("offline");
                    }
                }
            }

            if (shippingMenuActive && this.Config.EndOfDayTimeOut != 0)
            {
                shippingMenuTimeoutTicks += 1;
                if (shippingMenuTimeoutTicks >= this.Config.EndOfDayTimeOut * 60)
                {
                    Game1.options.setServerMode("offline");
                }
            }

            if (Game1.timeOfDay == 610)
            {
                shippingMenuActive = false;

                Game1.options.setServerMode("online");
                timeOutTicksForReset = 0;
                shippingMenuTimeoutTicks = 0;
            }

            if (Game1.timeOfDay == 2600)
            {
                Game1.paused = false;
            }
        }

        /// <summary>Leave the current festival, if any.</summary>
        private void LeaveFestival()
        {
            Game1.player.team.SetLocalReady("festivalEnd", true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, who =>
            {
                Game1.exitActiveMenu();
                HideFarmer();
                var isSpiritsEve = SDate.Now() == spiritsEve;
                Game1.timeOfDay = isSpiritsEve ? 2400 : 2200;
            });
        }

        private void HideFarmer()
        {
            Game1.warpFarmer("Farm", 1, 1, false);
        }
    }
}