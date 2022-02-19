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

            var msg = obj.Message;
            if (string.IsNullOrEmpty(msg) || msg[0] != '!' || msg.Length < 2)
            {
                return;
            }

            var commandStringWithArgs = msg[1..];
            var commandParts = commandStringWithArgs.Split(" ");
            var commandName = commandParts[0];
            var commandArgs = Array.Empty<string>();
            if (commandParts.Length > 1)
            {
                commandArgs = commandParts[1..];
            }

            foreach (var command in _registeredCommands.Where(command => command.Name.Equals(commandName)))
            {
                command.Action(commandArgs, obj);
            }
        }

        public void RegisterCommand(string name, string description, Action<string[], ReceivedMessage> action)
        {
            _registeredCommands.Add(new ChatCommand(name, description, action));
        }

        public void RegisterCommand(ChatCommand command)
        {
            _registeredCommands.Add(command);
        }
    }
}