namespace Nativefy.Services.LocalMetadata;

public interface ISearchQueriesService
{
    void SaveQuery(string query);
    string[] GetQueries();
    void Clear();
}
