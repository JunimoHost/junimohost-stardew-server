using StardewValley;

namespace JunimoServer.Services.ChatCommands
{
    public class ReceivedMessage
    {
        public enum ChatKinds
        {
            ChatMessage,
            ErrorMessage,
            UserNotification,
            PrivateMessage
        }

        public long SourceFarmer { get; set; }
        public ChatKinds ChatKind { get; set; }
        public LocalizedContentManager.LanguageCode Language { get; set; }
        public string Message { get; set; }
    }
}