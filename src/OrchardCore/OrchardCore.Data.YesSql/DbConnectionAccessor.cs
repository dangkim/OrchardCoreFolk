using System;
using System.Data.Common;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents an accessor to the database connection.
    /// </summary>
    public class DbConnectionAccessor : IDbConnectionAccessor
    {
        private readonly IStore _store;
        private readonly IStore _storeReadOnly;

        /// <summary>
        /// Creates a new instance of the <see cref="DbConnectionAccessor"/>.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/>.</param>
        /// <param name="storeReadOnly">The <see cref="IStore"/>.</param>
        public DbConnectionAccessor(IStore store, IStore storeReadOnly)
        {
            ArgumentNullException.ThrowIfNull(store);

            _store = store;
            _storeReadOnly = storeReadOnly;
        }

        /// <inheritdocs />
        public DbConnection CreateConnection()
        {
            return _store.Configuration.ConnectionFactory.CreateConnection();
        }
        public DbConnection CreateReadOnlyConnection()
        {
            return _storeReadOnly.Configuration.ConnectionFactory.CreateConnection();
        }

    }
}
