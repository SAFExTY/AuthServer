using System;
using System.Text.Json;
using AuthServer.Entities;
using AuthServer.Migrations;

namespace AuthServer.Services
{
    public interface IGameService
    {
        bool Update(string gameId, JsonElement savedGame);
        Save Get(string gameId);
    }

    public class GameService : IGameService
    {
        public bool Update(string gameId, JsonElement savedGame)
        {
            var task = DatabaseManager.GetDatabaseManager().Update(gameId, savedGame);
            lock (task)
            {
                Console.WriteLine(task.IsCompleted);
                Console.WriteLine(task.IsCanceled);
                Console.WriteLine(task.IsFaulted);
                Console.WriteLine(task.Result);
                return task.IsCompletedSuccessfully;
            }
        }

        public Save Get(string gameId)
        {
            var save = DatabaseManager.GetDatabaseManager().GetSave(gameId);
            lock (save)
            {
                return save.Result;
            }
        }
    }
}