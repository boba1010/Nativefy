namespace Nativefy.Services;

public interface ISearchQueriesService
{
    void SaveQuery(string query);
    string[] GetQueries();
}
