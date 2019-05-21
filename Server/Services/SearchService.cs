using System.Threading.Tasks;
using Common.Models;
using EasyNetQ;

namespace Server.Services
{
    public class SearchService
    {
        private readonly IBus _bus;

        public SearchService(IBus bus)
        {
            _bus = bus;
        }

        public async Task<SearchResults> Search(string searchText)
        {
            return await _bus.RequestAsync<SearchRequest, SearchResults>(new SearchRequest {Text = searchText});
        }
    }
}