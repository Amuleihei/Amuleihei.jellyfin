using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Controller;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Persistence;
using Jellyfin.Model.Serialization;
using Microsoft.Extensions.Logging;
using SQLitePCL.pretty;

namespace Jellyfin.Server.Implementations.Data
{
    /// <summary>
    /// Class SQLiteUserRepository
    /// </summary>
    public class SqliteUserRepository : BaseSqliteRepository, IUserRepository
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SqliteUserRepository(
            ILoggerFactory loggerFactory,
            IServerApplicationPaths appPaths,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory.CreateLogger(nameof(SqliteUserRepository)))
        {
            _jsonSerializer = jsonSerializer;

            DbFilePath = Path.Combine(appPaths.DataPath, "users.db");
        }

        /// <summary>
        /// Gets the name of the repository
        /// </summary>
        /// <value>The name.</value>
        public string Name => "SQLite";

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>Task.</returns>
        public void Initialize()
        {
            using (var connection = CreateConnection())
            {
                RunDefaultInitialization(connection);

                var localUsersTableExists = TableExists(connection, "LocalUsersv2");

                connection.RunQueries(new[] {
                    "create table if not exists LocalUsersv2 (Id INTEGER PRIMARY KEY, guid GUID NOT NULL, data BLOB NOT NULL)",
                    "drop index if exists idx_users"
                });

                if (!localUsersTableExists && TableExists(connection, "Users"))
                {
                    TryMigrateToLocalUsersTable(connection);
                }

                RemoveEmptyPasswordHashes();
            }
        }

        private void TryMigrateToLocalUsersTable(ManagedConnection connection)
        {
            try
            {
                connection.RunQueries(new[]
                {
                    "INSERT INTO LocalUsersv2 (guid, data) SELECT guid,data from users"
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error migrating users database");
            }
        }

        private void RemoveEmptyPasswordHashes()
        {
            foreach (var user in RetrieveAllUsers())
            {
                // If the user password is the sha1 hash of the empty string, remove it
                if (!string.Equals(user.Password, "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", StringComparison.Ordinal)
                    || !string.Equals(user.Password, "$SHA1$DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", StringComparison.Ordinal))
                {
                    continue;
                }

                user.Password = null;
                var serialized = _jsonSerializer.SerializeToBytes(user);

                using (WriteLock.Write())
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("update LocalUsersv2 set data=@data where Id=@InternalId"))
                        {
                            statement.TryBind("@InternalId", user.InternalId);
                            statement.TryBind("@data", serialized);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }

        }

        /// <summary>
        /// Save a user in the repo
        /// </summary>
        public void CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var serialized = _jsonSerializer.SerializeToBytes(user);

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("insert into LocalUsersv2 (guid, data) values (@guid, @data)"))
                        {
                            statement.TryBind("@guid", user.Id.ToGuidBlob());
                            statement.TryBind("@data", serialized);

                            statement.MoveNext();
                        }

                        var createdUser = GetUser(user.Id, false);

                        if (createdUser == null)
                        {
                            throw new ApplicationException("created user should never be null");
                        }

                        user.InternalId = createdUser.InternalId;

                    }, TransactionMode);
                }
            }
        }

        public void UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var serialized = _jsonSerializer.SerializeToBytes(user);

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("update LocalUsersv2 set data=@data where Id=@InternalId"))
                        {
                            statement.TryBind("@InternalId", user.InternalId);
                            statement.TryBind("@data", serialized);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }
        }

        private User GetUser(Guid guid, bool openLock)
        {
            using (openLock ? WriteLock.Read() : null)
            {
                using (var connection = CreateConnection(true))
                {
                    using (var statement = connection.PrepareStatement("select id,guid,data from LocalUsersv2 where guid=@guid"))
                    {
                        statement.TryBind("@guid", guid);

                        foreach (var row in statement.ExecuteQuery())
                        {
                            return GetUser(row);
                        }
                    }
                }
            }

            return null;
        }

        private User GetUser(IReadOnlyList<IResultSetValue> row)
        {
            var id = row[0].ToInt64();
            var guid = row[1].ReadGuidFromBlob();

            using (var stream = new MemoryStream(row[2].ToBlob()))
            {
                stream.Position = 0;
                var user = _jsonSerializer.DeserializeFromStream<User>(stream);
                user.InternalId = id;
                user.Id = guid;
                return user;
            }
        }

        /// <summary>
        /// Retrieve all users from the database
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        public List<User> RetrieveAllUsers()
        {
            var list = new List<User>();

            using (WriteLock.Read())
            {
                using (var connection = CreateConnection(true))
                {
                    foreach (var row in connection.Query("select id,guid,data from LocalUsersv2"))
                    {
                        list.Add(GetUser(row));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">user</exception>
        public void DeleteUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("delete from LocalUsersv2 where Id=@id"))
                        {
                            statement.TryBind("@id", user.InternalId);
                            statement.MoveNext();
                        }
                    }, TransactionMode);
                }
            }
        }
    }
}
