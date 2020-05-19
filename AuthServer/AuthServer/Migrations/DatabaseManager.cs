using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArangoDBNetStandard;
using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.DocumentApi.Models;
using ArangoDBNetStandard.Transport.Http;
using AuthServer.Entities;
using AuthServer.Helpers;

namespace AuthServer.Migrations
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        public bool IsReady { get; private set; }
        private ArangoDBClient Client { get; set; }

        private readonly DatabaseInfos _databaseSettings = Startup.AppSettings.Arango;

        public static DatabaseManager GetDatabaseManager()
        {
            return _instance ??= new DatabaseManager();
        }

        public DatabaseManager()
        {
            IsReady = false;
        }

        public ArangoDBClient OpenConnection(string dbName = "_system", bool root = false)
        {
            if (Client != null)
                return Client;

            var user = root ? _databaseSettings.User : _databaseSettings.DbUser;
            var password = root ? _databaseSettings.Password : _databaseSettings.DbPassword;
            // We must use the _system database to create databases
            var systemDbTransport = HttpApiTransport.UsingBasicAuth(
                new Uri(_databaseSettings.Url),
                dbName,
                password,
                user
            );
            return Client = new ArangoDBClient(systemDbTransport);
        }

        public void CloseConnection()
        {
            Client?.Dispose();
            Client = null;
        }

        public async Task<InternalUser> GetUser(string username)
        {
            var adb = OpenConnection(_databaseSettings.DbName, true);
            //Email
            var user = await adb.Cursor.PostCursorAsync<InternalUser>(
                @"FOR doc IN users
                            FILTER doc.Username == '" + username + "' || doc.Email == '" + username + @"'
                            LIMIT 1
                            RETURN doc");
            return user.Result.FirstOrDefault();
        }

        public async Task<IEnumerable<IUser>> GetUsers()
        {
            var adb = OpenConnection(_databaseSettings.DbName, true);
            //Email
            var user = await adb.Cursor.PostCursorAsync<InternalUser>(
                @"FOR doc IN users
                            LIMIT 1
                            RETURN doc");
            return user.Result;
        }

        public async Task<ISave> GetSave(string gameId)
        {
            var adb = OpenConnection(_databaseSettings.DbName, true);
            var save = await adb.Cursor.PostCursorAsync<Save>(
                @"FOR doc IN games
                            FILTER doc._key == '" + gameId + @"'
                            LIMIT 1
                            RETURN doc");
            return save.Result.FirstOrDefault();
        }

        public async Task<(Task<PostDocumentResponse<Save>>, Task<PostDocumentResponse<Save>>)> Update(string gameId,
            string game)
        {
            var adb = OpenConnection(_databaseSettings.DbName, true);
            var returnVal = adb.Document.PutDocumentAsync(
                $"games/{gameId}",
                new Save
                {
                    Game = game
                }
            );
            var taskPost = adb.Document.PostDocumentAsync(
                "games",
                new Save
                {
                    _key = gameId,
                    Game = game
                });
            returnVal.ContinueWith(response => response.Exception != null ? null : taskPost);
            CloseConnection();
            return (returnVal, taskPost);
        }

        public async void CreateIfNotExists()
        {
            // Use root user to check if the database exist (and create it if needed)
            var adb = OpenConnection(root: true);

            // Lists all databases
            var dtbTask = await adb.Database.GetDatabasesAsync();
            if (dtbTask.Error)
            {
                Console.WriteLine("Unable to get databases ! Aborting...");
                Environment.Exit(1);
                return;
            }

            var databases = dtbTask.Result;
            if (databases.Contains(_databaseSettings.DbName))
            {
                IsReady = true;
                CloseConnection();
                Console.WriteLine("Database ready !");
                return;
            }

            // Create the database with one user.
            var dtbPostTask = await adb.Database.PostDatabaseAsync(new PostDatabaseBody
            {
                Name = _databaseSettings.DbName,
                Users = new List<DatabaseUser>
                {
                    //todo Fork the api to manager user permission
                    new DatabaseUser
                    {
                        Username = _databaseSettings.DbUser,
                        Passwd = _databaseSettings.DbPassword,
                    }
                }
            });
            if (dtbPostTask.Error)
            {
                Console.WriteLine("Unable to create database ! Aborting...");
                Environment.Exit(1);
                return;
            }

            IsReady = true;
            Console.WriteLine("Database created !");
            CloseConnection();
            // Switch db to create collection
            adb = OpenConnection(_databaseSettings.DbName, true);
            var dtbCollectionTask = await adb.Collection.PostCollectionAsync(new PostCollectionBody
            {
                Name = "users",
            });

            if (dtbCollectionTask.Error)
            {
                Console.WriteLine("Unable to create collection ! Aborting...");
                Environment.Exit(1);
                return;
            }

            CloseConnection();
        }
    }
}