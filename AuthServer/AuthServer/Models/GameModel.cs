using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AuthServer.Models
{
    public class GetGameModel
    {
        [Required] public string GameId { get; set; }
    }

    public class UpdateGameModel
    {
        [Required] public string GameId { get; set; }
        [Required] public JsonElement Game { get; set; }
    }
}