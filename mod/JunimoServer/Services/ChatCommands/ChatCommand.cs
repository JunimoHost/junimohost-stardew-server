﻿using System;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommand
    {
        public string Name;
        public string Description;
        public Action<string[], ReceivedMessage> Action;

        public ChatCommand(string name, string description, Action<string[], ReceivedMessage> action)
        {
            Name = name;
            Description = description;
            Action = action;
        }
    }
}