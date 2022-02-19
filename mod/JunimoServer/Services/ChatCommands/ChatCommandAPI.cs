using System;

namespace JunimoServer.Services.ChatCommands
{
    public interface IChatCommandApi
    {
        public void RegisterCommand(string name, string description, Action<ReceivedMessage> action);
    }
}