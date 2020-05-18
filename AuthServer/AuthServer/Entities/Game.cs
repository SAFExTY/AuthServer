using System.Text.Json;

namespace AuthServer.Entities
{
    public interface ISave
    {
        public JsonElement Game { get; }
    }

    public class Save : ISave
    {
        public string _key { get; set; }
        public JsonElement Game { get; set; }
        
    }
    
    public class InternalSave : Save
    {
        public string GameId { get; set; }
        
    }
}