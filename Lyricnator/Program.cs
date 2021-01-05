using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Genius;
using Genius.Models;
using Newtonsoft.Json;

namespace Lyricnator
{
    public static class Program
    {
        public static async Task Main()
        {
            var fullTitle = GetCurrentLocalSpotifyTrack().Split(" - ");
            var artist = fullTitle.First();
            var songTitle = string.Join(" ", fullTitle.Skip(1));
    
            var query = $"{songTitle} {artist}";
            var result = await GeniusApi.SearchSongByTitle(query);
    
            if (result == null)
            {
                throw new Exception("Couldn't connect to the API");
            }
    
            if (!result.Meta.Status[0].Equals('2'))
            {
                throw new Exception($"Couldn't find a song with title: {query}");
            }
    
    
            var hit = result.Response?.FirstOrDefault(h => h.Result.ToString().ToLower().Contains(artist.ToLower()));
            var song = JsonConvert.DeserializeObject<SongEntity>(hit?.Result.ToString());
    
            Console.WriteLine($"Opening the lyrics to {query}.....");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {song.Url}") {CreateNoWindow = true});
        }
    
        public static string GetCurrentLocalSpotifyTrack()
        {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(
                p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
    
            if (proc == null)
            {
                throw new Exception("Spotify is  not being executed at this moment");
            }
    
            if (string.Equals(proc.MainWindowTitle, "Spotify Premium", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Spotify is paused at this moment");
            }
    
            return proc.MainWindowTitle;
        }
    }
    
    public class GeniusApi
    {
        private static readonly GeniusClient GeniusClient = new GeniusClient(
            "bNcjvc81IcUfYnOI0DXr6lozaYOKhmsHp2qf30DUa7zhkulwrTntz4bTFPdKoEZK");
    
        public static Task<HttpResponse<Song>> GetSongById()
        {
            return GeniusClient.SongsClient.GetSong(TextFormat.Dom, "4727496");
        }
    
        public static async Task<HttpResponse<List<Hit>>> SearchSongByTitle(string title) =>
            await GeniusClient.SearchClient.Search(TextFormat.Dom, title);
    }
    
    public class SongEntity
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("full_title")] public string FullTitle { get; set; }
        [JsonProperty("title_with_featured")] public string TitleWithFeatured { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("primary_artist")] public ArtistEntity PrimaryArtist { get; set; }
    }
    
    public class ArtistEntity
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}