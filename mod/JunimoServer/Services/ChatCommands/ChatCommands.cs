using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Menus;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommands : IChatCommandApi
    {
        private readonly IMonitor _monitor;

        private readonly List<ChatCommand> _registeredCommands = new List<ChatCommand>();

        public ChatCommands(IMonitor monitor, Harmony harmony)
        {
            _monitor = monitor;
            ChatWatcher.Initialize(OnChatMessage);

            harmony.Patch(
                original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
                postfix: new HarmonyMethod(typeof(ChatWatcher), nameof(ChatWatcher.receiveChatMessage_Postfix))
            );
        }


        private void OnChatMessage(ReceivedMessage obj)
        {
            _monitor.Log($"{obj.Message}");

            foreach (var command in _registeredCommands.Where(command => command.Name.Equals(obj.Message)))
            {
                command.Action(obj);
            }
        }

        public void RegisterCommand(string name, string description, Action<ReceivedMessage> action)
        {
            _registeredCommands.Add(new ChatCommand(name, description, action));
        }
    }
}