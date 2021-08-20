namespace Common.Services.DataBase.Interfaces
{
    public interface ISearchResultReciever
    {
        public void Recieve(SearchResult searchResult);

        public SearchResult[] ViewResults();

        public bool TryDequeueResult(out SearchResult searchResult);

        public bool IsComplited { get; set; }
    }
}
