using System;
using System.Text.Json;
using AuthServer.Entities;
using AuthServer.Migrations;

namespace AuthServer.Services
{
    public interface IGameService
    {
        bool Update(string gameId, string savedGame);
        ISave Get(string gameId);
    }

    public class GameService : IGameService
    {
        public bool Update(string gameId, string savedGame)
        {
            var task = DatabaseManager.GetDatabaseManager().Update(gameId, savedGame);
            lock (task)
            {
                return task.IsCompletedSuccessfully;
            }
        }

        public ISave Get(string gameId)
        {
            var save = DatabaseManager.GetDatabaseManager().GetSave(gameId);
            lock (save)
            {
                return save.Result;
            }
        }
    }
}