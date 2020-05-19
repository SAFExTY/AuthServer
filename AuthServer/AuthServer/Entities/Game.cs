namespace AuthServer.Entities
{
    public interface ISave
    {
        public string Game { get; }
    }

    public class Save : ISave
    {
        public string _key { get; set; }
        public string Game { get; set; }
    }

}