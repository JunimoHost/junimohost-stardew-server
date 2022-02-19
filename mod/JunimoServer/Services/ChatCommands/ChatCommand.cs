using System;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommand
    {
        public string Name;
        public string Description;
        public Action<ReceivedMessage> Action;

        public ChatCommand(string name, string description, Action<ReceivedMessage> action)
        {
            Name = name;
            Description = description;
            Action = action;
        }
    }
}