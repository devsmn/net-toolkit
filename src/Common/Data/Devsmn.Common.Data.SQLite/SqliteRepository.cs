using Devsmn.Common.Diagnostics;
using SQLite;

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
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual async Task<TResult?> AuditAsync<TResult>(IContext context, string commandText, Func<SQLiteCommand, TResult> action)
        {
            TResult? result = default;

            if (string.IsNullOrEmpty(commandText))
            {
                context.Log(new Exception("Command text is empty"));
                return result;
            }

            await Database.RunInTransactionAsync(connection =>
            {
                result = action(connection.CreateCommand(commandText));
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
        public virtual async Task AuditAsync(IContext context, string commandText, Action<SQLiteCommand> action)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                context.Log(new Exception("Command text is empty"));
                return;
            }

            await Database.RunInTransactionAsync(connection => action(connection.CreateCommand(commandText)));
        }

        /// <summary>
        /// Executes the given <paramref name="actions"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public virtual async Task AuditAsync(IContext context, params Action<ISQLiteConnection>[] actions)
        {
            await Database.RunInTransactionAsync(connection =>
            {
                foreach (var action in actions)
                {
                    action(connection);
                }
            });
        }

        /// <summary>
        /// Executes the given <paramref name="actions"/>.
        /// Do not call Commit, Rollback or any other transaction methods, as the transaction is handled within this method.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TResult>> AuditAsync<TResult>(IContext context, params Func<ISQLiteConnection, TResult>[] actions)
        {
            List<TResult> result = new List<TResult>();

            await Database.RunInTransactionAsync(connection =>
            {
                foreach (var action in actions)
                {
                    result.Add(action(connection));
                }
            });

            return result;
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
