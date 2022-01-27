using System;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommand
    {
        public string Name;
        public string Documentation;
        public Action<string> OnCommand;

        public ChatCommand(string name, string documentation, Action<string> onCommand)
        {
            Name = name;
            Documentation = documentation;
            OnCommand = onCommand;
        }
    }
}