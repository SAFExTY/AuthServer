using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArangoDBNetStandard;
using ArangoDBNetStandard.DatabaseApi;
using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.Transport.Http;

namespace AuthServer.Migrations
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        public bool IsReady { get; private set; }
        private ArangoDBClient Client { get; set; }

        public static DatabaseManager GetDatabaseManager()
        {
            return _instance ??= new DatabaseManager();
        }

        public DatabaseManager()
        {
            IsReady = false;
        }

        public ArangoDBClient OpenConnection()
        {
            if (Client != null)
                return Client;
            var databaseSettings = Startup.AppSettings.Arango;
            // We must use the _system database to create databases
            var systemDbTransport = HttpApiTransport.UsingBasicAuth(
                new Uri(databaseSettings.Url),
                "_system",
                databaseSettings.User,
                databaseSettings.Password
            );
            return Client = new ArangoDBClient(systemDbTransport);
        }

        public void CloseConnection()
        {
            Client?.Dispose();
        }

        public async void CreateIfNotExists()
        {
            var databaseSettings = Startup.AppSettings.Arango;
            var adb = OpenConnection();

            // Lists all databases
            var dtbTask = await adb.Database.GetDatabasesAsync();
            if (dtbTask.Error)
            {
                Console.WriteLine("Unable to get databases ! Aborting...");
                Environment.Exit(1);
                return;
            }

            var databases = dtbTask.Result;
            if (databases.Contains(databaseSettings.DbName))
            {
                IsReady = true;
                Console.WriteLine("Database ready !");
                return;
            }

            // Create the database with one user.
            var dtbPostTask = await adb.Database.PostDatabaseAsync(new PostDatabaseBody
            {
                Name = databaseSettings.DbName,
                Users = new List<DatabaseUser>
                {
                    new DatabaseUser
                    {
                        Username = databaseSettings.DbUser,
                        Passwd = databaseSettings.DbPassword
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
        }
    }
}