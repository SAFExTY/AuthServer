using System;
using Microsoft.AspNetCore.Mvc;
using AuthServer.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public IActionResult Update([FromBody] UpdateGameModel model)
        {
            if (_gameService.Update(model.GameId, model.Game))
                return Ok();
            return BadRequest();
        }

        [HttpGet("{gameId}")]
        public IActionResult Get(string gameId)
        {
            Console.WriteLine(gameId);
            var saved = _gameService.Get(gameId);
            if (saved == null)
                return NotFound();
            return Ok(saved.Game);
        }
    }
}