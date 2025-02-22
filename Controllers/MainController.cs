using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using Microsoft.Extensions.Configuration; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discover_small_artists_api.Controllers
{
    [ApiController]
    [Route("api")]
    public class MainController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private string clientId;
        private string clientSecret;

        public MainController(IConfiguration configuration)
        {
            _configuration = configuration;
            clientId = _configuration["Spotify:ClientId"];
            clientSecret = _configuration["Spotify:ClientSecret"];
        }

        [HttpGet("random-small-artists")]
        public async Task<IActionResult> GetRandomSmallArtists()
        {
            var spotify = new SpotifyClient(await GetSpotifyTokenAsync());

            var searchRequest = new SearchRequest(SearchRequest.Types.Artist, ((char)new Random().Next('a', 'z' + 1)).ToString())
            {
                Limit = 50,
                Offset = new Random().Next(0, 950) 
            };

            var searchResults = await spotify.Search.Item(searchRequest);

            int followersLimit = 100000;

            var smallArtists = searchResults.Artists.Items
                .Where(a => a.Followers.Total < followersLimit)
                .ToList();

            if (!smallArtists.Any())
            {
                return NotFound(new { message = $"No artists found with less than {followersLimit} followers." });
            }

            var random = new Random();
            var randomArtists = smallArtists.OrderBy(x => random.Next()).Take(10).ToList();

            var tasks = new List<Task<object>>();

            foreach (var artist in randomArtists)
            {
                tasks.Add(GetArtistDataAsync(spotify, artist.Id));
            }

            var results = await Task.WhenAll(tasks);

            return Ok(results);
        }

        private async Task<object> GetArtistDataAsync(SpotifyClient spotify, string artistId)
        {
            var artist = await spotify.Artists.Get(artistId);
            var topTracksRequest = new ArtistsTopTracksRequest("US");
            var topTracks = await spotify.Artists.GetTopTracks(artistId, topTracksRequest);

            var topTrackInfo = topTracks.Tracks.Take(5).Select(track => new
            {
                Name = track.Name,
                Id = track.Id,
                ImageUrl = track.Album.Images.FirstOrDefault()?.Url ?? "No image"
            }).ToList();

            return new
            {
                Name = artist.Name,
                Followers = artist.Followers.Total,
                ImageUrl = artist.Images.FirstOrDefault()?.Url ?? "No image",
                TopTracks = topTrackInfo
            };
        }

        private async Task<string> GetSpotifyTokenAsync()
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(clientId, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);

            return response.AccessToken;
        }
    }
}
