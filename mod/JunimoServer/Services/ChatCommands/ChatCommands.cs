using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Menus;

namespace JunimoServer.Services.ChatCommands
{
    public class ChatCommands
    {

        private readonly IMonitor _monitor;
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
        }
    }
}