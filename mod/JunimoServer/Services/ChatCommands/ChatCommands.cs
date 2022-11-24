using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JunimoServer.Util;
using StardewModdingAPI;
using StardewValley.Menus;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommands : IChatCommandApi
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        private readonly List<ChatCommand> _registeredCommands = new List<ChatCommand>();

        public ChatCommands(IMonitor monitor, Harmony harmony, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;

            ChatWatcher.Initialize(OnChatMessage);

            harmony.Patch(
                original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
                postfix: new HarmonyMethod(typeof(ChatWatcher), nameof(ChatWatcher.receiveChatMessage_Postfix))
            );

            RegisterCommand(new ChatCommand("help", "Displays available commands.", HelpCommand));
        }

        private void HelpCommand(string[] args, ReceivedMessage msg)
        {

            foreach (var command in _registeredCommands)
            {
                _helper.SendPrivateMessage(msg.SourceFarmer,  $"!{command.Name}: {command.Description}");
            }

        }


        private void OnChatMessage(ReceivedMessage obj)
        {
            _monitor.Log($"{obj.Message}");

            var msg = obj.Message;
            if (String.IsNullOrEmpty(msg) || msg[0] != '!' || msg.Length < 2)
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