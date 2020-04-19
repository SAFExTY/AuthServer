namespace AuthServer.Helpers
{
    public class AppSettings
    {
        // todo make token loadable from env
        public string Secret { get; set; }
        public DatabaseInfos Arango { get; set; }
    }

    public class DatabaseInfos
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
    }
}