using Nativefy.DbContexts;

namespace Nativefy.Services;

public sealed class SearchQueriesService : ISearchQueriesService
{
    public string[] GetQueries()
    {
        return [.. NativefyDatabaseProvider.SearchQueries];
    }

    public void SaveQuery(string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        query = query.Trim();

        if (NativefyDatabaseProvider.SearchQueries.Contains(query, StringComparer.OrdinalIgnoreCase))
            return;

        NativefyDatabaseProvider.SearchQueries.Add(query);
        NativefyDatabaseProvider.SaveSearchQueries();
    }
}
