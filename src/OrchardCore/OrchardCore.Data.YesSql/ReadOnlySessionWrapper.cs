using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Data;
public class ReadOnlySessionWrapper : IReadOnlySession
{
    private readonly ISession _session;

    public ReadOnlySessionWrapper(ISession session)
    {
        _session = session;
    }

    public DbTransaction CurrentTransaction => _session.CurrentTransaction;

    public IStore Store => _session.Store;

    public Task<DbTransaction> BeginTransactionAsync()
    {
        return _session.BeginTransactionAsync();
    }

    public Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        return _session.BeginTransactionAsync(isolationLevel);
    }

    public Task CancelAsync()
    {
        return _session.CancelAsync();
    }

    public Task<DbConnection> CreateConnectionAsync()
    {
        return _session.CreateConnectionAsync();
    }

    public void Delete(object item, string collection = null)
    {
        _session.Delete(item, collection);
    }

    public void Detach(object item, string collection = null)
    {
        _session.Detach(item, collection);
    }

    public void Detach(IEnumerable<object> entries, string collection = null)
    {
        _session.Detach(entries, collection);
    }

    public void Dispose() => _session.Dispose();

    public ValueTask DisposeAsync() => _session.DisposeAsync();

    public IQuery<T> ExecuteQuery<T>(ICompiledQuery<T> compiledQuery, string collection = null) where T : class
    {
        return _session.ExecuteQuery(compiledQuery, collection);
    }

    public Task FlushAsync()
    {
        return _session.FlushAsync();
    }

    public Task<IEnumerable<T>> GetAsync<T>(long[] ids, string collection = null) where T : class
    {
        return _session.GetAsync<T>(ids, collection);
    }

    public bool Import(object item, long id, long version, string collection = null)
    {
        return _session.Import(item, id, version, collection);
    }

    public IQuery Query(string collection = null)
    {
        return _session.Query(collection);
    }

    public ISession RegisterIndexes(IIndexProvider[] indexProviders, string collection = null)
    {
        return _session.RegisterIndexes(indexProviders, collection);
    }

    public void Save(object obj, bool checkConcurrency = false, string collection = null)
    {
        _session.Save(obj, checkConcurrency, collection);
    }

    public Task SaveAsync(object obj, bool checkConcurrency = false, string collection = null)
    {
        return _session.SaveAsync(obj, checkConcurrency, collection);
    }

    public Task SaveChangesAsync()
    {
        return _session.SaveChangesAsync();
    }
}

