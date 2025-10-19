using Devsmn.Common.Diagnostics;
using SQLite;

namespace Devsmn.Common.Data.SQLite
{
    /// <summary>
    /// Provides common sqlite specific functionality. This class is not thread-safe.
    /// </summary>
    public abstract class SqliteRepository
    {
        public bool IsValid { get; private set; }

        private SQLiteAsyncConnection? _database;

        protected SQLiteAsyncConnection? Database
        {
            get {
                if (!IsValid)
                    throw new InvalidOperationException("Database is not in a valid state");

                return _database;
            }
        }

        protected abstract SQLiteConnectionString CreateConnectionString();

        /// <summary>
        /// Validates the connection to the database.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ValidateConnection()
        {
            try {
                if (_database == null)
                    throw new InvalidOperationException("Database is not initialized");

                // The only way to validate whether the cipher was correct is to execute a statement.
                string table = await _database.ExecuteScalarAsync<string>("SELECT name FROM sqlite_master WHERE type='table' and name='META';");
                IsValid = !string.IsNullOrEmpty(table);
            }
            catch (Exception) {
                IsValid = false;
            }

            if (!IsValid)
                throw new InvalidOperationException("Database is not in a valid state");
        }

        public virtual async Task<bool> ValidateIntegrity(IContext context)
        {
            try {
                if (_database == null)
                    throw new InvalidOperationException("Database is not initialized");

                string result = await _database.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                context.Log($"Database integrity check=[{result}]");

                return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) {
                context.Log(ex);
            }

            return false;
        }

        /// <summary>
        /// Executes the given <paramref name="action"/> for the provided <paramref name="commandText"/> while
        /// observing exceptions.
        /// <para>
        /// The transaction is managed by this method, i.e. it is rolled-back automatically on errors.</para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual TResult? Audit<TResult>(IContext context, string commandText, Func<SQLiteCommand, TResult> action)
        {
            if (string.IsNullOrEmpty(commandText)) {
                context.Log(new Exception("Command text is empty"));
                return default;
            }

            if (Database == null)
                throw new InvalidOperationException("Database is not initialized");

            bool ownTransaction = false;
            SQLiteConnection? connection = null;
            bool success = true;

            try {
                connection = Database.GetConnection();
                ownTransaction = !connection.IsInTransaction;

                if (ownTransaction) {
                    connection.BeginTransaction();
                }

                return action(connection.CreateCommand(commandText));
            }
            catch (Exception ex) {
                if (ownTransaction) {
                    connection?.Rollback();
                }

                success = false;
                context.Log(ex);
            }
            finally {
                if (success && ownTransaction) {
                    connection?.Commit();
                }
            }

            return default;
        }


        /// <summary>
        /// Creates an <see cref="SQLiteCommand"/> based on the provided <paramref name="commandText"/>
        /// and executes the given <paramref name="action"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandText"></param>
        /// <param name="action"></param>
        public virtual void Audit(IContext context, string commandText, Action<SQLiteCommand> action)
        {
            bool ownTransaction = false;
            SQLiteConnection? connection = null;

            if (Database == null)
                throw new InvalidOperationException("Database is not initialized");

            try {
                connection = Database.GetConnection();
                ownTransaction = !connection.IsInTransaction;

                if (ownTransaction) {
                    connection.BeginTransaction();
                }

                action(connection.CreateCommand(commandText));

                if (ownTransaction) {
                    connection.Commit();
                }
            }
            catch (Exception ex) {
                if (ownTransaction) {
                    connection?.Rollback();
                }

                context.Log(ex);
            }
        }

        public virtual async Task CloseAsync()
        {
            if (_database == null)
                return;

            await _database.CloseAsync();
            _database = null;
        }

    }
}
