using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Models;
using EasyNetQ;
using MoreLinq;
using Polly;
using Server.Models;
using Email = Common.Models.Email;

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

        public async Task<Search> Search(string searchText)
        {
            var results = Task.Run(async () =>
            {
                var searchResults = await Policies<SearchResults<Email>>.Complete.ExecuteAsync(async c =>
                    await _bus.RequestAsync<SearchRequest, SearchResults<Email>>(new SearchRequest
                    {
                        Text = c.OperationKey
                    }), new Context(searchText));

                var paths = searchResults.Results.Select(r => r.Result.Path).ToArray();
                var request = new ResultPreview.Request {path = paths};
                var previews = await Policies<ResultPreview>.Complete
                    .WrapAsync(Policy<ResultPreview>.Handle<TimeoutException>().FallbackAsync(new ResultPreview
                        {Results = new (string path, string body)[0]}))
                    .ExecuteAsync(
                        async c =>
                            await _bus.RequestAsync<ResultPreview.Request, ResultPreview>(request),
                        new Context(string.Join(",", request.path)));

                var emails =
                    searchResults.Results.LeftJoin(
                        previews.Results,
                        r => r.Result.Path,
                        r => r.path,
                        old =>
                        {
                            var n = _mapper.Map<Models.Email>(old.Result);
                            n.Body = "Service not available";
                            return (old.Score,n);
                        },
                        (result, preview) =>
                        {
                            var email = _mapper.Map<Models.Email>(result.Result);
                            email.Body = preview.body;
                            return (result.Score, email);
                        });
                return new SearchResults<Models.Email> {Results = emails.ToList()};
            });

            var spellings = Policies<Spellings>.Complete
                .WrapAsync(Policy<Spellings>.Handle<TimeoutException>()
                    .FallbackAsync(new Spellings {spellings = new string[0]})).ExecuteAsync(async () =>
                    await _bus.RequestAsync<Spellings.Request, Spellings>(new Spellings.Request {Text = searchText}));

            await Task.WhenAll(results, spellings);

            return new Search((await spellings).spellings, await results);
        }
    }
}