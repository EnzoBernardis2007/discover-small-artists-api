using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
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
        private static string clientId = "14a01892998c454886e55c329d0c637b";  // Substitua pelo seu Client ID
        private static string clientSecret = "adf4dbcea7ed483696c0a15fa35bb9f4";  // Substitua pelo seu Client Secret

        [HttpGet("random-small-artist")]
        public async Task<IActionResult> GetRandomSmallArtist()
        {
            var spotify = new SpotifyClient(await GetSpotifyTokenAsync());

            var searchRequest = new SearchRequest(SearchRequest.Types.Artist, "artist")
            {
                Limit = 50 // Limita a busca a 50 resultados
            };
            var searchResults = await spotify.Search.Item(searchRequest);

            int followersLimit = 100000;

            var smallArtists = searchResults.Artists.Items
                .Where(a => a.Followers.Total < followersLimit)
                .ToList();

            if (!smallArtists.Any())
            {
                return NotFound(new { message = $"no artist with less than {followersLimit} followers." });
            }

            var random = new Random();
            var selectedArtist = smallArtists[random.Next(smallArtists.Count)];

            return Ok(new
            {
                Name = selectedArtist.Name,
                Followers = selectedArtist.Followers.Total,
                ImageUrl = selectedArtist.Images.FirstOrDefault()?.Url ?? "No image"
            });
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