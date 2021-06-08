
namespace DiscordQR
{
    public class Discord
    {
        public int QrCodeId;
        private string discordToken;
        private string discordUsername;
        private string discordEmail;
        private string status;

        public string DiscordToken { get => discordToken; set => discordToken = value; }
        public string DiscordUsername { get => discordUsername; set => discordUsername = value; }
        public string DiscordEmail { get => discordEmail; set => discordEmail = value; }
        public string Status { get => status; set => status = value; }

        public Discord(int id)
        {
            this.QrCodeId = id;
            this.DiscordToken = "/";
            this.DiscordUsername = "/";
            this.DiscordEmail = "/";
            this.Status = "Available";
        }

        public override string ToString()
        {
            return "Token: " + DiscordToken + " ~ Username: " + DiscordUsername + " ~ Email: " + DiscordEmail;
        }
    }
}
