
namespace DiscordQR
{
    public class Discord
    {
        public int QrCodeId;
        private string discordToken;
        private string discordUsername;
        private string discordEmail;

        public string DiscordToken { get => discordToken; set => discordToken = value; }
        public string DiscordUsername { get => discordUsername; set => discordUsername = value; }
        public string DiscordEmail { get => discordEmail; set => discordEmail = value; }

        public Discord(int id)
        {
            this.QrCodeId = id;
            this.DiscordToken = "/";
            this.DiscordUsername = "/";
            this.DiscordEmail = "/";
        }
    }
}
