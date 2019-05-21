using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Models;
using EasyNetQ;
using Email = Server.Models.Email;

namespace Server.Services
{
    public class SearchService
    {
        private readonly IBus _bus;
        private readonly IMapper _mapper;

        public SearchService(IBus bus, IMapper mapper)
        {
            _bus = bus;
            _mapper = mapper;
        }

        public async Task<SearchResults<Email>> Search(string searchText)
        {
            var searchResults =
                await _bus.RequestAsync<SearchRequest, SearchResults<Common.Models.Email>>(new SearchRequest {Text = searchText});
            var paths = searchResults.Results.Select(r => r.Result.Path).ToArray();
            var request = new ResultPreview.Request {path = paths};
            var previews =
                await _bus.RequestAsync<ResultPreview.Request, ResultPreview>(request);
            var emails = searchResults.Results.Join(previews.Results, r => r.Result.Path, r => r.path,
                (result, preview) =>
                {
                    var email = _mapper.Map<Email>(result.Result);
                    email.Body = preview.body;
                    return (result.Score, email);
                });
            return new SearchResults<Email> {Results = emails.ToList()};
        }
    }
}