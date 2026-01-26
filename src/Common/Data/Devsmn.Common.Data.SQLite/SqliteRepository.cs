using System.Data;
using Devsmn.Common.Diagnostics;
using SQLite;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SQLite.SQLite3;

namespace Devsmn.Common.Data.SQLite
{
    /// <summary>
    /// Provides common sqlite specific functionality. This class is not thread-safe.
    /// </summary>
    public abstract class SqliteRepository
    {
        public bool IsValid { get; private set; } = true;

        private SQLiteAsyncConnection? _database;

        protected SQLiteAsyncConnection Database
        {
            get
            {
                _database ??= new SQLiteAsyncConnection(CreateConnectionString());

                if (!IsValid)
                    throw new InvalidOperationException("Database is not in a valid state");

                return _database;
            }
        }

        /// <summary>
        /// Creates the connection string.
        /// </summary>
        /// <returns></returns>
        protected abstract SQLiteConnectionString CreateConnectionString();

        /// <summary>
        /// Validates the connection to the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ValidateConnection()
        {
            try
            {
                // The only way to validate whether the cipher was correct is to execute a statement.
                string table = await Database.ExecuteScalarAsync<string>("SELECT name FROM sqlite_master WHERE type='table' and name='META';");
                IsValid = !string.IsNullOrEmpty(table);
            }
            catch (Exception)
            {
                IsValid = false;
            }

            if (!IsValid)
                throw new InvalidOperationException("Database is not in a valid state");
        }

        /// <summary>
        /// Validates the integrity of the database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<bool> ValidateIntegrity(IContext context)
        {
            try
            {
                string result = await Database.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                context.Log($"Database integrity check=[{result}]");

                return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }

            return false;
        }


        /// <summary>
        /// Executes the given <paramref name="action"/> by creating a <see cref="SQLiteCommand"/> based on the provided <paramref name="commandText"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual async Task<SqliteResult<TData>?> AuditAsync<TData>(
            IContext context,
            string commandText,
            Func<SQLiteCommand, TData> action)
        {
            SqliteResult<TData> result = new();

            if (string.IsNullOrEmpty(commandText))
            {
                context.Log(new Exception("Command text is empty"));
                return result;
            }

            await Database.RunInTransactionAsync(connection =>
            {
                SQLiteCommand command = connection.CreateCommand(commandText);
                result.Data = action(command);
            });

            return result;
        }

        /// <summary>
        /// Executes the given <paramref name="action"/> by creating a <see cref="SQLiteCommand"/> based on the provided <paramref name="commandText"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual async Task<SqliteResult> AuditAsync(IContext context, string commandText, Action<SQLiteCommand> action)
        {
            SqliteResult result = new();
            if (string.IsNullOrEmpty(commandText))
            {
                context.Log(new Exception("Command text is empty"));
                return result;
            }

            await Task.Run(() =>
            {
                SQLiteConnectionWithLock connection = Database.GetConnection();
                using (connection.Lock())
                {
                    SQLiteCommand command = connection.CreateCommand(commandText);
                    connection.RunInTransaction(() => action(command));
                    result.RowId = SQLite3.LastInsertRowid(connection.Handle);
                }
            });

            return result;
        }

        /// <summary>
        /// Executes the given <paramref name="actions"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<SqliteResult>> AuditAsync(IContext context, params Action<ISQLiteConnection>[] actions)
        {
            List<SqliteResult> results = new();

            await Task.Run(() =>
            {
                SQLiteConnectionWithLock connection = Database.GetConnection();

                foreach (var action in actions)
                {
                    action(connection);

                    SqliteResult result = new();
                    result.RowId = SQLite3.LastInsertRowid(connection.Handle);
                    results.Add(result);
                }

            });

            return results;
        }

        /// <summary>
        /// Executes the given <paramref name="actions"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="context"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<SqliteResult<TData>>> AuditAsync<TData>(IContext context, params Func<ISQLiteConnection, TData>[] actions)
        {
            List<SqliteResult<TData>> results = new();

            await Database.RunInTransactionAsync(connection =>
            {
                foreach (var action in actions)
                {
                    SqliteResult<TData> result = new();
                    result.Data = action(connection);

                    results.Add(result);
                }
            });

            return results;
        }

        /// <summary>
        /// Closes the database.
        /// </summary>
        /// <returns></returns>
        public virtual async Task CloseAsync()
        {
            if (!IsValid)
                return;

            await Database.CloseAsync();
            IsValid = false;
        }
    }
}
