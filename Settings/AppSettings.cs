namespace schedulebackup.Settings
{
     public class AppSettings
    {
        public Mail Mail { get; set; } = null!;

    }
    public class Mail
    {
        public string Server { get; set; } = null!;
        public int Port { get; set; }
        public string Account { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Notify { get; set; } = null!;
    }
}