using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Data;
public class ReadOnlyStoreWrapper : IReadOnlyYesSqlStore
{
    private readonly IStore _store;

    public ReadOnlyStoreWrapper(IStore store)
    {
        _store = store;
    }

    public IConfiguration Configuration => _store.Configuration;

    public ITypeService TypeNames => _store.TypeNames;

    public ISession CreateSession(bool withTracking = true)
    {
        return _store.CreateSession(withTracking);
    }

    public IEnumerable<IndexDescriptor> Describe(Type target, string collection = null)
    {
        return _store.Describe(target, collection);
    }

    public void Dispose()
    {
        _store.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task InitializeAsync()
    {
        return _store.InitializeAsync();
    }

    public Task InitializeCollectionAsync(string collection)
    {
        return _store.InitializeCollectionAsync(collection);
    }

    public IStore RegisterIndexes(IEnumerable<IIndexProvider> indexProviders, string collection = null)
    {
        return _store.RegisterIndexes(indexProviders, collection);
    }
}

